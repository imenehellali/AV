from collections import defaultdict
import torch
import json
import os
import fitz  # PyMuPDF
import pdfplumber
import re
import tempfile
import os
import openai
from openai import ChatCompletion
from PIL import Image
from io import BytesIO
from doctr.io import DocumentFile
from doctr.models import ocr_predictor
from typing import Any, List, Dict, Tuple
from sentence_transformers import SentenceTransformer, util
from transformers import BlipProcessor, BlipForConditionalGeneration

doctr_model = ocr_predictor(pretrained=True)
embedding_model = SentenceTransformer("paraphrase-MiniLM-L6-v2")

define_ranges = {
    "Urgency": (11, 44), "Lack of premeditation": (10, 40), "Lack of perseverance": (10, 40),
    "Sensation seeking": (12, 48), "Extraversion": (12, 60), "Verträglichkeit": (12, 60),
    "Gewissenhaftigkeit": (12, 60), "Negative Emotionalität": (12, 60), "Offenheit": (12, 60),
    "Geselligkeit": (4, 20), "Durchsetzungsfähigkeit": (4, 20), "Aktivität": (4, 20),
    "Mitgefühl": (4, 20), "Höflichkeit": (4, 20), "Zwischenmenschliches Vertrauen": (4, 20),
    "Ordnungsliebe": (4, 20), "Fleiß": (4, 20), "Verlässlichkeit": (4, 20),
    "Ängstlichkeit": (4, 20), "Niedergeschlagenheit": (4, 20), "Unbeständigkeit der Gefühle": (4, 20),
    "Ästhetisches Empfinden": (4, 20), "Intellektuelle Neugierde": (4, 20),
    "Kreativer Einfallsreichtum": (4, 20), "Aufmerksamkeit": (0, 20), "Kognitive Instabilität": (0, 12),
    "Motorische Impulsivität": (0, 28), "Beharrlichkeit": (0, 16), "Selbst Kontrolle": (0, 24),
    "Kognitive Komplexität": (0, 20)
}

ALL_SCORES = [
    "Urgency", "Lack of premeditation", "Lack of perseverance", "Sensation seeking",
    "Extraversion", "Verträglichkeit", "Gewissenhaftigkeit", "Negative Emotionalität", "Offenheit",
    "Geselligkeit", "Durchsetzungsfähigkeit", "Aktivität", "Mitgefühl", "Höflichkeit",
    "Zwischenmenschliches Vertrauen", "Ordnungsliebe", "Fleiß", "Verlässlichkeit",
    "Ängstlichkeit", "Niedergeschlagenheit", "Unbeständigkeit der Gefühle",
    "Ästhetisches Empfinden", "Intellektuelle Neugierde", "Kreativer Einfallsreichtum",
    "Aufmerksamkeit", "Kognitive Instabilität", "Motorische Impulsivität",
    "Beharrlichkeit", "Selbst Kontrolle", "Kognitive Komplexität"
]

######  Creating custom term map and extending via embedding space #######

