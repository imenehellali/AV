import os
from PyPDF2 import PdfReader
import openai
import re
import csv
import json

import pandas as pd

def prepare_prompt_puw(data):
    prompt = """
    You are an advanced german psychological scoring system tasked with interpreting tagged measurements provided below. These measurements must be mapped to psychological scores according to established frameworks: UPPS, Big Five Inventory, and BIS-11 assessments.

    The interpretation must strictly adhere to the following guidelines:
    - Use the given ranges to calculate the scores.
    - Ensure consistency in mapping the data to their respective traits or dimensions.
    - Provide results as integers within the defined ranges.

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [12-60]
    Verträglichkeit: [12-60]
    Gewissenhaftigkeit: [12-60]
    Negative Emotionalität: [12-60]
    Offenheit: [12-60]

    // Big Five Facets
    Geselligkeit: [4-20]
    Durchsetzungsfähigkeit: [4-20]
    Aktivität: [4-20]
    Mitgefühl: [4-20]
    Höflichkeit: [4-20]
    Zwischenmenschliches Vertrauen: [4-20]
    Ordnungsliebe: [4-20]
    Fleiß: [4-20]
    Verlässlichkeit: [4-20]
    Ängstlichkeit: [4-20]
    Niedergeschlagenheit: [4-20]
    Unbeständigkeit der Gefühle: [4-20]
    Ästhetisches Empfinden: [4-20]
    Intellektuelle Neugierde: [4-20]
    Kreativer Einfallsreichtum: [4-20]

    // BIS-11 Scores
    Aufmerksamkeit: [0-20]
    Kognitive Instabilität: [0-12]
    Motorische Impulsivität: [0-28]
    Beharrlichkeit: [0-16]
    Selbst Kontrolle: [0-24]
    Kognitive Komplexität: [0-20]

    Given the following tagged measurements:
    """
    for key, value in data.items():
        prompt += f"{key}: {value}\n"
    
    prompt += """
    Based on these measurements, provide only the psychological scores for each category with its name in the format of name: integerValue
    """
    return prompt

def prepare_prompt_ls(data):
    prompt = """
    The following analysis should consider multiple german facets for a better understanding:

    Important Focus point:
    The defined strategy emphasizes the importance of the statement below as a key consideration
    """
    strategy = next((v for k, v in data.items() if "strategy" in k.lower()), "Unknown Strategy")
    prompt += f"strategy: {strategy}\n\n"

    prompt += """
    You are given in addition a set of tagged measurements, which should be interpreted together with the important focus point and mapped to psychological scores based on UPPS, Big Five Inventory, and BIS-11 assessments.
    Present each score as an integer within the specified range, as shown below:

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [12-60]
    Verträglichkeit: [12-60]
    Gewissenhaftigkeit: [12-60]
    Negative Emotionalität: [12-60]
    Offenheit: [12-60]

    // Big Five Facets
    Geselligkeit: [4-20]
    Durchsetzungsfähigkeit: [4-20]
    Aktivität: [4-20]
    Mitgefühl: [4-20]
    Höflichkeit: [4-20]
    Zwischenmenschliches Vertrauen: [4-20]
    Ordnungsliebe: [4-20]
    Fleiß: [4-20]
    Verlässlichkeit: [4-20]
    Ängstlichkeit: [4-20]
    Niedergeschlagenheit: [4-20]
    Unbeständigkeit der Gefühle: [4-20]
    Ästhetisches Empfinden: [4-20]
    Intellektuelle Neugierde: [4-20]
    Kreativer Einfallsreichtum: [4-20]

    // BIS-11 Scores
    Aufmerksamkeit: [0-20]
    Kognitive Instabilität: [0-12]
    Motorische Impulsivität: [0-28]
    Beharrlichkeit: [0-16]
    Selbst Kontrolle: [0-24]
    Kognitive Komplexität: [0-20]

    Given the following tagged measurements:
    """
    prompt += f"\nInput Data: {data}\n"

    prompt += """
    Based on these measurements, provide only the psychological scores for each category with its name in the format of name: integerValue
    """
    return prompt

