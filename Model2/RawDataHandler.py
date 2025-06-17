import os
import json
import pandas as pd
import re
import spacy
import umap
import hdbscan
import numpy as np
import matplotlib.pyplot as plt
from sklearn.cluster import KMeans
from pprint import pprint

DURATION_KEYS = [
    "ReactionTime",
    "TimeSpent",
    "TimeGaze",
    "TimePlaying",
    "FixationTime",
    "StagnantTime",
    "PurchaseDuration",
    "AvgTimePlaying",
    "AvgFixationTime",
    "AvgTimeGazeOn",
    "incDecMarginReactionTime"
]
nlp = spacy.load("en_core_web_lg")
plt.show(block=False)


############ Variable conversion and saving as excel ##################
def read_json(file_path: str) -> dict:
    with open(file_path, "r", encoding="utf-8") as f:
        return json.load(f)

#--------------DElivers--------------
def parse_row(row: dict, excluded_columns: list) -> dict:
    parsed = {}
    # Iterate over pairs: odd -> even (e.g., Column1 → Column2)
    for i in [1, 3, 5, 9]:
        key_col = f"Column{i}"
        val_col = f"Column{i + 1}"
        
        # Skip if key column is in excluded list
        if key_col in excluded_columns or val_col in excluded_columns:
            continue

        key = row.get(key_col, "").strip()
        value = row.get(val_col, "")

        if key != "":
            parsed[key] = value  # raw value, convert later
    return parsed

#---------------DElivers--------------
def convert_value(key: str, value: any, duration_key_substrings: list) -> str:
    
    try:
        numeric_value = float(value)
    except (ValueError, TypeError):
        return str(value)  # keep original string/text

    if any(substring in key for substring in duration_key_substrings):
        return f"{round(numeric_value, 2)} sec"
    else:
        return str(numeric_value)

#-------------------Devlivers------------------------
def save_as_excel(file_name: str, variables: dict, output_dir: str = "NormalizedData"):
    os.makedirs(output_dir, exist_ok=True)

    # Create a DataFrame with one row of data
    df = pd.DataFrame([variables])
    df.to_excel(os.path.join(output_dir, file_name.replace(".json", ".xlsx")), index=False)

############  Varibale clustering -> reducing token for openAi ############


#------------- Delivers---------------
def split_variable_name(name: str) -> str:
    try:
        words = re.findall(r'[A-Z]?[a-z]+|[A-Z]+(?=[A-Z]|$)', name)
        return " ".join(words)
    except Exception as e:
        print(f"[split_variable_name] Error processing '{name}': {e}")
        return name  # fallback: return original

#-------------- DElivers-----------
def embed_variable_names_spacy(names: list[str]) -> dict:
    
    embeddings = {}
    for name in names:
        try:
            phrase = split_variable_name(name)
            vector = nlp(phrase).vector
            embeddings[name] = vector
        except Exception as e:
            print(f"[embed_variable_names_spacy] Failed for {name}: {e}")
    return embeddings

#---------------Delivers--------------
def cluster_variable_names(embeddings: dict, n_neighbors: int = 15, min_cluster_size: int = 2):
    """
    Reduces and clusters variable name embeddings using UMAP + HDBSCAN.

    Returns: dict {variable_name: cluster_id}, plus a visualization.
    """
    try:
        names = list(embeddings.keys())
        vectors = np.array(list(embeddings.values()))

        reducer = umap.UMAP(n_components=2, n_neighbors=n_neighbors, random_state=42)
        reduced = reducer.fit_transform(vectors)

        clusterer = hdbscan.HDBSCAN(min_cluster_size=min_cluster_size)
        cluster_labels = clusterer.fit_predict(reduced)

        clustered = {name: int(cluster) for name, cluster in zip(names, cluster_labels)}

        return clustered

    except Exception as e:
        print(f"[cluster_variable_names] Error: {e}")
        return {}

#---------------Delivers: honestly perfect result---------------
def cluster_variable_names_from_excel(file_path: str):
    try:
        df = pd.read_excel(file_path)
        variable_names = list(df.columns)
        embeddings = embed_variable_names_spacy(variable_names)
        return cluster_variable_names(embeddings)
    except Exception as e:
        print(f"[cluster_variable_names_from_excel] Error: {e}")
        return {}


def categorize_yes_variables_meaningfully(file_path: str):
    print("\n=== Step 3b: Meaningful Summary of 'yes' Flags ===")
    df = pd.read_excel(file_path)
    values = df.iloc[0]
    progress_keys = [k for k in values.keys() if "ProgressOfSavingWithin" in k and str(values[k]).strip() == "1"]
    saved_flags = [k.replace("ProgressOfSavingWithin", "") for k in progress_keys]

    summary = {
        "WatchedVideosYesCount": 0,
        "HumanCasesSaved": 0,
        "HumanCasesKilled": 0,
        "AnimalCasesSaved": 0,
        "AnimalCasesKilled": 0,
        "Strategy": None
    }

    for var, val in values.items():
        val_str = str(val).strip().lower()

        if var == "Strategy":
            summary["Strategy"] = val
            print(f"📌 Preserving Strategy: {val}")

        elif val_str == "yes":
            if "WatchedVideo" in var:
                summary["WatchedVideosYesCount"] += 1

            elif "Saved?" in var:
                if "CH" in var:
                    summary["HumanCasesSaved"] += 1
                elif "CA" in var:
                    summary["AnimalCasesSaved"] += 1

            elif "Killed?" in var:
                stripped = var.replace("DiscoveredAndKilled?", "")
                if stripped not in saved_flags:
                    if "CH" in var:
                        summary["HumanCasesKilled"] += 1
                    elif "CA" in var:
                        summary["AnimalCasesKilled"] += 1

    for k, v in summary.items():
        if k != "Strategy":
            print(f"✅ {k} → {v}")
    return summary

if __name__ == "__main__":
    print("\n=== Step 3: Clustering Values by Group ===")
    variable_clusters = cluster_variable_names_from_excel("NormalizedData/CG001.xlsx")
    summary = categorize_yes_variables_meaningfully("NormalizedData/CG001.xlsx")