def load_base_terms() -> Dict[str, List[str]]:
    return {
        "BFI-2": [
            "Extraversion", "Verträglichkeit", "Gewissenhaftigkeit", "Negative Emotionalität", "Offenheit",
            "Geselligkeit", "Durchsetzungsfähigkeit", "Aktivität", "Mitgefühl", "Höflichkeit", "Zwischenmenschliches Vertrauen",
            "Ordnungsliebe", "Fleiß", "Verlässlichkeit", "Ängstlichkeit", "Niedergeschlagenheit", "Unbeständigkeit der Gefühle",
            "Ästhetisches Empfinden", "Intellektuelle Neugierde", "Kreativer Einfallsreichtum","Extraversion", "Agreeableness", "Conscientiousness", "Neuroticism", "Openness",
            "Sociability", "Assertiveness", "Activity", "Compassion", "Politeness", "Interpersonal Trust",
            "Orderliness", "Diligence", "Reliability", "Anxiety", "Depression", "Emotional Instability",
            "Aesthetic Sensitivity", "Intellectual Curiosity", "Creative Imagination"
        ],
        "UPPS": [
            "Urgency", "Lack of premeditation", "Lack of perseverance", "Sensation seeking"
        ],
        "BIS-11": [
            "Aufmerksamkeit", "Kognitive Komplexität", "Motorische Impulsivität",
            "Selbst Kontrolle", "Beharrlichkeit", "Kognitive Instabilität", "Attention score", "Cognitive Complexity scores", "Motor Scores",
            "Self-Control scores", "Perseverance scores", "Cognitive Instability score"
        ],
        "Neurobehaviorale Marker": [
            "vmPFC", "rIFG", "Striatum", "Amygdala", "Hautleitfähigkeit (SCR)", "Sakkaden", "Okulomotorische Kontrolle",
            "Aufmerksamkeitsverzerrung", "Cue-Reaktivität", "Belohnungssensitivität", "Emotionale Regulation",
            "Kognitive Flexibilität", "Reaktionshemmung",
            "skin conductance response", "saccades", "oculomotor control", "attentional bias", "cue reactivity",
            "reward sensitivity", "emotion regulation", "cognitive flexibility", "response inhibition"
        ],
        "Spielmetriken": [
            "Fixationsdauer", "Disengagement-Zeit", "Pupillenerweiterung", "Reaktionszeit", "Blickwechselmuster",
            "Spielrate am Spielautomaten", "Ressourcenzuteilungszeit", "Entscheidungsverzögerung", "Strategieanpassung",
            "Risikobereitschaftsindex", "Pfadwahlmuster", "Fehlerrate", "Latenz akustischer Reaktion",
            "Nutzung der Anweisungstafel", "Blickabweichung Alkoholisch vs. Neutral",
            "fixation duration", "disengagement time", "pupil dilation", "reaction time", "gaze shift pattern",
            "slot machine play rate", "resource allocation time", "decision latency", "strategy adaptation",
            "risk taking index", "path choice pattern", "error rate", "audio response latency",
            "instruction panel use", "gaze deviation alcoholic vs neutral"
        ],
        "Abhängigkeitskontext": [
            "Substanzgebrauchsstörung", "Craving", "Sucht-Cue", "Belohnungsreiz", "Verlustsensitivität",
            "Impulskontrolle", "Risikobias", "Zwanghaftes Verhalten", "Hochsaliente Stimuli",
            "Hemmungsversagen", "Verhaltenssucht", "Rückfallrisiko", "Emotionale Labilität",
            "Neuheitssuche",
            "substance use disorder", "addictive cue", "reward cue", "loss sensitivity",
            "impulse control", "risk bias", "compulsive behavior", "high salience stimuli",
            "inhibitory failure", "behavioral addiction", "relapse risk", "emotional volatility",
            "novelty seeking"
        ]
    }

# Step 2: Generate embeddings
def generate_embeddings(terms: List[str], model) -> torch.Tensor:
    return model.encode(terms, convert_to_tensor=True)

# Step 3: Find related terms using cosine similarity
def find_related_terms(terms: List[str], embeddings: torch.Tensor, top_k: int = 5) -> Dict[str, List[Tuple[str, float]]]:
    similarities = {}
    for i, term in enumerate(terms):
        cos_scores = util.pytorch_cos_sim(embeddings[i], embeddings)[0]
        top_results = torch.topk(cos_scores, k=top_k + 1)
        related = [(terms[idx], float(cos_scores[idx])) for idx in top_results.indices[1:]]
        similarities[term] = related
    return similarities

# Step 4: Save the similarity dictionary to file
def save_expanded_terms(similarities: Dict[str, List[Tuple[str, float]]], output_path: str) -> None:
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(similarities, f, indent=2, ensure_ascii=False)

# Step 5: Load expanded terms from file
def load_expanded_terms(output_path: str) -> Dict[str, List[Tuple[str, float]]]:
    with open(output_path, 'r', encoding='utf-8') as f:
        return json.load(f)
    

######### Reading the pdf's and extracting knwoledge via the custom terms map #######

# Step 7: Extract raw text from all PDFs using PyMuPDF (text layer only)
def extract_text_from_pdf(pdf_path: str) -> str:
    try:
        doc = fitz.open(pdf_path)
        full_text = ""
        for page in doc:
            full_text += page.get_text("text") + "\n"
        return full_text
    except Exception as e:
        print(f"Error reading {pdf_path}: {e}")
        return ""

# Step 7b: Extract tables using pdfplumber
def extract_tables_from_pdf(pdf_path: str) -> List[str]:
    tables_text = []
    try:
        with pdfplumber.open(pdf_path) as pdf:
            for page in pdf.pages:
                tables = page.extract_tables()
                for table in tables:
                    for row in table:
                        line = " | ".join([cell.strip() if cell else "" for cell in row])
                        tables_text.append(line)
    except Exception as e:
        print(f"Error extracting tables from {pdf_path}: {e}")
    return tables_text