def prepare_prompt_gb(data):
    prompt = """
    You are an advanced german psychological scoring system tasked with interpreting tagged measurements provided below. These measurements must be mapped to psychological scores according to established frameworks: UPPS, Big Five Inventory, and BIS-11 assessments.

    The interpretation must strictly adhere to the following guidelines:
    - Use the given ranges to calculate the scores.
    - Ensure consistency in mapping the data to their respective traits or dimensions.
    - Provide results as integers within the defined ranges.

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [12-60]
    Verträglichkeit: [12-60]
    Gewissenhaftigkeit: [12-60]
    Negative Emotionalität: [12-60]
    Offenheit: [12-60]

    // Big Five Facets
    Geselligkeit: [4-20]
    Durchsetzungsfähigkeit: [4-20]
    Aktivität: [4-20]
    Mitgefühl: [4-20]
    Höflichkeit: [4-20]
    Zwischenmenschliches Vertrauen: [4-20]
    Ordnungsliebe: [4-20]
    Fleiß: [4-20]
    Verlässlichkeit: [4-20]
    Ängstlichkeit: [4-20]
    Niedergeschlagenheit: [4-20]
    Unbeständigkeit der Gefühle: [4-20]
    Ästhetisches Empfinden: [4-20]
    Intellektuelle Neugierde: [4-20]
    Kreativer Einfallsreichtum: [4-20]

    // BIS-11 Scores
    Aufmerksamkeit: [0-20]
    Kognitive Instabilität: [0-12]
    Motorische Impulsivität: [0-28]
    Beharrlichkeit: [0-16]
    Selbst Kontrolle: [0-24]
    Kognitive Komplexität: [0-20]

    Given the following tagged measurements:
    """
    for key, value in data.items():
        prompt += f"{key}: {value}\n"
    prompt += """
    Based on these measurements, provide only the psychological scores for each category with its name in the format of name: integerValue
    """
    return prompt

def prepare_prompt_tm(data):
    prompt = """
    You are an advanced german psychological scoring system tasked with interpreting tagged measurements provided below. These measurements must be mapped to psychological scores according to established frameworks: UPPS, Big Five Inventory, and BIS-11 assessments.

    The interpretation must strictly adhere to the following guidelines:
    - Use the given ranges to calculate the scores.
    - Ensure consistency in mapping the data to their respective traits or dimensions.
    - Provide results as integers within the defined ranges.

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [12-60]
    Verträglichkeit: [12-60]
    Gewissenhaftigkeit: [12-60]
    Negative Emotionalität: [12-60]
    Offenheit: [12-60]

    // Big Five Facets
    Geselligkeit: [4-20]
    Durchsetzungsfähigkeit: [4-20]
    Aktivität: [4-20]
    Mitgefühl: [4-20]
    Höflichkeit: [4-20]
    Zwischenmenschliches Vertrauen: [4-20]
    Ordnungsliebe: [4-20]
    Fleiß: [4-20]
    Verlässlichkeit: [4-20]
    Ängstlichkeit: [4-20]
    Niedergeschlagenheit: [4-20]
    Unbeständigkeit der Gefühle: [4-20]
    Ästhetisches Empfinden: [4-20]
    Intellektuelle Neugierde: [4-20]
    Kreativer Einfallsreichtum: [4-20]

    // BIS-11 Scores
    Aufmerksamkeit: [0-20]
    Kognitive Instabilität: [0-12]
    Motorische Impulsivität: [0-28]
    Beharrlichkeit: [0-16]
    Selbst Kontrolle: [0-24]
    Kognitive Komplexität: [0-20]

    Given the following tagged measurements:
    """
    for key, value in data.items():
        prompt += f"{key}: {value}\n"
    prompt += """
    Based on these measurements, provide only the psychological scores for each category with its name in the format of name: integerValue
    """
    return prompt


