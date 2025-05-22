import os
import json
import pandas as pd
import numpy as np
import torch
import torch.nn as nn   
import torch.optim as optim
import pdfplumber
import networkx as nx
import nltk

from langdetect import detect
from PyPDF2 import PdfReader
from sklearn.feature_extraction.text import TfidfVectorizer
from transformers import LayoutLMv3Processor, LayoutLMv3ForTokenClassification, AutoModel, AutoTokenizer, pipeline
from neo4j import GraphDatabase
from pgmpy.models import BayesianNetwork
from pgmpy.estimators import MaximumLikelihoodEstimator
from pgmpy.inference import VariableElimination

# Define folder path
predicted_folder = os.path.join(os.getcwd(), 'ParticipantPredictedResults')
resources_folder = os.path.join(os.getcwd(), 'Resources')
measured_folder = os.path.join(os.getcwd(), 'NormalizedData')
LOCAL_DATA_FILE = os.path.join(os.getcwd(), "local_knowledge_data.json")
LOCAL_DATA_GRAPH = os.path.join(os.getcwd(), "local_knowledge_graph.json")

################ Maps ###############################
mapping = {
    "Urgency": "Urgency",
    "Lack of premeditation": "Lack of premeditation",
    "Lack of perseverance": "Lack of perseverance",
    "Sensation seeking": "Sensation seeking",
    "Extraversion": "Extraversion",
    "Agreeableness": "Verträglichkeit",
    "Conscientiousness": "Gewissenhaftigkeit",
    "Neuroticism": "Negative Emotionalität",
    "Openness": "Offenheit",
    "Sociability": "Geselligkeit",
    "Assertiveness": "Durchsetzungsfähigkeit",
    "Activity": "Aktivität",
    "Compassion": "Mitgefühl",
    "Politeness": "Höflichkeit",
    "Interpersonal Trust": "Zwischenmenschliches Vertrauen",
    "Orderliness": "Ordnungsliebe",
    "Diligence": "Fleiß",
    "Reliability": "Verlässlichkeit",
    "Anxiety": "Ängstlichkeit",
    "Depression": "Niedergeschlagenheit",
    "Emotional Instability": "Unbeständigkeit der Gefühle",
    "Aesthetic Sensitivity": "Ästhetisches Empfinden",
    "Intellectual Curiosity": "Intellektuelle Neugierde",
    "Creative Imagination": "Kreativer Einfallsreichtum",
    "Attention score": "Aufmerksamkeit",
    "Cognitive Instability score": "Kognitive Instabilität",
    "Motor Scores": "Motorische Impulsivität",
    "Perseverance scores": "Beharrlichkeit",
    "Self-Control scores": "Selbst Kontrolle",
    "Cognitive Complexity scores": "Kognitive Komplexität"
}
define_ranges = {
    # UPPS Scores
    "Urgency": (11, 44),
    "Lack of premeditation": (10, 40),
    "Lack of perseverance": (10, 40),
    "Sensation seeking": (12, 48),

    # Big Five Traits
    "Extraversion": (12, 60),
    "Verträglichkeit": (12, 60),
    "Gewissenhaftigkeit": (12, 60),
    "Negative Emotionalität": (12, 60),
    "Offenheit": (12, 60),

    # Big Five Facets
    "Geselligkeit": (4, 20),
    "Durchsetzungsfähigkeit": (4, 20),
    "Aktivität": (4, 20),
    "Mitgefühl": (4, 20),
    "Höflichkeit": (4, 20),
    "Zwischenmenschliches Vertrauen": (4, 20),
    "Ordnungsliebe": (4, 20),
    "Fleiß": (4, 20),
    "Verlässlichkeit": (4, 20),
    "Ängstlichkeit": (4, 20),
    "Niedergeschlagenheit": (4, 20),
    "Unbeständigkeit der Gefühle": (4, 20),
    "Ästhetisches Empfinden": (4, 20),
    "Intellektuelle Neugierde": (4, 20),
    "Kreativer Einfallsreichtum": (4, 20),

    # BIS-11 Scores
    "Aufmerksamkeit": (0, 20),
    "Kognitive Instabilität": (0, 12),
    "Motorische Impulsivität": (0, 28),
    "Beharrlichkeit": (0, 16),
    "Selbst Kontrolle": (0, 24),
    "Kognitive Komplexität": (0, 20)
}