# Step 7c: Placeholder for extracting info from images (e.g. diagrams or scanned pages)
def extract_images_from_pdf(pdf_path: str, processor=None, model=None, doctr_model=None) -> List[str]:
    from PIL import Image
    from doctr.io import DocumentFile
    import warnings
    warnings.filterwarnings("ignore", category=UserWarning)

    image_insights = []
    try:
        doc = fitz.open(pdf_path)
        for page_number, page in enumerate(doc):
            try:
                # Render full page as image (high resolution)
                pix = page.get_pixmap(matrix=fitz.Matrix(2, 2))
                image = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)

                extracted_parts = []

                # Vision-language model (BLIP)
                if processor and model:
                    inputs = processor(images=image, return_tensors="pt")
                    out = model.generate(**inputs, max_new_tokens=100)
                    blip_caption = processor.decode(out[0], skip_special_tokens=True)
                    extracted_parts.append(blip_caption)

                # Deep OCR using Doctr
                try:
                    if image.mode != 'RGB':
                        image = image.convert('RGB')

                    with tempfile.NamedTemporaryFile(suffix=".png", delete=False) as tmp:
                        image.save(tmp.name, format="PNG")
                        tmp_path = tmp.name

                    doc_img = DocumentFile.from_images(tmp_path)
                    ocr_result = doctr_model(doc_img).export()

                    os.remove(tmp_path)  # Clean up
                    for block in ocr_result['pages'][0]['blocks']:
                        for line in block['lines']:
                            text_line = ' '.join([word['value'] for word in line['words']])
                            if text_line.strip():
                                extracted_parts.append(text_line)
                except Exception as ocr_error:
                    print(f"❌ OCR failed on page {page_number} of {pdf_path}: {ocr_error}")
                    if 'tmp_path' in locals():
                        os.remove(tmp_path)
                    continue

                combined_text = " ".join(extracted_parts).strip()
                if combined_text:
                    image_insights.append(combined_text)

            except Exception as e:
                print(f"❌ Failed to render page {page_number} of {pdf_path}: {e}")
    except Exception as e:
        print(f"❌ Error accessing PDF {pdf_path}: {e}")
    return image_insights

# Step 8: Search text and tables for rule-like statements
def extract_rule_like_snippets(lines: List[str], known_terms: List[str]) -> List[str]:
    rule_lines = []
    trigger_keywords = [ "correlates with", "is associated with", "is related to", "related to",
    "is linked to", "linked to", "depends on", "shows association with", "predicts", "leads to", "influences", "contributes to", "results in",
    "accounts for", "explains", "is a predictor of", "causes", "due to", "because of", "as a result of", "affects",
    "impacts", "determines", "triggers", "if", "then", "when", "whenever", "where", "significant", "shows", "increased", "decreased", "difference between", "effect of"]

    for line in lines:
        lower_line = line.lower()
        if any(term.lower() in lower_line for term in known_terms):
            if any(kw in lower_line for kw in trigger_keywords):
                rule_lines.append(line.strip())
    return rule_lines