def prepare_prompt_whole_game(data, previous_interpretations):
    prompt = """
    You are an advanced german psychological scoring system tasked with interpreting tagged measurements and unifying them with previously generated psychological scores and interpretations. 
    Your goal is to refine the previous assessments by synthesizing new data with historical data to produce more accurate, nuanced, and logical results.

    The interpretation must strictly adhere to the following guidelines:
    - Use the given ranges to calculate the scores.
    - When unifying scores, prioritize patterns or correlations from both new and previous data rather than relying on majority values.
    - Refine and adjust the previously mapped scores if new evidence justifies it.
    - Provide results **only in the format: ScoreName : ScoreValue**, as integers within the defined ranges.
    - Highlight how the refined scores align with behavioral patterns or tendencies observed.

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [12-60]
    Verträglichkeit: [12-60]
    Gewissenhaftigkeit: [12-60]
    Negative Emotionalität: [12-60]
    Offenheit: [12-60]

    // Big Five Facets
    Geselligkeit: [4-20]
    Durchsetzungsfähigkeit: [4-20]
    Aktivität: [4-20]
    Mitgefühl: [4-20]
    Höflichkeit: [4-20]
    Zwischenmenschliches Vertrauen: [4-20]
    Ordnungsliebe: [4-20]
    Fleiß: [4-20]
    Verlässlichkeit: [4-20]
    Ängstlichkeit: [4-20]
    Niedergeschlagenheit: [4-20]
    Unbeständigkeit der Gefühle: [4-20]
    Ästhetisches Empfinden: [4-20]
    Intellektuelle Neugierde: [4-20]
    Kreativer Einfallsreichtum: [4-20]

    // BIS-11 Scores
    Aufmerksamkeit: [0-20]
    Kognitive Instabilität: [0-12]
    Motorische Impulsivität: [0-28]
    Beharrlichkeit: [0-16]
    Selbst Kontrolle: [0-24]
    Kognitive Komplexität: [0-20]

    After presenting the scores, provide a detailed interpretation of the individual's personality and tendencies, focusing on:
    - Behavioral patterns derived from unified scores.
    - Implications of adjusted scores on individual tendencies.
    - Real-world contexts, including social settings, stressful situations, and risk-prone environments.

    Given the following tagged measurements:
    """
    for key, value in data.items():
        prompt += f"{key}: {value}\n"

    prompt += "\nGiven previous Interpretations:\n"
    for level_name, interpretation in previous_interpretations.items():
        prompt += f"{level_name} Interpretation:\n{interpretation}\n\n"

    prompt += """
    Based on the above measurements and interpretations:
    1. Integrate new measurements with previous scores to refine the psychological scores for each category, ensuring adjustments make logical sense.
    2. Provide psychological scores for each category strictly in the format:
       ScoreName : ScoreValue
    3. Explain the refined scores and their implications on behavioral tendencies and personality traits.
    4. Offer specific strategies and actionable recommendations for self-regulation and decision-making based on the refined scores.
    """
    return prompt

def extract_text_from_pdfs():
    """
    Returns the combined text as a single string.
    """
    folder_path = os.path.join(os.getcwd(), 'Resources')
    pdf_files = ["UPPSGerman.pdf", "PersonalityBigFiveInventory.pdf", "BIS_11.pdf"]

    combined_text = ""

    for pdf_file in pdf_files:
        pdf_path = os.path.join(folder_path, pdf_file)
        try:
            reader = PdfReader(pdf_path)
            for page in reader.pages:
                combined_text += page.extract_text() + "\n"
        except Exception as e:
            print(f"Error reading {pdf_file}: {e}")

    return combined_text

def generate_interpretation_per_level(prompt):
    """
    Generates an interpretation based on the given prompt using OpenAI's GPT-3.5-turbo.
    """
    try:
        openai.api_key = os.getenv("OPENAI_API_KEY")  # Ensure your API key is set in the environment variables --> DONE

        response = openai.ChatCompletion.create(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": "You are a highly skilled psychologist helping to interpret complex data."},
                {"role": "user", "content": prompt}
            ],
            max_tokens=3000,
            temperature=0.2
        )

        interpretation = response['choices'][0]['message']['content']
        print(interpretation)
        return interpretation

    except Exception as e:
        print(f"Error generating interpretation: {e}")
        return None
 
