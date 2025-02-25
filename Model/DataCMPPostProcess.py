import os
import json
import csv
import numpy as np
from scipy.stats import multivariate_normal, ttest_1samp
from statsmodels.stats.power import TTestPower

# Define folder paths
predicted_folder = os.path.join(os.getcwd(), 'ParticipantPredictedResults')
real_folder = os.path.join(os.getcwd(), 'ParticipantRealResults')

# Ensure folders exist
def ensure_folders_exist():
    if not os.path.exists(predicted_folder) or not os.path.exists(real_folder):
        raise FileNotFoundError("One or both folders do not exist.")

# Value ranges for each score
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



def validate_value(value, score_name, real_values=None):
    if callable(define_ranges[score_name][0]):
        dependency = define_ranges[score_name][1]
        dependent_value = real_values.get(dependency, None)
        if dependent_value is None:
            raise ValueError(f"Dependency {dependency} not found for {score_name}.")
        min_val, max_val = define_ranges[score_name][0](dependent_value), dependent_value
    else:
        min_val, max_val = define_ranges[score_name]
    return min_val <= value <= max_val





# data analytics operations
# 1. Perform T-squared tests to check multivariate discrepancies.                                               --> DONE
# 2. Apply Chi-squared tests for independence between observed and predicted categories.
# 3. Compute p-values for statistical significance in discrepancies.                                            --> DONE
# 4. Use correlation metrics to understand the relationship between ground truth and predictions.
# 5. Evaluate model performance using RMSE (Root Mean Square Error).
# 6. Use ANOVA for assessing differences in variance across multiple datasets. ( comparing predictions across groups - later when i have AD too)
# 7. Implement Mahalanobis distance to detect outliers in multivariate data.
import numpy as np

import numpy as np
from sklearn.covariance import LedoitWolf


#


def calculate_differences(predicted_data, real_data):
    """ Computes absolute differences between predicted and real scores."""
    differences = []
    row_differences = {}
    for score_name, predicted_value in predicted_data.items():
        real_value = real_data.get(score_name)
        if real_value is not None:
            row_differences[score_name] = abs(predicted_value - real_value)
        else:
            print(f"Score {score_name} not found in real data.")
    differences.append(row_differences)
    return differences

def perform_t_squared_test(all_differences):
    if not isinstance(all_differences, list) or not all_differences:
        print("No differences available for T-squared test.")
        return [None] * len(all_differences)
    differences_array = np.array([list(diff.values()) for diff in all_differences if diff]).reshape(-1, len(all_differences[0]))
    lw = LedoitWolf()
    cov_matrix = lw.fit(differences_array).covariance_
    try:
        inv_cov_matrix = np.linalg.inv(cov_matrix)
    except np.linalg.LinAlgError:
        inv_cov_matrix = np.linalg.pinv(cov_matrix)
    mean_vector = np.mean(differences_array, axis=0)
    return [np.dot(np.dot((row - mean_vector), inv_cov_matrix), (row - mean_vector).T) for row in differences_array]

def compute_p_values(all_differences):
    p_values = []
    if not all_differences:
        return [None]
    differences_array = np.array([list(diff.values()) for diff in all_differences if diff]).flatten()
    for score_idx in range(differences_array.shape[0]):
        t_stat, p_val = ttest_1samp(differences_array[score_idx::len(differences_array)], 0)
        p_values.append(p_val)
    return p_values

def calculate_sample_size(current_differences, alpha=0.05, power=0.8):
    if not current_differences:
        return None
    differences_array = np.array([list(diff.values()) for diff in current_differences if diff]).flatten()
    mean_diff = np.mean(differences_array)
    std_diff = np.std(differences_array, ddof=1)
    effect_size = mean_diff / std_diff if std_diff != 0 else 0
    analysis = TTestPower()
    sample_size = analysis.solve_power(effect_size=effect_size, alpha=alpha, power=power, alternative='two-sided')
    return round(sample_size)