# Step 10: Convert rule-like strings to structured rule dicts
def parse_to_structured_rule(text: str, known_terms: List[str], threshold: float = 0.6) -> Dict:
    rule = {
        "input": {},
        "output": {},
        "relations": [],
        "raw": text
    }
    try:
        lower_text = text.lower()
        text_embedding = embedding_model.encode(text, convert_to_tensor=True)
        term_embeddings = embedding_model.encode(known_terms, convert_to_tensor=True)
        cosine_scores = util.pytorch_cos_sim(text_embedding, term_embeddings)[0]

        matched_terms = [(known_terms[i], float(cosine_scores[i])) for i in range(len(known_terms)) if cosine_scores[i] >= threshold]
        matched_terms = sorted(matched_terms, key=lambda x: -x[1])  # sort by confidence

        trigger_phrases = [ "correlates with", "is associated with", "is related to", "related to", "is linked to", "linked to", "depends on", "shows association with", "predicts", "leads to", "influences", "contributes to", "results in", "accounts for", "explains", "is a predictor of", "causes", "due to", "because of", "as a result of", "affects", "impacts", "determines", "triggers", "if", "then", "when", "whenever", "where", "significant", "shows", "increased", "decreased", "difference between", "effect of"]
        
        trigger_found=False

        # Detect relations if trigger phrases exist
        for phrase in trigger_phrases:
            if phrase in lower_text:
                trigger_found=True
                parts = lower_text.split(phrase)
                if len(parts) == 2:
                    left, right = parts
                    left_terms = [term for term, _ in matched_terms if term.lower() in left]
                    right_terms = [term for term, _ in matched_terms if term.lower() in right]
                    for src in left_terms:
                        for tgt in right_terms:
                            if src != tgt:
                                rule["input"].setdefault(src, [])
                                rule["output"].setdefault(tgt, [])
                                rule["relations"].append({"from": src, "to": tgt, "type": phrase})
                break

        # Even without phrases, add semantic terms to input or output
        if not trigger_found and matched_terms:
            for term, _ in matched_terms:
                rule["input"].setdefault(term, [])

        # Try to attach values to matched terms
        for term, _ in matched_terms:
            matches = re.findall(rf"{re.escape(term)}.*?(\d+[.,]?\d*)", text, re.IGNORECASE)
            for m in matches:
                try:
                    value = float(m.replace(",", "."))
                    if term in rule["output"]:
                        rule["output"][term].append(value)
                    else:
                        rule["input"].setdefault(term, []).append(value)
                except:
                    continue

    except Exception as e:
        print(f"Error parsing rule: {e}")

    return rule

# Step 9: Scan PDFs and build local knowledge graph with structured rules
def scan_pdfs_for_relations(pdf_paths: List[str], term_list: List[str]) -> None:
    knowledge_graph = {"nodes": set(), "edges": []}
    extracted_rules = []
    structured_rules = []

    blip_processor = BlipProcessor.from_pretrained("Salesforce/blip-image-captioning-base")
    blip_model = BlipForConditionalGeneration.from_pretrained("Salesforce/blip-image-captioning-base")
    doctr_model = ocr_predictor(pretrained=True)

    for pdf in pdf_paths:
        print(f"\n📄 Scanning: {pdf}")
        try:
            raw_text = extract_text_from_pdf(pdf)
            table_lines = extract_tables_from_pdf(pdf)
            image_lines = extract_images_from_pdf(pdf, processor=blip_processor, model=blip_model, doctr_model=doctr_model)
            all_lines = raw_text.split("\n") + table_lines + image_lines
            rules = extract_rule_like_snippets(all_lines, term_list)
            for rule_text in rules:
                extracted_rules.append({"source": pdf, "text": rule_text})
                parsed = parse_to_structured_rule(rule_text, term_list)
                structured_rules.append(parsed)

                all_terms = list(parsed["input"].keys()) + list(parsed["output"].keys())
                knowledge_graph["nodes"].update(all_terms)

                for i in range(len(all_terms)):
                    for j in range(i + 1, len(all_terms)):
                        direction = "input_to_output" if all_terms[i] in parsed["input"] and all_terms[j] in parsed["output"] else ("output_to_input" if all_terms[i] in parsed["output"] and all_terms[j] in parsed["input"] else "related") 
                        knowledge_graph["edges"].append({"from": all_terms[i], "to": all_terms[j], "label": direction})

                for rel in parsed.get("relations", []):
                    knowledge_graph["edges"].append({"from": rel["from"], "to": rel["to"], "label": rel["type"]})

        except Exception as e:
            print(f"Failed to scan {pdf}: {e}")

    knowledge_graph["nodes"] = [
        {"name": node, "values": []} for node in knowledge_graph["nodes"]
    ]

    # Link numerical values from structured rules to node metadata
    for rule in structured_rules:
        for role in ["input", "output"]:
            for term, values in rule[role].items():
                for node in knowledge_graph["nodes"]:
                    if node["name"] == term:
                        node["values"].extend(values)

    os.makedirs("KnowledgeBase", exist_ok=True)
    with open("KnowledgeBase/extracted_rules.json", "w", encoding="utf-8") as f:
        json.dump(extracted_rules, f, indent=2, ensure_ascii=False)
    with open("KnowledgeBase/structured_rules.json", "w", encoding="utf-8") as f:
        json.dump(structured_rules, f, indent=2, ensure_ascii=False)
    with open("KnowledgeBase/knowledge_graph.json", "w", encoding="utf-8") as f:
        json.dump(knowledge_graph, f, indent=2, ensure_ascii=False)

    # Add similarity-based edges from embeddings
    for term, similars in loaded_terms.items():
        for similar_term, score in similars:
            if term != similar_term:
                knowledge_graph["edges"].append({
                    "from": term,
                    "to": similar_term,
                    "label": "similar",
                    "weight": round(score, 3)
                })

    print("✔ Extraktion, Strukturierung und Ähnlichkeitsvernetzung abgeschlossen.")