def extract_per_level_interpretation(level_name, interpretation):
    """
    Extracts and returns a vertical vector (column) containing the interpreted segments from the provided interpretation.
    The first cell contains the level name.
    """
    try:
        column = [level_name]
        # Updated regex patterns to match German labels and allow flexible spaces
        patterns = {
            # UPPS Scores (remain in English)
            "Urgency": r"Urgency\s*:\s*(\d+)",
            "Lack of premeditation": r"Lack of premeditation\s*:\s*(\d+)",
            "Lack of perseverance": r"Lack of perseverance\s*:\s*(\d+)",
            "Sensation seeking": r"Sensation seeking\s*:\s*(\d+)",
    
            # Big Five Traits (now using German labels)
            "Extraversion": r"Extraversion\s*:\s*(\d+)",
            "Verträglichkeit": r"Verträglichkeit\s*:\s*(\d+)",
            "Gewissenhaftigkeit": r"Gewissenhaftigkeit\s*:\s*(\d+)",
            "Negative Emotionalität": r"Negative Emotionalität\s*:\s*(\d+)",
            "Offenheit": r"Offenheit\s*:\s*(\d+)",
    
            # Big Five Facets (German)
            "Geselligkeit": r"Geselligkeit\s*:\s*(\d+)",
            "Durchsetzungsfähigkeit": r"Durchsetzungsfähigkeit\s*:\s*(\d+)",
            "Aktivität": r"Aktivität\s*:\s*(\d+)",
            "Mitgefühl": r"Mitgefühl\s*:\s*(\d+)",
            "Höflichkeit": r"Höflichkeit\s*:\s*(\d+)",
            "Zwischenmenschliches Vertrauen": r"Zwischenmenschliches Vertrauen\s*:\s*(\d+)",
            "Ordnungsliebe": r"Ordnungsliebe\s*:\s*(\d+)",
            "Fleiß": r"Fleiß\s*:\s*(\d+)",
            "Verlässlichkeit": r"Verlässlichkeit\s*:\s*(\d+)",
            "Ängstlichkeit": r"Ängstlichkeit\s*:\s*(\d+)",
            "Niedergeschlagenheit": r"Niedergeschlagenheit\s*:\s*(\d+)",
            "Unbeständigkeit der Gefühle": r"Unbeständigkeit der Gefühle\s*:\s*(\d+)",
            "Ästhetisches Empfinden": r"Ästhetisches Empfinden\s*:\s*(\d+)",
            "Intellektuelle Neugierde": r"Intellektuelle Neugierde\s*:\s*(\d+)",
            "Kreativer Einfallsreichtum": r"Kreativer Einfallsreichtum\s*:\s*(\d+)",
    
            # BIS-11 Scores (German)
            "Aufmerksamkeit": r"Aufmerksamkeit\s*:\s*(\d+)",
            "Kognitive Instabilität": r"Kognitive Instabilität\s*:\s*(\d+)",
            "Motorische Impulsivität": r"Motorische Impulsivität\s*:\s*(\d+)",
            "Beharrlichkeit": r"Beharrlichkeit\s*:\s*(\d+)",
            "Selbst Kontrolle": r"Selbst Kontrolle\s*:\s*(\d+)",
            "Kognitive Komplexität": r"Kognitive Komplexität\s*:\s*(\d+)"
        }


        for key, pattern in patterns.items():
            match = re.search(pattern, interpretation)
            if match:
                column.append(match.group(1))
            else:
                column.append("N/A")
        return column

    except Exception as e:
        print(f"Error extracting interpretation for {level_name}: {e}")
        return [level_name] + ["N/A"] * 31

