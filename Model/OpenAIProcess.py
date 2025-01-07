import os
from PyPDF2 import PdfReader
import openai
import re
import csv
import json

def prepare_prompt_puw(data):
    prompt = """
    You are given a set of tagged measurements, which should be interpreted and mapped to psychological scores based on UPPS, Big Five Inventory, and BIS-11 assessments.
    Present each score as an integer within the specified range, as shown below:

    // UPPS Scores
    Urgency: [11-44]
    Lack of premeditation: [10-40]
    Lack of perseverance: [10-40]
    Sensation seeking: [12-48]

    // Big Five Traits
    Extraversion: [8-40]
    Introversion: [40-Extraversion]
    Agreeableness: [9-45]
    Antagonism: [45-Agreeableness]
    Conscientiousness: [9-45]
    Lack of direction: [45-Conscientiousness]
    Neuroticism: [8-40]
    Emotional stability: [40-Neuroticism]
    Openness: [10-50]
    Closedness to experience: [50-Openness]

    // BIS-11 Scores
    Attention score: [0-20]
    Cognitive Instability score: [0-12]
    Motor Scores: [0-28]
    Perseverance scores: [0-16]
    Self-Control scores: [0-24]
    Cognitive Complexity scores: [0-20]

    After presenting the scores, provide a detailed interpretation of the individual's personality and tendencies, including behavioral predictions and solutions in the following scenarios:
    Social settings
    Sad situations
    Stressful situations
    Peaceful situations
    Situations under pressure
    Overwhelming situations
    Complex situations
    Reward-triggering environments
    Risk-prone environments

    Additionally, offer coping strategies and practical recommendations to help the individual manage challenges in these areas. Do not repeat any parts of the response.

    Given the following tagged measurements:
    """
    for key, value in data.items():
        prompt += f"{key}: {value}\n"
    return prompt

def prepare_prompt_ls(data):
    prompt = """
    The following analysis should consider multiple facets for a better understanding:

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
    Extraversion: [8-40]
    Introversion: [40-Extraversion]
    Agreeableness: [9-45]
    Antagonism: [45-Agreeableness]
    Conscientiousness: [9-45]
    Lack of direction: [45-Conscientiousness]
    Neuroticism: [8-40]
    Emotional stability: [40-Neuroticism]
    Openness: [10-50]
    Closedness to experience: [50-Openness]

    // BIS-11 Scores
    Attention score: [0-20]
    Cognitive Instability score: [0-12]
    Motor Scores: [0-28]
    Perseverance scores: [0-16]
    Self-Control scores: [0-24]
    Cognitive Complexity scores: [0-20]

    After presenting the scores, provide a detailed interpretation of the individual's personality and tendencies, including behavioral predictions and solutions in the following scenarios:
    Social settings
    Sad situations
    Stressful situations
    Peaceful situations
    Situations under pressure
    Overwhelming situations
    Complex situations
    Reward-triggering environments
    Risk-prone environments

    Additionally, offer coping strategies and practical recommendations to help the individual manage challenges in these areas. Do not repeat any parts of the response.

    Given the following tagged measurements:
    """
    prompt += f"\nInput Data: {data}\n"
    return prompt

def prepare_prompt_gb(data):
    prompt = """
    
    """
    prompt += f"\nInput Data: {data}\n"
    return prompt

def prepare_prompt_tm(data):
    prompt = """
    
    """
    prompt += f"\nInput Data: {data}\n"
    return prompt

def prepare_prompt_whole_game(data):
    prompt = """
        combining PUW, LS, GG, TM, PUS and total progress
    """
    prompt += f"\nInput Data: {data}\n"
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
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": "You are a highly skilled psychologist helping to interpret complex data."},
                {"role": "user", "content": prompt}
            ],
            max_tokens=3000,
            temperature=0.2
        )

        interpretation = response['choices'][0]['message']['content']
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
        # Initialize the column with the level name
        column = [level_name]

        # Regular expressions to extract scores
        patterns = {
            "Urgency": r"Urgency: \[(\d+)\]",
            "Lack of premeditation": r"Lack of premeditation: \[(\d+)\]",
            "Lack of perseverance": r"Lack of perseverance: \[(\d+)\]",
            "Sensation seeking": r"Sensation seeking: \[(\d+)\]",
            "Extraversion": r"Extraversion: \[(\d+)\]",
            "Introversion": r"Introversion: \[(\d+)\]",
            "Agreeableness": r"Agreeableness: \[(\d+)\]",
            "Antagonism": r"Antagonism: \[(\d+)\]",
            "Conscientiousness": r"Conscientiousness: \[(\d+)\]",
            "Lack of direction": r"Lack of direction: \[(\d+)\]",
            "Neuroticism": r"Neuroticism: \[(\d+)\]",
            "Emotional stability": r"Emotional stability: \[(\d+)\]",
            "Openness": r"Openness: \[(\d+)\]",
            "Closedness to experience": r"Closedness to experience: \[(\d+)\]",
            "Attention score": r"Attention score: \[(\d+)\]",
            "Cognitive Instability score": r"Cognitive Instability score: \[(\d+)\]",
            "Motor Scores": r"Motor Scores: \[(\d+)\]",
            "Perseverance scores": r"Perseverance scores: \[(\d+)\]",
            "Self-Control scores": r"Self-Control scores: \[(\d+)\]",
            "Cognitive Complexity scores": r"Cognitive Complexity scores: \[(\d+)\]"
        }

        # Extract scores based on patterns
        for key, pattern in patterns.items():
            match = re.search(pattern, interpretation)
            if match:
                column.append(match.group(1))
            else:
                column.append("N/A")  # Add "N/A" if the value is not found

        return column

    except Exception as e:
        print(f"Error saving interpretation: {e}")
        return None

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
        "Introversion",
        "Agreeableness",
        "Antagonism",
        "Conscientiousness",
        "Lack of direction",
        "Neuroticism",
        "Emotional stability",
        "Openness",
        "Closedness to experience",
        "Attention score",
        "Cognitive Instability score",
        "Motor Scores",
        "Perseverance scores",
        "Self-Control scores",
        "Cognitive Complexity scores"
    ]]

    with open(file_path, 'r') as f:
        reader = csv.reader(f)
        for row in reader:
            levels_data = {
                "PUW": {row[0]: row[1]} if row[0] else None,
                "LS": {row[2]: row[3]} if row[2] else None,
                "GB": {row[4]: row[5]} if row[4] else None,
                "TM": {row[6]: row[7]} if row[6] else None,
                "WholeGame": {row[8]: row[9]} if row[8] else None
            }

            for level_name, data in levels_data.items():
                if data is None:
                    combined_array.append([level_name] + ["0"] * 20)
                else:
                    if level_name == "PUW":
                        prompt = prepare_prompt_puw(data)
                    elif level_name == "LS":
                        prompt = prepare_prompt_ls(data)
                    elif level_name == "GB":
                        prompt = prepare_prompt_gb(data)
                    elif level_name == "TM":
                        prompt = prepare_prompt_tm(data)
                    elif level_name == "WholeGame":
                        prompt = prepare_prompt_whole_game(data)

                    interpretation = generate_interpretation_per_level(prompt)
                    column = extract_per_level_interpretation(level_name, interpretation)
                    combined_array.append(column)

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
        with open(output_path, 'w') as json_file:
            json.dump(combined_array, json_file, indent=4)
    
def main():
    load_and_save_interpretation()

if __name__ == "__main__":
    main()