def rebuild_structured_rules_and_graph(raw_rule_path: str, term_list: List[str]) -> None:
    with open(raw_rule_path, "r", encoding="utf-8") as f:
        extracted_rules = json.load(f)

    structured_rules = []
    knowledge_graph = {"nodes": set(), "edges": []}

    for rule in extracted_rules:
        parsed = parse_to_structured_rule(rule["text"], term_list)
        structured_rules.append(parsed)
        all_terms = list(parsed["input"].keys()) + list(parsed["output"].keys())
        knowledge_graph["nodes"].update(all_terms)
        for i in range(len(all_terms)):
            for j in range(i + 1, len(all_terms)):
                direction = "input_to_output" if all_terms[i] in parsed["input"] and all_terms[j] in parsed["output"] else ("output_to_input" if all_terms[i] in parsed["output"] and all_terms[j] in parsed["input"] else "related") 
                knowledge_graph["edges"].append({"from": all_terms[i], "to": all_terms[j], "label": direction})

    # Link numerical values from structured rules to node metadata
    nodes = []
    for node_name in knowledge_graph["nodes"]:
        node_values = []
        for rule in structured_rules:
            for role in ["input", "output"]:
                if node_name in rule[role]:
                    node_values.extend(rule[role][node_name])
        nodes.append({"name": node_name, "values": node_values})
    knowledge_graph["nodes"] = nodes

    os.makedirs("KnowledgeBase", exist_ok=True)
    with open("KnowledgeBase/structured_rules.json", "w", encoding="utf-8") as f:
        json.dump(structured_rules, f, indent=2, ensure_ascii=False)
    with open("KnowledgeBase/knowledge_graph.json", "w", encoding="utf-8") as f:
        json.dump(knowledge_graph, f, indent=2, ensure_ascii=False)

    print("✔ Re-parsed structured rules and updated the graph.")

# Step 11: In-Game variable Name integration
def get_all_measured_variable_names() -> List[str]:
    return [
        # Extracted in-game variables
        "CoinsQuantity", "CoinsBoughtCount", "MysterySlotPlaysPerMinute", "DiamondSlotPlaysPerMinute",
        "BillSlotPlaysPerMinute", "AvgFixationTimeAlcoholicDisplays", "AvgFixationTimeAlcoholicVsNonAlcoholic",
        "DiamondSlotPlayCount", "BillSlotPlayCount", "CakeSlotPlayCount", "MysterySlotPlayCount",
        "AvgTimePlayingDiamondMachine", "AvgTimePlayingBillMachine", "AvgTimePlayingMysteryMachine",
        "AvgTimePlayingCakeMachine", "AvgStagnantTime", "GameAccountFinalSum",
        "ProgressOfSavingWithinCH4", "TimeSpentOnCH4", "CH4DiscoveredAndSaved?", "CH4DiscoveredAndKilled?",
        "CH4WatchedVideo", "ProgressOfSavingWithinCH3", "TimeSpentOnCH3", "CH3DiscoveredAndSaved?",
        "CH3DiscoveredAndKilled?", "CH32WatchedVideo", "ProgressOfSavingWithinCH2", "TimeSpentOnCH2",
        "CH2DiscoveredAndSaved?", "CH2DiscoveredAndKilled?", "CH2WatchedVideo",
        "ProgressOfSavingWithinCH1", "TimeSpentOnCH1", "CH1DiscoveredAndSaved?", "CH1DiscoveredAndKilled?",
        "CH1WatchedVideo", "ProgressOfSavingWithinCA4", "TimeSpentOnCA4", "CA4DiscoveredAndSaved?",
        "CA4DiscoveredAndKilled?", "CA4WatchedVideo", "ProgressOfSavingWithinCA3", "TimeSpentOnCA3",
        "CA3DiscoveredAndSaved?", "CA3DiscoveredAndKilled?", "CA3WatchedVideo", "ProgressOfSavingWithinCA2",
        "TimeSpentOnCA2", "CA2DiscoveredAndSaved?", "CA2DiscoveredAndKilled?", "CA2WatchedVideo",
        "ProgressOfSavingWithinCA1", "TimeSpentOnCA1", "CA1DiscoveredAndSaved?", "CA1DiscoveredAndKilled?",
        "CA1WatchedVideo", "ProgressOfSavingWithinAStrategy", "AvgOfProgressOfHumanCases",
        "AvgOfProgressOfAnimalCases", "Strategy",

        # Strategy variants (to treat as variables too)
        "Individual optimized the allocation of resources to cover most of cases.",
        "Individual generally maximized cases with least time to survive.",
        "Individual generally maximized cases with longest time to survive.",
        "Unstructured! Individual did not optimize the allocation of resources to cover most of cases.",
        "Individual did not optimize on longest time to survive of cases.",
        "Individual did not optimize on least time to survive of cases.",
        "Individual generally prioritzed Animal cases over Human cases.",
        "Individual generally prioritzed Human cases over Animals cases.",
        "Individual did not distinctinguish nor prefer Animal cases from human cases.",
        "Individual did chose longest time to survive for human case and least time to survive for animal cases",
        "Individual did chose longest time to survive for Animal case and least time to survive for human cases"
    ]

