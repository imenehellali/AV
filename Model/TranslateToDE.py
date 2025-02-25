import os
import json

def rename_and_save_json_files(folder_path):
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
    
    for filename in os.listdir(folder_path):
        if filename.endswith(".json"):
            file_path = os.path.join(folder_path, filename)
            with open(file_path, "r", encoding="utf-8") as file:
                data = json.load(file)
                
                for entry in data.get("dataArray", []):
                    if entry["scoreName"] in mapping:
                        entry["scoreName"] = mapping[entry["scoreName"]]
            
            with open(file_path, "w", encoding="utf-8") as file:
                json.dump(data, file, ensure_ascii=False, indent=4)
                
rename_and_save_json_files("ParticipantPredictedResults")
