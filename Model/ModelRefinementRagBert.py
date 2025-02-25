import os
import json
import numpy as np
import pandas as pd
import xgboost as xgb
import torch
import dill as pickle
from sentence_transformers import SentenceTransformer
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from transformers import AutoModel, AutoTokenizer
from langchain_community.document_loaders import PyPDFLoader
#from langchain_community.embeddings import HuggingFaceEmbeddings
from langchain_huggingface import HuggingFaceEmbeddings
from transformers import AutoModelForSequenceClassification, BitsAndBytesConfig

# Define input and output directories
INPUT_FOLDER = 'NormalizedData'
OUTPUT_FOLDER = 'ParticipantPredictedResultsRagBert'
RESOURCES_FOLDER = 'Resources'  # PDFs for RAG
os.makedirs(OUTPUT_FOLDER, exist_ok=True)

# Define score ranges
SCORE_RANGES = {  # Same as before
    "Urgency": (11, 44), "Lack of premeditation": (10, 40), "Lack of perseverance": (10, 40), "Sensation seeking": (12, 48),
    "Extraversion": (12, 60), "Verträglichkeit": (12, 60), "Gewissenhaftigkeit": (12, 60), "Negative Emotionalität": (12, 60), "Offenheit": (12, 60),
    "Geselligkeit": (4, 20), "Durchsetzungsfähigkeit": (4, 20), "Aktivität": (4, 20), "Mitgefühl": (4, 20), "Höflichkeit": (4, 20),
    "Zwischenmenschliches Vertrauen": (4, 20), "Ordnungsliebe": (4, 20), "Fleiß": (4, 20), "Verlässlichkeit": (4, 20), "Ängstlichkeit": (4, 20),
    "Niedergeschlagenheit": (4, 20), "Unbeständigkeit der Gefühle": (4, 20), "Ästhetisches Empfinden": (4, 20), "Intellektuelle Neugierde": (4, 20),
    "Kreativer Einfallsreichtum": (4, 20), "Aufmerksamkeit": (0, 20), "Kognitive Instabilität": (0, 12), "Motorische Impulsivität": (0, 28),
    "Beharrlichkeit": (0, 16), "Selbst Kontrolle": (0, 24), "Kognitive Komplexität": (0, 20)
}
# Load and process data
def load_data(file_path):
    with open(file_path, 'r') as f:
        data = json.load(f)
    
    extracted_data = []
    for entry in data['Rows']:
        for i in range(1, 10, 2):  # Iterate over feature-value pairs
            feature_name = entry.get(f'Column{i}', '').strip()
            feature_value = entry.get(f'Column{i+1}', '')
            
            if feature_name:
                if isinstance(feature_value, (int, float)):
                    extracted_data.append([feature_name, feature_value, 'numeric'])
                elif isinstance(feature_value, str) and feature_value.strip():
                    if feature_name.lower() == "strategy":
                        extracted_data.append([feature_name, feature_value.replace("\n", " "), 'text'])
                    elif feature_name.endswith("AndSaved?") or feature_name.endswith("ndKilled?") or feature_name.endswith("Video"):
                        extracted_data.append([feature_name, feature_value.lower(), 'category'])
                    else:
                        try:
                            feature_value = float(feature_value.replace(',', '.'))
                            extracted_data.append([feature_name, feature_value, 'numeric'])
                        except ValueError:
                            extracted_data.append([feature_name, feature_value, 'text'])
    
    df = pd.DataFrame(extracted_data, columns=['Feature', 'Value', 'Type'])
    return df
# Load BERT model
#bert_model_name = 'microsoft/deberta-v3-large'
#tokenizer = AutoTokenizer.from_pretrained(bert_model_name)
bert_model_name = "bert-base-multilingual-cased"
bnb_config = BitsAndBytesConfig(load_in_8bit=True)  # Use 4-bit if needed

tokenizer = AutoTokenizer.from_pretrained(bert_model_name, force_download=True)
bert_model = AutoModelForSequenceClassification.from_pretrained(
    bert_model_name, 
    force_download=True,
    quantization_config=bnb_config
)

# Load SBERT for sentence embeddings
sbert_model = SentenceTransformer('all-mpnet-base-v2')

# Function to process text using BERT

def process_text(text_data, target_rows):
    embeddings = []
    for text in text_data.values.ravel():
        inputs = tokenizer(text, return_tensors="pt", truncation=True, padding=True)
        with torch.no_grad():
            output = bert_model(**inputs).last_hidden_state.mean(dim=1).cpu().numpy()
        embeddings.append(output.flatten())
    embeddings = np.array(embeddings) if len(embeddings) > 0 else np.zeros((1, 1024))
    return np.tile(embeddings, (target_rows // embeddings.shape[0] + 1, 1))[:target_rows]


# Move the model initialization outside to prevent pickling issues
embed_model = HuggingFaceEmbeddings(model_name="sentence-transformers/all-mpnet-base-v2")

# RAG Pipeline: Extract embeddings from PDFs
def extract_knowledge_embeddings():
    pdf_files = [f for f in os.listdir(RESOURCES_FOLDER) if f.endswith('.pdf')]
    rag_embeddings = []
    
    for pdf in pdf_files:
        pdf_loader = PyPDFLoader(os.path.join(RESOURCES_FOLDER, pdf))
        docs = pdf_loader.load_and_split()
        
        for doc in docs:
            text_embedding = embed_model.embed_query(doc.page_content)
            rag_embeddings.append(text_embedding)

    # Return raw numpy array instead of objects
    return np.mean(rag_embeddings, axis=0) if rag_embeddings else np.zeros((1, 768))


# Ensure row dimensions match
def match_row_dimensions(arr, target_rows):
    current_rows = arr.shape[0]
    return arr[:target_rows] if current_rows > target_rows else np.tile(arr, (target_rows // current_rows + 1, 1))[:target_rows]

# Process Predictions
def process_predictions():
    input_files = [f for f in os.listdir(INPUT_FOLDER) if f.endswith('.json')]
    knowledge_embedding = extract_knowledge_embeddings()
    
    for file_name in input_files:
        file_path = os.path.join(INPUT_FOLDER, file_name)
        df = df = load_data(file_path).fillna('')

        numeric_data = df[df['Type'] == 'numeric'][['Value']].astype(float)
        categorical_data = df[df['Type'] == 'category'][['Value']]
        text_data = df[df['Type'] == 'text'][['Value']]

        num_rows = max(len(numeric_data), len(categorical_data), len(text_data))
        numeric_transformer = StandardScaler()
        categorical_transformer = OneHotEncoder(handle_unknown='ignore')

        X_numeric = numeric_transformer.fit_transform(numeric_data) if not numeric_data.empty else np.zeros((num_rows, 1))
        X_categorical = categorical_transformer.fit_transform(categorical_data).toarray() if not categorical_data.empty else np.zeros((num_rows, 1))
        X_text = process_text(text_data, num_rows)
        X_rag = np.tile(knowledge_embedding, (num_rows, 1))

        X = np.hstack([match_row_dimensions(X_numeric, num_rows), match_row_dimensions(X_categorical, num_rows), X_text, X_rag])
        model = xgb.XGBRegressor(n_estimators=200, max_depth=6, learning_rate=0.05)
        model.fit(X, np.zeros(X.shape[0]))
        predictions = model.predict(X)
        output_data = [["Scores"] + list(SCORE_RANGES.keys()), ["WholeGame"] + predictions.tolist()]
        with open(os.path.join(OUTPUT_FOLDER, file_name), 'w', encoding='utf-8') as json_file:
            json.dump(output_data, json_file, indent=4, ensure_ascii=False)

process_predictions()