def interpret_participant_data(file_path):
    """
    Reads a single participant data file, generates prompts for each level,
    interprets them, and returns a combined array for that file.
    If a level contains empty strings as variable names, it is skipped.
    """
    combined_array = [[
        "Scores",
        "Urgency",
        "Lack of premeditation",
        "Lack of perseverance",
        "Sensation seeking",
        "Extraversion",
        "Verträglichkeit",
        "Gewissenhaftigkeit",
        "Negative Emotionalität",
        "Offenheit",
        "Geselligkeit",
        "Durchsetzungsfähigkeit",
        "Aktivität",
        "Mitgefühl",
        "Höflichkeit",
        "Zwischenmenschliches Vertrauen",
        "Ordnungsliebe",
        "Fleiß",
        "Verlässlichkeit",
        "Ängstlichkeit",
        "Niedergeschlagenheit",
        "Unbeständigkeit der Gefühle",
        "Ästhetisches Empfinden",
        "Intellektuelle Neugierde",
        "Kreativer Einfallsreichtum",
        "Aufmerksamkeit",
        "Kognitive Instabilität",
        "Motorische Impulsivität",
        "Beharrlichkeit",
        "Selbst Kontrolle",
        "Kognitive Komplexität"
    ]]

    previous_interpretations = {}

    # Load the JSON file
    try:
        with open(file_path, 'r', encoding='utf-8') as json_file:
            raw_data = json.load(json_file)

        if "Rows" not in raw_data:
            raise ValueError("The JSON file does not contain the required 'Rows' key.")
        
        # Convert JSON structure into a DataFrame
        data = pd.DataFrame(raw_data["Rows"])
    except Exception as e:
        print(f"Error reading JSON file '{file_path}': {e}")
        return combined_array  # Return header only in case of error

    # Ensure column names exist
    required_columns = ["Column1", "Column2", "Column3", "Column4", "Column5", "Column6", "Column7", "Column8", "Column9", "Column10"]
    for column in required_columns:
        if column not in data.columns:
            data[column] = None  # Add missing columns as empty

    levels = {
        "PUW": ["Column1", "Column2"],
        "LS": ["Column3", "Column4"],
        "GB": ["Column5", "Column6"],
        "TM": ["Column7", "Column8"],
        "WholeGame": []  # Special handling for WholeGame
    }

    # Iterate through levels and process their respective columns
    for level_name, columns in levels.items():
        measurements = {}

        # Extract measurements for the current level
        for _, row in data.iterrows():
            if len(columns) >= 2 and row[columns[0]]:
                measurements[row[columns[0]]] = row[columns[1]]

        if not measurements and level_name != "WholeGame":
            print(f"No valid measurements for level {level_name}. Skipping.")
            combined_array.append([level_name] + ["N/A"] * 30)
            continue

        try:
            # Handle WholeGame separately
            if level_name == "WholeGame":
                wg_prompt = prepare_prompt_whole_game(measurements, previous_interpretations)
                wg_interpretation = generate_interpretation_per_level(wg_prompt)
                wg_column = extract_per_level_interpretation("WholeGame", wg_interpretation)
                combined_array.append(wg_column)
            else:
                # Call the appropriate prompt preparation function
                if level_name == "PUW":
                    prompt = prepare_prompt_puw(measurements)
                elif level_name == "LS":
                    prompt = prepare_prompt_ls(measurements)
                elif level_name == "GB":
                    prompt = prepare_prompt_gb(measurements)
                elif level_name == "TM":
                    prompt = prepare_prompt_tm(measurements)

                interpretation = generate_interpretation_per_level(prompt)
                previous_interpretations[level_name] = interpretation
                column = extract_per_level_interpretation(level_name, interpretation)
                combined_array.append(column)
        except Exception as e:
            print(f"Error processing level '{level_name}': {e}")
            combined_array.append([level_name] + ["N/A"] * 30)


    return combined_array

def load_and_save_interpretation():
    """
    Loads all participant data files, interprets them, and saves the results as JSON files.
    """
    input_folder = os.path.join(os.getcwd(), 'NormalizedData')
    output_folder = os.path.join(os.getcwd(), 'ParticipantPredictedResults')
    os.makedirs(output_folder, exist_ok=True)

    files = [f for f in os.listdir(input_folder) if f.startswith("AD") or f.startswith("CG")]

    for file in files:
        file_path = os.path.join(input_folder, file)
        combined_array = interpret_participant_data(file_path)

        output_path = os.path.join(output_folder, f"{os.path.splitext(file)[0]}.json")
        with open(output_path, 'w', encoding='utf-8') as json_file:
            json.dump(combined_array, json_file, indent=4, ensure_ascii=False)
    
def main():
    load_and_save_interpretation()

if __name__ == "__main__":
    main()