# Step 12: Match in-game variables to structured rule inputs
def match_variables_to_rules(structured_rules_path: str, variable_names: List[str], term_threshold: float = 0.55) -> Dict[str, List[Dict]]:
    # Load rules
    with open(structured_rules_path, "r", encoding="utf-8") as f:
        rules = json.load(f)

    matched_map = {}
    var_embeddings = embedding_model.encode(variable_names, convert_to_tensor=True)

    for var_index, var_name in enumerate(variable_names):
        var_embed = var_embeddings[var_index]
        var_matches = []

        for rule_index, rule in enumerate(rules):
            input_terms = list(rule.get("input", {}).keys())
            if not input_terms:
                continue

            input_embeddings = embedding_model.encode(input_terms, convert_to_tensor=True)
            sim_scores = util.pytorch_cos_sim(var_embed, input_embeddings)[0]
            top_score, top_idx = torch.max(sim_scores, dim=0)

            if float(top_score) >= term_threshold:
                var_matches.append({
                    "rule_index": rule_index,
                    "matched_term": input_terms[top_idx],
                    "score": float(top_score)
                })

        # Always match at least 1 rule (fallback to best match even if < threshold)
        if not var_matches:
            best_match = None
            best_score = -1
            for rule_index, rule in enumerate(rules):
                input_terms = list(rule.get("input", {}).keys())
                if not input_terms:
                    continue
                input_embeddings = embedding_model.encode(input_terms, convert_to_tensor=True)
                sim_scores = util.pytorch_cos_sim(var_embed, input_embeddings)[0]
                top_score, top_idx = torch.max(sim_scores, dim=0)
                if float(top_score) > best_score:
                    best_score = float(top_score)
                    best_match = {
                        "rule_index": rule_index,
                        "matched_term": input_terms[top_idx],
                        "score": float(top_score)
                    }
            if best_match:
                var_matches.append(best_match)

        matched_map[var_name] = var_matches

    # Save the mapping
    os.makedirs("KnowledgeBase", exist_ok=True)
    with open("KnowledgeBase/variable_to_rule_map.json", "w", encoding="utf-8") as f:
        json.dump(matched_map, f, indent=2, ensure_ascii=False)

    return matched_map

def load_json(path: str):
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

# Step 13: extract in-game values to put in prompt chunks
def extract_all_relevant_variables(file_path: str) -> Dict[str, float]:
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    rows = data.get("Rows", [])
    variable_map = {}

    for row in rows:
        col1 = row.get("Column1", "").strip()
        col2 = row.get("Column2", None)
        col3 = row.get("Column3", "").strip()
        col4 = row.get("Column4", None)
        col5 = row.get("Column5", "").strip()
        col6 = row.get("Column6", None)
        col9 = row.get("Column9", "").strip()
        col10 = row.get("Column10", None)

        if col1 and isinstance(col2, (int, float)):
            variable_map[col1] = col2
        if col3 and isinstance(col4, (int, float, str)):
            variable_map[col3] = col4
        if col5 and isinstance(col6, (int, float)):
            variable_map[col5] = col6
        if col9 and isinstance(col10, (int, float, str)):
            variable_map[col9] = col10

    return variable_map

