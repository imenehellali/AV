import os
import json
import re

FOLDER = "KnowledgeBase/ScoreRules"
############# Clustering rules & summarization --> reducing token for openAi#############
BAD_PATTERNS = [
    r"no (mention|relevance|clear relation|relevant information)",
    r"the sentence does not",
    r"see response",
    r"not directly relate",
    r"irrelevant",
    r"does not apply",
    r"focuses on irrelevant",
    r"the sentence implies there is no",
    r"low relevance",
    r"the study did not",
    r"the text does not",
    r"unrelated to the trait",
    r"this information is not connected",
]
LEADING_PATTERNS = [
    r"^(The sentence('?s)? (highlights|suggests|implies|focuses|focus|indicates|shows|describes|explores|mentions|states)\s*(that\s)?)",
    r"^(The study('?s)? (investigates|suggests|implies|focuses|focus|focused on|indicates|shows|describes|explores|mentions|states)\s*(that\s)?)",
    r"^(This sentence\s+(suggests|implies|focuses|focus|indicates|shows|describes|explores)\s*(that\s)?)",
    r"^(Sentence\s+(suggests|implies|shows|indicates|focuses)\s*(that\s)?)",
    r"^Cleaned Rule:\s*",
    r"^Rule:\s*",
    r"^Value:\s*",
    r"^Metric:\s*",
    r"^Relevance:\s*",
    r"^In this sentence, "
]


compiled_leading = [re.compile(p, re.IGNORECASE) for p in LEADING_PATTERNS]

def is_garbage(rule_text: str) -> bool:
    lower = rule_text.lower().strip()
    return any(re.search(pattern, lower, flags=re.IGNORECASE) for pattern in BAD_PATTERNS)

def clean_json_file(path: str):
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)

    if not isinstance(data, list):
        print(f"⚠️ Skipped non-list file: {path}")
        return

    original_len = len(data)
    cleaned_data = [entry for entry in data if not is_garbage(entry.get("rule", ""))]

    if len(cleaned_data) != original_len:
        with open(path, "w", encoding="utf-8") as f:
            json.dump(cleaned_data, f, indent=2, ensure_ascii=False)
        print(f"🧹 Cleaned {path}: removed {original_len - len(cleaned_data)} garbage rules")

# ✂️ Step 2: Strip leading subphrases from rule
def clean_rule_texts_in_json_files(folder_path: str):
    for filename in os.listdir(folder_path):
        if not filename.endswith(".json"):
            continue

        path = os.path.join(folder_path, filename)
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)

            cleaned = []
            for entry in data:
                rule = entry.get("rule", "")
                original = rule

                for pattern in compiled_leading:
                    rule = pattern.sub("", rule).strip()

                # Capitalize if lost after strip
                if rule and not rule[0].isupper():
                    rule = rule[0].upper() + rule[1:]

                if rule:
                    cleaned.append({"rule": rule})

            with open(path, "w", encoding="utf-8") as f:
                json.dump(cleaned, f, indent=2, ensure_ascii=False)

            print(f"✅ Stripped subphrases in {filename} — {len(cleaned)} rules")

        except Exception as e:
            print(f"❌ Error cleaning {filename}: {e}")

# 🧼 Full pipeline
def clean_all_score_rules():
    print("🔁 Removing garbage rules...")
    for filename in os.listdir(FOLDER):
        if filename.endswith(".json"):
            clean_json_file(os.path.join(FOLDER, filename))

    print("✂️ Stripping leading subphrases...")
    clean_rule_texts_in_json_files(FOLDER)

# Run
if __name__ == "__main__":
    clean_all_score_rules()