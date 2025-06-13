# === NEW PIPELINE FROM SCRATCH (CLEAN DESIGN) ===
# STEP 0: Constants
from multiprocessing import Pool
from dotenv import load_dotenv
import os, json
from pathlib import Path
import httpx
from llama_cpp import Llama
import openai
from typing import List, Dict
from huggingface_hub import InferenceClient
import fitz
import tiktoken
import pdfplumber
from PIL import Image
import pytesseract
import psutil
import time
from concurrent.futures import ProcessPoolExecutor
import subprocess
import uuid
import re

pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
print(pytesseract.image_to_string(Image.new("RGB", (100, 30))))

load_dotenv()

DEFINE_RANGES = {
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

SCORE_ALIASES = {
    "Impulsivity": "Motorische Impulsivität",
    "Motor Impulsivity": "Motorische Impulsivität",
    "Impulse Control": "Selbst Kontrolle",
    "Lack of self-control": "Selbst Kontrolle",
    "Self Control": "Selbst Kontrolle",
    "Self-Control": "Selbst Kontrolle",

    "Impulsiveness": "Impulsivität",
    "Perseverance": "Beharrlichkeit",

    "Attention": "Aufmerksamkeit",
    "Attention score": "Aufmerksamkeit",
    "Distractibility": "Aufmerksamkeit",

    "Cognitive Instability": "Kognitive Instabilität",
    "Cognitive Instability score": "Kognitive Instabilität",
    "Emotional Instability": "Unbeständigkeit der Gefühle",
    "Cognitive Complexity": "Kognitive Komplexität",

    "Creativity": "Kreativer Einfallsreichtum",
    "Creative Imagination": "Kreativer Einfallsreichtum",

    "Curiosity": "Intellektuelle Neugierde",
    "Intellectual Curiosity": "Intellektuelle Neugierde",

    "Aesthetic Sensitivity": "Ästhetisches Empfinden",

    "Negative Emotion": "Negative Emotionalität",
    "Negative Emotionality": "Negative Emotionalität",
    "Neuroticism": "Negative Emotionalität",
    "Anxiety": "Ängstlichkeit",
    "Depression": "Niedergeschlagenheit",

    "Diligence": "Fleiß",
    "Orderliness": "Ordnungsliebe",
    "Politeness": "Höflichkeit",
    "Empathy": "Mitgefühl",
    "Trust": "Zwischenmenschliches Vertrauen",
    "Interpersonal Trust": "Zwischenmenschliches Vertrauen",
    "Reliability": "Verlässlichkeit",

    "Extraversion": "Extraversion",
    "Sociability": "Geselligkeit",
    "Assertiveness": "Durchsetzungsfähigkeit",
    "Activity Level": "Aktivität",

    "Agreeableness": "Verträglichkeit",
    "Conscientiousness": "Gewissenhaftigkeit",

    "Openness": "Offenheit",
    "Sensation Seeking": "Sensation seeking"
}

ALL_SCORES = list(DEFINE_RANGES.keys())
# === GLOBAL CONFIG ===
MAX_TOKENS_PER_CHUNK = 2000
NUM_WORKERS = 2
MODEL_PATH = "C:/Users/hella/Desktop/llama.cpp/models/mistral-7b/mistral-7b-instruct-v0.2.Q4_K_M.gguf"

# === LLaMA LOADER ===
def load_llama():
    return Llama(
        model_path=MODEL_PATH,
        n_ctx=4096,
        n_threads=6,
        use_mlock=True,
        n_gpu_layers=0  # adjust if needed
    )

LLAMA_MODEL = load_llama()
# STEP 1: Create 30 clean JSON files for each score

def initialize_score_json_files():
    os.makedirs("KnowledgeBase/ScoreRules", exist_ok=True)
    for score in ALL_SCORES:
        path = f"KnowledgeBase/ScoreRules/{score}.json"
        if not os.path.exists(path):
            with open(path, "w", encoding="utf-8") as f:
                json.dump([], f, indent=2, ensure_ascii=False)
    print("✅ Initialized 30 JSON files")

#-------------- DELIVERS:            STEP 2: Create structured prompt for LLaMA/DeepSeek
def build_semantic_extraction_prompt(text_chunk: str) -> str:
    return f"""### Instruction:
Extract behavioral or psychological rules from the following scientific text.
Use one sentence per rule. If no rules, write "No rules found."

### Text:
{text_chunk}

### Response:"""

def build_trait_classification_prompt(text: str) -> str:
    base_prompt_template = """
Understad the scientific sentence below, and Assign it to the 1 to 5 most relevant psychometric traits from this list:
{traits}
Response Format: TraitName --> Cleaned Rule (one line per trait, summarization, include values and metrics only if they appear in sentence):

### Text:
"{text}"
### Response:""".strip()

    return base_prompt_template.format(
        text=text.strip(),
        traits=", ".join(ALL_SCORES)
    )

#---------------DELIVERS
def estimate_token_count(text: str) -> int:
    enc = tiktoken.get_encoding("cl100k_base")
    return len(enc.encode(text))

# STEP 3: DELIVERS ------------LLaMA/DeepSeek Semnatic Rule extraction

def query_model(prompt: str, model: Llama) -> List[str]:
    try:
        prompt_tokens = estimate_token_count(prompt)
        max_total_tokens = 4096
        buffer = 50
        available = max_total_tokens - prompt_tokens - buffer
        if available <= 0:
            print(f"❌ Prompt too large: {prompt_tokens} tokens, skipping.")
            return []
        max_tokens = min(available, 1000)  # Limit response to 1000 max even if more room
        assert prompt_tokens + max_tokens <= 4096, f"Overflow: {prompt_tokens + max_tokens}"
        response = model(prompt, max_tokens=max_tokens, temperature=0.2, stop=["\n\n"])
        output = response.get("choices", [{}])[0].get("text", "").strip()
        return [line.strip() for line in output.split("\n") if line.strip()]
    except Exception as e:
        print(f"❌ LLaMA failed to respond: {e}")
        return []

#------------- DELIVERS
def save_score_rule_line(rule_line: str):
    try:
        if "-->" not in rule_line:
            print(f"⚠️ Malformed rule line: {rule_line}")
            return

        trait, rule = [part.strip() for part in rule_line.split("-->", 1)]
        trait = SCORE_ALIASES.get(trait, trait)

        # Match against known score names
        for canonical in ALL_SCORES:
            if canonical.lower() == trait.lower():
                trait = canonical
                break
        else:
            print(f"⚠️ Trait '{trait}' not recognized → skipping.")
            return

        # Load file
        path = f"KnowledgeBase/ScoreRules/{trait}.json"
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)
        except FileNotFoundError:
            data = []

        # Check for duplicate
        if any(entry["rule"] == rule for entry in data):
            print(f"⏭️ Skipping duplicate rule for {trait}: {rule}")
            return

        # Add rule
        data.append({"rule": rule})
        with open(path, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        print(f"✅ Added rule to {trait}.json: {rule}")

    except Exception as e:
        print(f"❌ Error saving rule line: {e}")

#----------------------DELIVERS
def split_text_by_token_limit(text: str, max_tokens: int = 3000) -> List[str]:

    enc = tiktoken.get_encoding("cl100k_base")  # works well for GPT-like models
    tokens = enc.encode(text)
    chunks = []
    start = 0
    while start < len(tokens):
        end = min(start + max_tokens, len(tokens))
        chunk = enc.decode(tokens[start:end])
        chunks.append(chunk)
        start = end
    return chunks

# STEP 5.3 --- Early Termination ----
def is_reference_section(text: str) -> bool:
    lines = text.strip().split("\n")
    reference_like = 0

    for line in lines:
        if re.match(r"^[A-Z][a-z]+,\s?[A-Z]\.", line):  # e.g., "Smith, J."
            reference_like += 1
        elif re.match(r"^[A-Z]\w+,\s?[A-Z]\w+.*\d{4}", line):  # "Yucel, M., Lubman, D.I., 2007"
            reference_like += 1

    ratio = reference_like / max(len(lines), 1)
    return ratio > 0.5  # if over 50% of lines look like references, skip

#STEP 5.2 --- Table Text ---
def extract_tables_as_text(page) -> str:
    try:
        tables = page.extract_tables()
        table_text = []
        for table in tables:
            for row in table:
                cleaned_row = [cell.strip() if cell else "" for cell in row]
                table_text.append(" | ".join(cleaned_row))
        return "\n".join(table_text)
    except:
        return ""

def clean_reference_lines(text: str) -> str:
    lines = text.strip().split("\n")
    return "\n".join([
        l for l in lines
        if not re.match(r".*\d{4}.*\.\s*J\.\w+", l)  # crude match for: Author, year. Journal
    ])

# STEP 5: Process one PDF
def process_pdf(pdf_path: str) -> List[str]:
    llama = LLAMA_MODEL

    if not os.path.exists("DoneResources"):
        os.makedirs("DoneResources")

    filename = os.path.basename(pdf_path)
    dest_path = f"DoneResources/{filename}"
    if os.path.exists(dest_path):
        print(f"✅ Already processed {filename}, skipping.")
        return []

    full_text = []

    try:
        with pdfplumber.open(pdf_path) as pdf:
            for i, page in enumerate(pdf.pages):
                try:
                    text = clean_reference_lines(page.extract_text() or "")
                    table_text = extract_tables_as_text(page) or ""

                    page_content = "\n".join([
                        f"📄 Extracted Text:\n{text.strip()}",
                        f"📊 Extracted Tables:\n{table_text.strip()}"
                    ]).strip()

                    if is_reference_section(page_content):
                        print(f"🛑 Reached references on page {i}, stopping.")
                        break

                    full_text.append(page_content)

                except Exception as e:
                    print(f"⚠️ Error on page {i}: {e}")
                    continue

        if not full_text:
            print(f"⚠️ No extractable text found in {filename}")
            return []

        document_text = "\n\n".join(full_text)
        
        # Use token-based chunking for precision
        chunks = split_text_by_token_limit(document_text, max_tokens=3000)

        for i, chunk in enumerate(chunks):
            print(f"\n--- Chunk {i+1} ---\n{chunk[:300]}...")  # Show preview of chunk

            # Step 2: Build prompt
            prompt = build_semantic_extraction_prompt(chunk)
            prompt_tokens = estimate_token_count(prompt)
            #print(f"🧮 Prompt token count: {prompt_tokens}")

            # Step 3: Query model
            results = query_model(prompt, model=LLAMA_MODEL)

            # Step 4: Output
            print("🧠 Model output:")
            for line in results:
                print("-", line)
                full_prompt = build_trait_classification_prompt(line.strip())
                response_line = query_model(full_prompt, model=LLAMA_MODEL)
                for classified in response_line:
                    save_score_rule_line(classified)
        

    except Exception as e:
        print(f"❌ Failed to process {filename}: {e}")

    finally:
        # Always mark as done
        Path(pdf_path).rename(dest_path)
        print(f"📦 Moved to DoneResources: {filename}")
        return []


# === MAIN ===
if __name__ == "__main__":
    resource_folder = "Resources"
    for file in os.listdir(resource_folder):
        if file.lower().endswith(".pdf"):
            pdf_path = os.path.join(resource_folder, file)
            process_pdf(pdf_path)