# Step 14: Scaffold builder (prompt chunks)
def build_enriched_chunks_per_score(
    structured_rules_path: str,
    variable_rule_map_path: str,
    knowledge_graph_path: str,
    in_game_values: Dict[str, float],
    participant_id: str,
    chunk_size: int = 4,
    min_rules_per_score: int = 4
) -> Dict[str, List[Dict[str, Any]]]:
    # Load all required data
    rules = load_json(structured_rules_path)
    variable_to_rule = load_json(variable_rule_map_path)
    graph = load_json(knowledge_graph_path)

    # Build index of rules per score
    score_to_rule_indices = defaultdict(list)
    for idx, rule in enumerate(rules):
        for score in rule.get("output", {}):
            score_to_rule_indices[score].append(idx)
        for score in rule.get("input",{}):
            score_to_rule_indices[score].append(idx)
    
    # Match rules to in-game variables
    matched_rule_indices = set()
    for matches in variable_to_rule.values():
        for match in matches:
            matched_rule_indices.add(match["rule_index"])

    # Build enriched chunks
    score_chunks = {}
    for score in score_to_rule_indices:
        all_related_indices = score_to_rule_indices[score]
        matched_rules = []
        support_rules = []
        graph_related_rules = []

        # Separate matched vs support
        for idx in all_related_indices:
            if idx in matched_rule_indices:
                rules[idx]["source"] = "matched"
                matched_rules.append(rules[idx])
            else:
                rules[idx]["source"] = "support"
                support_rules.append(rules[idx])

       # Graph neighbors expansion
        graph_neighbors = [e for e in graph["edges"] if e["from"] == score or e["to"] == score]
        neighbor_scores = set()
        for edge in graph_neighbors:
            neighbor_scores.add(edge["from"])
            neighbor_scores.add(edge["to"])
        neighbor_scores.discard(score)

        for n_score in neighbor_scores:
            for idx in score_to_rule_indices.get(n_score, []):
                if idx not in all_related_indices:
                    rules[idx]["source"] = "graph_neighbor"
                    graph_related_rules.append(rules[idx])

        # Combine all and ensure diversity
        combined = matched_rules + support_rules + graph_related_rules
        unique_rules = {r["raw"]: r for r in combined}.values()
        unique_rules = list(unique_rules)

        if len(unique_rules) < min_rules_per_score:
            # fallback: expand with all possible rules related to neighbor scores
            additional = [
                rules[idx] for n in neighbor_scores
                for idx in score_to_rule_indices.get(n, [])
                if rules[idx]["raw"] not in [r["raw"] for r in unique_rules]
            ]
            for r in additional:
                r["source"] = "fallback_neighbor"
            unique_rules.extend(additional)

        # Split matched rules into chunks
        chunks = []
        for i in range(0, len(unique_rules), chunk_size):
            chunk = {
                "chunk_id": len(chunks) + 1,
                "variables": in_game_values,
                "rules": matched_rules[i:i + chunk_size],
                "graph_neighbors": graph_neighbors
            }
            chunks.append(chunk)

        score_chunks[score] = chunks

    # Save the enriched chunk mapping
    participant_folder = os.path.join("ParticipantPredictedResults", participant_id)
    os.makedirs(participant_folder, exist_ok=True)
    save_path = os.path.join(participant_folder, "chunked_enriched_per_score.json")
    
    with open(save_path, "w", encoding="utf-8") as f:
        json.dump(score_chunks, f, indent=2, ensure_ascii=False)

    print(f"✅ Chunks saved to {save_path}")
    return score_chunks

def initialize_prediction_json(participant_id: str):
    """Creates a blank prediction file for a participant."""
    result_path = f"ParticipantPredictedResults/{participant_id}/{participant_id}.json"
    os.makedirs(os.path.dirname(result_path), exist_ok=True)
    blank_scores = [{"scoreName": score, "scoreValue": 0} for score in ALL_SCORES]
    with open(result_path, "w", encoding="utf-8") as f:
        json.dump(blank_scores, f, indent=2, ensure_ascii=False)