################ End Maps ############################

# Ensure folders exist
def ensure_folders_exist():
    if not os.path.exists(predicted_folder):
        raise FileNotFoundError("Folder does not exist.")

# Load predicted JSON file
def load_predicted_json(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)

        if isinstance(data, dict):
            return data  # Return as-is if already in mapped format

        elif isinstance(data, list) and len(data) > 1:
            header = data[0]  # First row contains score names
            values = data[-1]  # Last row contains WholeGame scores
            
            mapped_header = [mapping.get(col, col) for col in header]
            return {mapped_header[i]: values[i] for i in range(1, len(header))}
        
        else:
            print(f"Unexpected format in predicted file: {file_path}")
            return None

    except Exception as e:
        print(f"Error reading predicted JSON file '{file_path}': {e}")
        return None

# Load measured data
def load_measured_data():
    measured_matrix = []
    header_row = ["Filename"]
    
    for filename in os.listdir(measured_folder):
        if filename.endswith(".json"):
            file_path = os.path.join(measured_folder, filename)
            with open(file_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            if "Rows" not in data:
                continue
            
            extracted_data = []
            for row in data["Rows"]:
                for col_index in range(1, 10, 2):  # Odd-numbered columns contain feature names
                    feature_name = row.get(f"Column{col_index}", "").strip()
                    feature_value = row.get(f"Column{col_index+1}", "")
                    if isinstance(feature_value, (float,int)):
                        feature_value = str(feature_value)
                        feature_value = feature_value.strip()
                    if not feature_name:
                        continue
                    
                    if feature_name.endswith("AndSaved?") or feature_name.endswith("ndKilled?") or feature_name.endswith("Video"):
                        feature_value = 1 if feature_value.lower() in ["yes", "true", "1"] else 0
                    elif feature_value.replace(".", "").isdigit():
                        feature_value = float(feature_value)
                    elif "\n" in feature_value:
                        for chunk in feature_value.split("\n"):
                            extracted_data.append([feature_name, chunk.strip()])
                        continue
                    extracted_data.append([feature_name, feature_value])
            
            if not header_row[1:]:
                header_row.extend([x[0] for x in extracted_data])
                measured_matrix.append(header_row)
            measured_matrix.append([os.path.splitext(filename)[0]] + [x[1] for x in extracted_data])
    
    return measured_matrix

##################### RAG Pipeline ##################################
nltk.download('punkt_tab')
nltk.download('averaged_perceptron_tagger_eng')
# Load NLP Models avoided spacy for installation issues trying with senza standford failed trying from huggingface
nlp_en = pipeline("ner", model="dbmdz/bert-large-cased-finetuned-conll03-english")
nlp_de = pipeline("ner", model="mschiesser/ner-bert-german")

# For dependency parsing & NER
scibert_tokenizer = AutoTokenizer.from_pretrained("allenai/scibert_scivocab_uncased")
scibert_model = AutoModel.from_pretrained("allenai/scibert_scivocab_uncased")

# Load LayoutLMv3 for Table Extraction
layoutlm_processor = LayoutLMv3Processor.from_pretrained("microsoft/layoutlmv3-base")
layoutlm_model = LayoutLMv3ForTokenClassification.from_pretrained("microsoft/layoutlmv3-base")

# Neo4j Connection
NEO4J_URI = "bolt://localhost:7687"
NEO4J_USER = "neo4j"
NEO4J_PASSWORD = "20041995"
driver = GraphDatabase.driver(NEO4J_URI, auth=(NEO4J_USER, NEO4J_PASSWORD))

# Read and process PDFs for RAG learning
def extract_text_tables_from_pdf(pdf_path):
    reader = PdfReader(pdf_path)
    text = ""
    for page in reader.pages:
        text += page.extract_text() + "\n"
    return text

# Extract entities and relationships
def extract_ner_relations(text):

    all_entities = []
    all_relations = []
    segments = text.split("\n")
    
    for segment in segments:
        segment = segment.strip()
        if not segment:
            continue
        
        try:
            lang = detect(segment)
        except Exception:
            lang = 'en'  # Fallback to English if detection fails
        
        # Route the segment to the appropriate NER pipeline
        if lang == 'de':
            doc = nlp_de(segment)
        else:
            doc = nlp_en(segment)
        
        # Collect entities (assuming each pipeline returns a similar structure)
        entities = [(ent['word'] if isinstance(ent, dict) else ent.text, ent['entity'] if isinstance(ent, dict) else ent.label_) for ent in doc]
        all_entities.extend(entities)
        
        # Use NLTK for sentence splitting and POS tagging
        sentences = nltk.sent_tokenize(segment)
        for sent in sentences:
            tokens = nltk.word_tokenize(sent)
            pos_tags = nltk.pos_tag(tokens)
            
            # Identify the first verb (ROOT candidate)
            verb_index = None
            for idx, (word, tag) in enumerate(pos_tags):
                if tag.startswith("VB"):
                    verb_index = idx
                    break
            
            if verb_index is not None:
                # Look backwards for a subject (noun)
                subject = None
                for idx in range(verb_index - 1, -1, -1):
                    if pos_tags[idx][1].startswith("NN"):
                        subject = pos_tags[idx][0]
                        break
                
                # Look forward for an object (noun)
                obj = None
                for idx in range(verb_index + 1, len(pos_tags)):
                    if pos_tags[idx][1].startswith("NN"):
                        obj = pos_tags[idx][0]
                        break
                
                # If both subject and object are found, add the relation
                if subject and obj:
                    relation = pos_tags[verb_index][0].lower()  # use the verb as the relation
                    all_relations.append((subject, obj, relation))
    
    print(f"Total relations extracted from segment: {len(all_relations)}")
    return all_entities, all_relations

# Extract tables using LayoutLMv3
def extract_tables_from_pdf(pdf_path):
    tables = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            extracted_tables = page.extract_table()
            if extracted_tables:
                for table in extracted_tables:
                    tables.append(table)
    return tables


# check if Neo4j already contains knowledge
def check_neo4j_cache():
    with driver.session() as session:
        result = session.run("MATCH (n) RETURN count(n) as count")
        count = result.single()["count"]
    return count

def check_neo4j_edges():
    with driver.session() as session:
        result = session.run("MATCH ()-[r]->() RETURN count(r) as count")
        edge_count = result.single()["count"]
    return edge_count

# Fetch existing data from Neo4j (if needed)
def fetch_neo4j_data():
    with driver.session() as session:
        result = session.run("MATCH (n) RETURN n")
        nodes = [record["n"] for record in result]
    return nodes

# Store knowledge in Neo4j
def store_knowledge_in_neo4j(entities, relations):
    try:
        with driver.session() as session:
            for entity, label in entities:
                query = f"MERGE (n:`{label}` {{name: $name}})"
                session.run(query, name=entity)
            for subj, obj, rel in relations:
                query = f"MATCH (a {{name: $subj}}), (b {{name: $obj}}) MERGE (a)-[:`{rel}`]->(b)"
                session.run(query, subj=subj, obj=obj)
        print("Knowledge graph successfully pushed to Neo4j.")
    except Exception as e:
        print("Error storing knowledge in Neo4j:", e)
        # Save locally so that you can push later without reprocessing PDFs
        data = {"all_entities": entities, "all_relations": relations}
        try:
            with open(LOCAL_DATA_GRAPH, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=4, ensure_ascii=False)
            print(f"Saved local knowledge graph to {LOCAL_DATA_GRAPH}.")
        except Exception as save_err:
            print("Error saving local knowledge graph:", save_err)



# Learn causal relationships using Bayesian Networks
def learn_bayesian_network(relations):
    edges = [(subj, obj) for subj, obj, rel in relations if subj != obj]
    G = nx.DiGraph(edges)
    G.add_edges_from(edges)

    # Detect and break cycles until the graph is acyclic.
    cycles = list(nx.simple_cycles(G))
    while cycles:
        for cycle in cycles:
            # For each cycle, remove one edge.
            # Here we remove the first edge in the cycle to break it.
            if len(cycle) > 1:
                edge_to_remove = (cycle[0], cycle[1])
                if G.has_edge(*edge_to_remove):
                    G.remove_edge(*edge_to_remove)
                    print(f"Removed edge {edge_to_remove} to break cycle {cycle}")
        cycles = list(nx.simple_cycles(G))
    bn_model = BayesianNetwork(G.edges())
    bn_model.fit(pd.DataFrame(edges, columns=["Cause", "Effect"]), estimator=MaximumLikelihoodEstimator)
    return bn_model

# Fine-tune SciBERT on extracted knowledge force truncation/padding to max_length=512
def fine_tune_scibert(knowledge_base):
    inputs = scibert_tokenizer(
        knowledge_base,
        return_tensors="pt",
        truncation=True,
        padding="max_length",  # Pad to the maximum length
        max_length=512         # Fixed maximum sequence length
    )
    with torch.no_grad():
        outputs = scibert_model(**inputs).last_hidden_state.mean(dim=1)  # Sentence embeddings
    return outputs.numpy()

def process_pdfs(force_reprocess=False):
    # If not forcing reprocessing and cache files exist, load them
    if not force_reprocess and os.path.exists(LOCAL_DATA_FILE) and os.path.exists(LOCAL_DATA_GRAPH):
        try:
            with open(LOCAL_DATA_FILE, "r", encoding="utf-8") as f:
                knowledge_base = f.read()
            with open(LOCAL_DATA_GRAPH, "r", encoding="utf-8") as f:
                graph_data = json.load(f)
            all_entities = graph_data.get("all_entities", [])
            all_relations = graph_data.get("all_relations", [])
            print("Loaded cached PDF processing data.")
            return knowledge_base, all_entities, all_relations
        except Exception as e:
            print("Error loading cached data:", e)
            print("Reprocessing PDFs...")
    # Reprocess PDFs from scratch
    knowledge_base = ""
    all_entities = []
    all_relations = []
    for filename in os.listdir(resources_folder):
        if filename.endswith(".pdf"):
            pdf_path = os.path.join(resources_folder, filename)
            text = extract_text_tables_from_pdf(pdf_path)
            entities, relations = extract_ner_relations(text)
            tables = extract_tables_from_pdf(pdf_path)
            
            tables_str = "\n".join([str(table) for table in tables])
            knowledge_base += text + "\n" + tables_str + "\n"

            all_entities.extend(entities)
            all_relations.extend(relations)
            print(f"Processed: {filename}")

    # Save processed data for future use
    try:
        with open(LOCAL_DATA_FILE, "w", encoding="utf-8") as f:
            f.write(knowledge_base)
        with open(LOCAL_DATA_GRAPH, "w", encoding="utf-8") as f:
            json.dump({"all_entities": all_entities, "all_relations": all_relations}, 
                      f, indent=4, ensure_ascii=False)
        print("Saved processed PDF data locally.")
    except Exception as e:
        print("Error saving local PDF data:", e)

    return knowledge_base, all_entities, all_relations

def learn_from_pdfs():
    try:
        # Try to check Neo4j for existing data.
        node_count = check_neo4j_cache()
        edge_count = check_neo4j_edges()
        print(f"Neo4j node count: {node_count}")
        print(f"Neo4j edge count: {edge_count}")
    except Exception as e:
        print("Neo4j connection failed:", e)
        # Fall back to local files if available.
        if os.path.exists(LOCAL_DATA_FILE) and os.path.exists(LOCAL_DATA_GRAPH):
            try:
                with open(LOCAL_DATA_FILE, "r", encoding="utf-8") as f:
                    knowledge_base = f.read()
                with open(LOCAL_DATA_GRAPH, "r", encoding="utf-8") as f:
                    graph_data = json.load(f)
                all_entities = graph_data.get("all_entities", [])
                all_relations = graph_data.get("all_relations", [])
                if not all_relations:
                    raise ValueError("Local graph data does not contain any edges.")
                print("Loaded knowledge data from local cache files.")
                bayesian_model = learn_bayesian_network(all_relations)
                fine_tuned_scibert = fine_tune_scibert(knowledge_base)
                return knowledge_base, bayesian_model, fine_tuned_scibert
            except Exception as load_err:
                print("Error loading local cache:", load_err)
                print("Reprocessing PDFs...")
        else:
            print("Local cache files not found. Reprocessing PDFs...")
        # Reprocess PDFs if local cache isn't available or valid.
        knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
        store_knowledge_in_neo4j(all_entities, all_relations)
        bayesian_model = learn_bayesian_network(all_relations)
        fine_tuned_scibert = fine_tune_scibert(knowledge_base)
        return knowledge_base, bayesian_model, fine_tuned_scibert

    # Neo4j connection was successful.
    if node_count > 0:
        # If Neo4j has nodes but no edges, try to push edges from local file.
        if edge_count == 0:
            print("Neo4j contains nodes but no edges. Attempting to push edges from local cache...")
            if os.path.exists(LOCAL_DATA_GRAPH):
                try:
                    with open(LOCAL_DATA_GRAPH, "r", encoding="utf-8") as f:
                        graph_data = json.load(f)
                    all_relations = graph_data.get("all_relations", [])
                    all_entities = graph_data.get("all_entities", [])
                    if all_relations:
                        store_knowledge_in_neo4j(all_entities, all_relations)
                        print("Successfully pushed edges to Neo4j.")
                    else:
                        print("Local cache does not contain edges. Will use local file data.")
                except Exception as push_err:
                    print("Failed to push edges from local cache:", push_err)
                    print("Continuing with local file data.")
            else:
                print("Local graph file not found. Cannot push edges. Continuing with local file data.")
            # Now attempt to load local cache data
            if os.path.exists(LOCAL_DATA_FILE) and os.path.exists(LOCAL_DATA_GRAPH):
                try:
                    with open(LOCAL_DATA_FILE, "r", encoding="utf-8") as f:
                        knowledge_base = f.read()
                    with open(LOCAL_DATA_GRAPH, "r", encoding="utf-8") as f:
                        graph_data = json.load(f)
                    all_entities = graph_data.get("all_entities", [])
                    all_relations = graph_data.get("all_relations", [])
                    if not all_relations:
                        raise ValueError("Local graph data does not contain any edges.")
                    print("Loaded knowledge data from local cache files.")
                except Exception as e:
                    print("Error loading local cache:", e)
                    print("Reprocessing PDFs...")
                    knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
            else:
                print("Local cache files not found. Processing PDFs...")
                knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
        else:
            # Neo4j has both nodes and edges.
            print("Neo4j already contains complete knowledge (nodes and edges). Attempting to load local cache...")
            if os.path.exists(LOCAL_DATA_FILE) and os.path.exists(LOCAL_DATA_GRAPH):
                try:
                    with open(LOCAL_DATA_FILE, "r", encoding="utf-8") as f:
                        knowledge_base = f.read()
                    with open(LOCAL_DATA_GRAPH, "r", encoding="utf-8") as f:
                        graph_data = json.load(f)
                    all_entities = graph_data.get("all_entities", [])
                    all_relations = graph_data.get("all_relations", [])
                    if not all_relations:
                        raise ValueError("Local graph data does not contain any edges.")
                    print("Loaded knowledge data from local cache files.")
                except Exception as e:
                    print("Error loading local cache:", e)
                    print("Reprocessing PDFs...")
                    knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
            else:
                print("Local cache files not found. Processing PDFs...")
                knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
    else:
        # Neo4j has no nodes.
        print("Neo4j has no nodes. Processing PDFs...")
        knowledge_base, all_entities, all_relations = process_pdfs(force_reprocess=True)
        store_knowledge_in_neo4j(all_entities, all_relations)

    bayesian_model = learn_bayesian_network(all_relations)
    fine_tuned_scibert = fine_tune_scibert(knowledge_base)
    return knowledge_base, bayesian_model, fine_tuned_scibert


def get_bayesian_probability(bayesian_model, score, evidence=None):
    """
    Computes a robust causal probability for a given score using Bayesian inference.
    
    Args:
        bayesian_model: A pgmpy BayesianNetwork object with learned CPDs.
        score (str): The score variable for which the probability is computed.
        evidence (dict, optional): Additional evidence (if available) as a dictionary.
        
    Returns:
        float: The marginal probability for the score.
    """
    try:
        # Create an inference object using Variable Elimination
        infer = VariableElimination(bayesian_model)
        # Query the network for the marginal probability of the score given evidence
        query_result = infer.query(variables=[score], evidence=evidence)
        # query_result is a DiscreteFactor, and you can select the probability of the most likely state,
        # or use an expectation. Here we simply take the maximum probability as a placeholder.
        probability = float(query_result.values.max())
    except Exception as e:
        print(f"Error computing marginal probability for {score}: {e}. Using default probability 0.5.")
        probability = 0.5
    return probability

def compute_combined_influence(score, knowledge_base, bayesian_model):
    """
    Computes the combined influence for a given score by merging TF-IDF-based influence
    with Bayesian network outputs.
    
    Args:
      score (str): The score or feature name.
      knowledge_base (str): The combined text and tables extracted from PDFs.
      bayesian_model: The Bayesian network constructed from the extracted relations.
    
    Returns:
      float: Combined influence value.
    """
    # Compute the TF-IDF influence as before
    tfidf_influence = compute_tfidf_influence(score, knowledge_base)
    bayesian_weight = get_bayesian_probability(bayesian_model, score)
    combined_influence = tfidf_influence * (1 + bayesian_weight)
    return combined_influence

def compute_tfidf_influence(score, knowledge_base):
    vectorizer = TfidfVectorizer()
    tfidf_matrix = vectorizer.fit_transform(knowledge_base.split("\n"))
    feature_names = vectorizer.get_feature_names_out()
    if score in feature_names:
        return tfidf_matrix[:, feature_names.tolist().index(score)].sum()
    return 0

def integrate_knowledge(knowledge_data):
    vectorizer = TfidfVectorizer()
    knowledge_matrix = vectorizer.fit_transform([knowledge_data]).toarray()
    return knowledge_matrix.flatten()

##################### End RAG pipeline ##################################

########### ScoresXMeasures Correlation pipeline ##############

# Neural Network for Learning Correlations
class CorrelationLearner(nn.Module):
    def __init__(self, input_size, output_size):
        super(CorrelationLearner, self).__init__()
        self.linear = nn.Linear(input_size, output_size)
    
    def forward(self, x):
        return torch.sigmoid(self.linear(x))

def convert_measured_values(measured_values):
    """
    Convert a dictionary or list of measured values (which may include strings or lists)
    into a list of numeric values. For string entries, we assign a unique float 
    for each unique string. If a value is a list, we attempt to convert its elements
    to float and compute their average.
    """
    mapping_dict = {}
    numeric_values = []

    def process_value(val):
        # If the value is a list, attempt to convert its items to float and average them.
        if isinstance(val, list):
            try:
                float_list = [float(item) for item in val]
                return sum(float_list) / len(float_list) if float_list else 0.0
            except Exception:
                # Fallback: treat the list as a string for mapping.
                val_str = str(val)
                if val_str not in mapping_dict:
                    mapping_dict[val_str] = float(len(mapping_dict) + 1)
                return mapping_dict[val_str]
        else:
            try:
                return float(val)
            except Exception:
                val_str = str(val)
                if val_str not in mapping_dict:
                    mapping_dict[val_str] = float(len(mapping_dict) + 1)
                return mapping_dict[val_str]

    if isinstance(measured_values, dict):
        for key, value in measured_values.items():
            numeric_values.append(process_value(value))
    elif isinstance(measured_values, list):
        for value in measured_values:
            numeric_values.append(process_value(value))
    else:
        raise ValueError("measured_values must be either a dict or a list")
    
    return numeric_values


# Train the CorrelationLearner model using structured knwoledge
def train_correlation_learner(measured_values, predicted_values, knowledge_data, fine_tuned_scibert, epochs=500, threshold=0.05):
    """
    Trains the CorrelationLearner using a combined input that includes:
      - measured values,
      - structured knowledge (e.g., TF-IDF vector),
      - fine-tuned SciBERT embeddings.
    """
    # Convert measured_values (a dict) to a list of numeric values.
    numeric_measured = convert_measured_values(measured_values)
    measured_tensor = torch.tensor([numeric_measured], dtype=torch.float32)
    
    knowledge_tensor = torch.tensor(knowledge_data, dtype=torch.float32).repeat(measured_tensor.shape[0], 1)
    scibert_tensor = torch.tensor(fine_tuned_scibert, dtype=torch.float32).repeat(measured_tensor.shape[0], 1)
    
    # Concatenate all features to form a comprehensive input
    input_tensor = torch.cat((measured_tensor, knowledge_tensor, scibert_tensor), dim=1)

    input_size = input_tensor.shape[1]
    output_size = len(predicted_values)  # Assumes predicted_values is a list or dict of score values

    model = CorrelationLearner(input_size, output_size)
    optimizer = optim.Adam(model.parameters(), lr=0.01)
    loss_fn = nn.MSELoss()
    correlation_tensor = torch.tensor(predicted_values, dtype=torch.float32)
    
    for epoch in range(epochs):
        optimizer.zero_grad()
        output = model(input_tensor)
        loss = loss_fn(output, correlation_tensor)
        loss.backward()
        optimizer.step()
        if loss.item() < threshold:
            break
    
    return model

############### End  ScoresXMeasures Correlation pipeline####################

##############################      START  RLAIF                   ############################################
# Reinforcement Learning-based Adjustment of Impulsivity Factors (RLAIF)
def apply_rlaif(predicted_scores, measured_values, model, knowledge_base, bayesian_model):
    """
    Adjusts each predicted score individually by:
      1. Getting the corresponding adjustment factor from the trained model.
      2. Computing a combined influence that merges TF-IDF influence with Bayesian network insights.
      3. Updating the score based on these factors.
    
    Args:
      predicted_scores (dict): Mapping of score names to their predicted values.
      measured_values (dict): Mapping of measured feature names to their values.
      model: Trained CorrelationLearner model.
      knowledge_base (str): The text-based knowledge extracted from PDFs.
      bayesian_model: The Bayesian network constructed from extracted relationships.
    
    Returns:
      dict: Adjusted scores.
    """

    adjusted_scores = {}
    numeric_features = []
    
    # Convert measured values to floats where possible; use 0.0 if conversion fails.
    for key, value in measured_values.items():
        try:
            numeric_features.append(float(value))
        except Exception:
            numeric_features.append(0.0)
    
    measured_tensor = torch.tensor([numeric_features], dtype=torch.float32)
    predicted_adjustments = model(measured_tensor).detach().numpy().flatten()
    
    for i, (score, value) in enumerate(predicted_scores.items()):
        adjustment = predicted_adjustments[i] if i < len(predicted_adjustments) else 0.0
        combined_influence = compute_combined_influence(score, knowledge_base, bayesian_model)
        adjusted_value = value + adjustment * combined_influence
        
        if score in define_ranges:
            range_min, range_max = define_ranges[score]
            center = (range_min + range_max) / 2
            scale = (range_max - range_min) / 4
            adjusted_value = range_min + (range_max - range_min) / (1 + np.exp(-(adjusted_value - center) / scale))
        
        adjusted_scores[score] = adjusted_value
        
    return adjusted_scores
  
##########################          END RLAIF       ##########################


######################### START preprocessing Phase ##########################

def training_phase():
    # Preprocess PDFs, extract knowledge, and get pretrained outputs
    knowledge_base, bayesian_model, fine_tuned_scibert = learn_from_pdfs()
    knowledge_data = integrate_knowledge(knowledge_base)
    measured_matrix = load_measured_data()
    
    # For demonstration, use the first valid predicted JSON file for training
    predicted_data = None
    for filename in os.listdir(predicted_folder):
        predicted_path = os.path.join(predicted_folder, filename)
        predicted_data = load_predicted_json(predicted_path)
        if predicted_data is not None:
            break
    if predicted_data is None:
        raise ValueError("No valid predicted file found for training.")
    
    # Convert predicted scores into a list (ensure the order matches your mapping)
    predicted_values = [predicted_data.get(key, 0) for key in mapping.values()]
    
    # Train the correlation learner with the combined inputs
    correlation_model = train_correlation_learner(
        measured_values=measured_matrix[1:], 
        predicted_values=predicted_values, 
        knowledge_data=knowledge_data, 
        fine_tuned_scibert=fine_tuned_scibert
    )
    
    return knowledge_base, bayesian_model, fine_tuned_scibert, knowledge_data, measured_matrix, correlation_model
######################### END preprocessing Phase ############################
    
######################### START Processing Phase #############################
def processing_phase(knowledge_base, bayesian_model, fine_tuned_scibert, knowledge_data, measured_matrix, correlation_model):
    predicted_matrix = []
    enhanced_matrix = []
    header_row = ["Filename"] + list(mapping.values())
    predicted_matrix.append(header_row)
    enhanced_matrix.append(header_row)
    
    # Iterate over each predicted file
    for filename in os.listdir(predicted_folder):
        predicted_path = os.path.join(predicted_folder, filename)
        predicted_data = load_predicted_json(predicted_path)
        if predicted_data is None:
            print(f"Skipping: {filename}")
            continue
        
        # Optionally, update the JSON file as needed
        with open(predicted_path, 'w', encoding='utf-8') as f:
            json.dump(predicted_data, f, indent=4, ensure_ascii=False)
        
        # Build row for predicted matrix
        score_values = [predicted_data.get(key, "N/A") for key in mapping.values()]
        predicted_matrix.append([os.path.splitext(filename)[0]] + score_values)
        
        # Retrieve the measured values for this file by matching filenames in measured_matrix
        measured_values = {}
        for row in measured_matrix[1:]:
            if row[0] == os.path.splitext(filename)[0]:
                for i, col in enumerate(measured_matrix[0][1:]):
                    measured_values[col] = row[i+1]
                break
        
        # Adjust scores using the revised RLAIF function
        adjusted_scores = apply_rlaif(
            predicted_scores=predicted_data, 
            measured_values=measured_values, 
            model=correlation_model, 
            knowledge_base=knowledge_base, 
            bayesian_model=bayesian_model
        )
        score_values_adjusted = [adjusted_scores.get(key, "N/A") for key in mapping.values()]
        enhanced_matrix.append([os.path.splitext(filename)[0]] + score_values_adjusted)
    
    visualize_measured_data(enhanced_matrix)
############################# END Processing Phase ##########################

def visualize_measured_data(matrix, output_path="enhanced_data.xlsx"):
    if len(matrix) <= 1:
        print("No data to save.")
        return

    # Ensure all rows have the same number of columns
    max_cols = max(len(row) for row in matrix)
    matrix = [row + [''] * (max_cols - len(row)) for row in matrix]  # Pad shorter rows

    # Convert matrix to DataFrame
    df = pd.DataFrame(matrix[1:], columns=matrix[0])

    # Save to Excel file
    df.to_excel(output_path, index=False, engine='openpyxl')
    print(f"Data saved as {output_path}")

def main():
    # Ensure necessary folders exist
    ensure_folders_exist()
    
    # Training Phase: Build knowledge base and train the correlation learner
    knowledge_base, bayesian_model, fine_tuned_scibert, knowledge_data, measured_matrix, correlation_model = training_phase()
    
    # Processing Phase: Process each predicted file and adjust scores accordingly
    processing_phase(knowledge_base, bayesian_model, fine_tuned_scibert, knowledge_data, measured_matrix, correlation_model)

if __name__ == "__main__":
    main()

#process_files()
#matrix=load_measured_data()
#visualize_measured_data(matrix)