#
#
#
def load_predicted_json(file_path):
    """ Reads the predicted JSON file and extracts score names and values."""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)
        
        if isinstance(data, dict):
            return {key: float(value) if isinstance(value, (int, float, str)) and str(value).replace('.', '', 1).isdigit() else value 
                    for key, value in data.items()}  # Extract score names and values
        else:
            print(f"Unexpected format in predicted file: {file_path}")
            return None
    
    except Exception as e:
        print(f"Error reading predicted JSON file '{file_path}': {e}")
        return None

def load_real_json(file_path):
    """ Reads the real JSON file and returns a dictionary of values."""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)

        if isinstance(data, dict) and "dataArray" in data and isinstance(data["dataArray"], list):
            return {item["scoreName"]: float(item["scoreValue"]) if isinstance(item["scoreValue"], (int, float, str)) and str(item["scoreValue"]).replace('.', '', 1).isdigit() else item["scoreValue"] 
                    for item in data["dataArray"]}
        elif isinstance(data, dict):
            return {key: value for key, value in data.items() if isinstance(value, (int, float))}
        else:
            print(f"Unexpected real data format in: {file_path}")
            return None
    except json.JSONDecodeError:
        print(f"Error: JSON decoding failed for real file: {file_path}")
        return None
    except Exception as e:
        print(f"Error reading real JSON file '{file_path}': {e}")
        return None



def process_files():
    comparison_results = []
    all_differences = []

    for filename in os.listdir(predicted_folder):
        predicted_path = os.path.join(predicted_folder, filename)
        real_path = os.path.join(real_folder, filename)

        if not os.path.exists(real_path):
            print(f"Real file missing for: {filename}")
            comparison_results.append({"Filename": filename, "Status": "Missing real file"})
            continue

        # Load predicted and real data
        predicted_data = load_predicted_json(predicted_path)
        real_data = load_real_json(real_path)

        # Ensure data is valid
        if predicted_data is None or real_data is None:
            print(f"Skipping comparison due to invalid data in: {filename}")
            continue

        # Compare and calculate differences
        differences = calculate_differences(predicted_data, real_data)
        
        if differences:
            all_values = [list(row.values()) for row in differences]  # Convert to list of lists
            avg_diff = np.mean([val for sublist in all_values for val in sublist]) if all_values else 0
            max_diff = np.max([val for sublist in all_values for val in sublist]) if all_values else 0
        else:
            avg_diff = 0
            max_diff = 0
        
        all_differences.extend(all_values)
        comparison_results.append({"Filename": filename, "Average Difference": avg_diff, "Maximum Difference": max_diff})
 
    print(all_differences)
    # Perform statistical analysis
    t_squared_values = perform_t_squared_test(all_differences)
    p_values = compute_p_values(all_differences)
    sample_size = calculate_sample_size(all_differences)
    if isinstance(sample_size, np.ndarray):
        if sample_size.size == 1:  # Single value array
            sample_size = sample_size.item()
        else:
            sample_size = sample_size.mean()  # Or other appropriate operation
    # Round the scalar sample size
    sample_size = round(sample_size)
    print(f"Required sample size for significance: {sample_size}")

    # Attach t-squared values and p-values to results
    for idx in range(len(comparison_results)):
        comparison_results[idx]["T-squared Value"] = t_squared_values[idx] if idx < len(t_squared_values) else None
        comparison_results[idx]["P-value"] = p_values[idx] if idx < len(p_values) else None

    return comparison_results


def export_results_to_csv(results):
    output_file = os.path.join(os.getcwd(), "ComparisonResults.csv")

    # Define only the expected fields for CSV
    fieldnames = ["Filename", "Average Difference", "Maximum Difference", "T-squared Value", "P-value"]

    # Remove extra fields from results before writing to CSV
    cleaned_results = [{k: v for k, v in row.items() if k in fieldnames} for row in results]

    with open(output_file, mode='w', newline='') as csv_file:
        writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(cleaned_results)

    print(f"Comparison results saved to {output_file}")


def main():
    ensure_folders_exist()
    results = process_files()
    export_results_to_csv(results)
    
if __name__ == "__main__":
    main()