def update_score_prediction(participant_id: str, score_name: str, value: int):
    """Updates the score value in the saved participant file."""
    result_path = f"ParticipantPredictedResults/{participant_id}/{participant_id}.json"
    with open(result_path, "r", encoding="utf-8") as f:
        data = json.load(f)
    for item in data:
        if item["scoreName"] == score_name:
            item["scoreValue"] = value
    with open(result_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def get_final_prediction_json(participant_id: str) -> List[Dict[str, Any]]:
    path = f"ParticipantPredictedResults/{participant_id}/{participant_id}.json"
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def predict_score_with_tot(
    participant_id: str,
    score_name: str,
    model: str = "gpt-4o-mini",
    temperature: float = 0.1,
    max_tokens: int = 3000
) -> dict:

    min_val, max_val = define_ranges.get(score_name, (0, 100))
    chunk_path = f"ParticipantPredictedResults/{participant_id}/chunked_enriched_per_score.json"

    with open(chunk_path, "r", encoding="utf-8") as f:
        score_chunks = json.load(f)

    chunks = score_chunks.get(score_name, [])
    if not chunks:
        return {"score": 0, "reasoning": "No chunks found."}

    variables_str = "\n".join([f"{k}: {v}" for k, v in chunks[0]["variables"].items()])

    # Step 1: Initial 10 candidates
    system_prompt = "You are an expert in behavioral science and psychological assessment."
    user_prompt = f"""
    Evaluate the psychometric score: {score_name}
    Valid range: [{min_val}, {max_val}]
    Use the following behavioral data:
    {variables_str}
    Related rules and context:
    {"\n".join([f"- {r['raw']}" for r in chunks[0].get("rules", [])])}
    Generate 10 plausible score candidates based on this data.
    Format your response as a list of numbers only.
    """.strip()

    response = openai.ChatCompletion.create(
        model=model,
        temperature=temperature,
        max_tokens=max_tokens,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt}
        ]
    )
    text = response["choices"][0]["message"]["content"]
    candidates = extract_valid_scores(text, min_val, max_val)
    candidates = list(sorted(set(candidates)))[:10]

    thoughts = [f"[Chunk 1] Candidates: {candidates}\n"]

    # Step 2–5: Use remaining chunks to refine
    for i, chunk in enumerate(chunks[1:], start=2):
        if not candidates:
            break

        rules_str = "\n".join([f"- {r['raw']}" for r in chunk.get("rules", [])])
        user_prompt = f"""
        Score to refine: {score_name} in range [{min_val}, {max_val}]
        Current score candidates: {candidates}
        Participant's behavior:
        {variables_str}
        Use these additional rules to refine and select a better subset:
        {rules_str or "(no extra rules)"}
        Choose the {max(5, len(candidates) // 2)} most likely candidates and briefly justify.
        """.strip()

        response = openai.ChatCompletion.create(
            model=model,
            temperature=temperature,
            max_tokens=max_tokens,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ]
        )
        reply = response["choices"][0]["message"]["content"]
        new_candidates = extract_valid_scores(reply, min_val, max_val)

        new_candidates = sorted(set(new_candidates))
        thoughts.append(f"[Chunk {i}] {reply.strip()}\n")

        if len(new_candidates) < 5:
            break
        candidates = new_candidates[:max(5, len(new_candidates)//2)]

    final = candidates[0] if candidates else 0

    return {
        "score": final,
        "reasoning": "\n".join(thoughts)
    }

def extract_valid_scores(text: str, min_val: int, max_val: int) -> List[int]:
    # Find all numbers including decimals
    raw_numbers = re.findall(r"\b\d+(?:\.\d+)?\b", text)
    candidates = []

    for num in raw_numbers:
        try:
            val = round(float(num))
            if min_val <= val <= max_val:
                candidates.append(val)
        except:
            continue
    return sorted(set(candidates))


# Step 15 : define prompt per score given the chunks, guide gpt to repredict given chunks over and over until last prompt

def predict_all_scores_for_participant(participant_id: str):

    initialize_prediction_json(participant_id)

    for score_name in ALL_SCORES:
        try:
            result = predict_score_with_tot(participant_id, score_name)
            score = result.get("score", 0)
            update_score_prediction(participant_id, score_name, score)
        except openai.OpenAIError as e:
            print(f"❌ GPT API failed for {score_name}: {e}")
        except Exception as e:
            print(f"❌ Unexpected error for {score_name}: {e}")

    # Check for missing scores (value = 0) and reattempt logic (to be expanded)
    final_data = get_final_prediction_json(participant_id)
    missing = [item["scoreName"] for item in final_data if item["scoreValue"] == 0]
    if missing:
        print(f"⚠️ Missing predictions for: {missing} — next step will handle post-fix.")

    print(f"✅ All scores predicted for participant: {participant_id}")
    return final_data


# Main runner to test the pipeline
if __name__ == "__main__":
    # Step F: Predict per score per participant
    predict_all_scores_for_participant("CG001")
    

    
