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
    "Agreeableness": (12, 60),
    "Conscientiousness": (12, 60),
    "Neuroticism": (12, 60),
    "Openness": (12, 60),

    # Big Five Facets
    "Sociability": (4, 20),
    "Assertiveness": (4, 20),
    "Activity": (4, 20),
    "Compassion": (4, 20),
    "Politeness": (4, 20),
    "Interpersonal Trust": (4, 20),
    "Orderliness": (4, 20),
    "Diligence": (4, 20),
    "Reliability": (4, 20),
    "Anxiety": (4, 20),
    "Depression": (4, 20),
    "Emotional Instability": (4, 20),
    "Aesthetic Sensitivity": (4, 20),
    "Intellectual Curiosity": (4, 20),
    "Creative Imagination": (4, 20),

    # BIS-11 Scores
    "Attention score": (0, 20),
    "Cognitive Instability score": (0, 12),
    "Motor Scores": (0, 28),
    "Perseverance scores": (0, 16),
    "Self-Control scores": (0, 24),
    "Cognitive Complexity scores": (0, 20)
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

def calculate_differences(predicted_data, real_data):
    differences = []

    # Extract real rows from the "dataArray" key
    real_rows = real_data.get("dataArray", [])
    if not real_rows:
        print("Error: 'dataArray' key missing or empty in real data.")
        return differences

    # Transform real data into a dictionary for easier access
    real_data_dict = {item["scoreName"]: item["scoreValue"] for item in real_rows}

    # Iterate through rows of predicted data
    for i, predicted_row in enumerate(predicted_data):
        row_differences = {}

        # Calculate differences for each score in the predicted row
        for score_name, predicted_value in predicted_row.items():
            if score_name in real_data_dict:
                real_value = real_data_dict[score_name]
                if not validate_value(predicted_value, score_name, real_values=real_data_dict):
                    print(f"Validation failed for row {i + 1}, column {score_name}: Predicted={predicted_value}, Real={real_value}")
                difference = abs(predicted_value - real_value)
                row_differences[score_name] = difference
            else:
                print(f"Score {score_name} not found in real data.")

        differences.append(row_differences)

    return differences



# data analytics operations
# 1. Perform T-squared tests to check multivariate discrepancies.                                               --> DONE
# 2. Apply Chi-squared tests for independence between observed and predicted categories.
# 3. Compute p-values for statistical significance in discrepancies.                                            --> DONE
# 4. Use correlation metrics to understand the relationship between ground truth and predictions.
# 5. Evaluate model performance using RMSE (Root Mean Square Error).
# 6. Use ANOVA for assessing differences in variance across multiple datasets. ( comparing predictions across groups - later when i have AD too)
# 7. Implement Mahalanobis distance to detect outliers in multivariate data.
def perform_t_squared_test(all_differences):
    differences_array = np.array(all_differences).reshape(-1, len(define_ranges))
    mean_vector = np.mean(differences_array, axis=0)
    cov_matrix = np.cov(differences_array, rowvar=False)

    if np.linalg.det(cov_matrix) == 0:
        print("Covariance matrix is singular; T-squared test cannot be performed.")
        return [None] * len(differences_array)
    else:
        inv_cov_matrix = np.linalg.inv(cov_matrix)
        return [
            np.dot(np.dot((row - mean_vector), inv_cov_matrix), (row - mean_vector).T)
            for row in differences_array
        ]
    
def compute_p_values(all_differences):
    p_values = []
    differences_array = np.array(all_differences).flatten()
    for score_idx in range(len(define_ranges)):
        score_differences = differences_array[score_idx::len(define_ranges)]
        t_stat, p_val = ttest_1samp(score_differences, 0)
        p_values.append(p_val)
    return p_values
#
#
#
def calculate_sample_size(current_differences, alpha=0.05, power=0.8):
    # Compute mean and standard deviation of current differences
    mean_diff = np.mean(current_differences)
    std_diff = np.std(current_differences, ddof=1)

    # Effect size (Cohen's d)
    effect_size = mean_diff / std_diff

    # Initialize power analysis
    analysis = TTestPower()
    sample_size = analysis.solve_power(effect_size=effect_size, alpha=alpha, power=power, alternative='two-sided')

    return sample_size
#
#
#
def load_predicted_json(file_path):
    """
    Reads and normalizes the ParticipantPredictedResults JSON file.
    Extracts a dictionary of score names and values.
    """
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)

        if isinstance(data, list) and len(data) > 1:
            header = data[0]  # First row contains score names
            values = data[-1]  # Last row contains the corresponding values
            return {header[i]: int(values[i]) if values[i] != "N/A" else None for i in range(1, len(header))}
        else:
            print(f"Unexpected format in predicted file: {file_path}")
            return None

    except Exception as e:
        print(f"Error reading predicted JSON file '{file_path}': {e}")
        return None


def load_real_json(file_path):
    """
    Reads and normalizes the ParticipantRealResults JSON file.
    Extracts a dictionary of score names and values.
    """
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)

        if isinstance(data, dict) and "dataArray" in data:
            return {item["scoreName"]: item["scoreValue"] for item in data["dataArray"]}
        else:
            print(f"Unexpected format in real file: {file_path}")
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

        # Check if the corresponding real file exists
        if not os.path.exists(real_path):
            print(f"Real file missing for: {filename}")
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
            all_values = [d for row in differences for d in row.values()]
            avg_diff = np.mean(all_values) if all_values else 0
            max_diff = np.max(all_values) if all_values else 0
        else:
            avg_diff = 0
            max_diff = 0
        comparison_results.append({"Filename": filename, "Average Difference": avg_diff, "Maximum Difference": max_diff})
        all_differences.extend([d for row in differences for d in row.values()])

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
    for idx, t_val in enumerate(t_squared_values):
        if idx < len(comparison_results):
            comparison_results[idx]["T-squared Value"] = t_val

    for idx, p_val in enumerate(p_values):
        if idx < len(comparison_results):
            comparison_results[idx]["P-value"] = p_val

    return comparison_results


def export_results_to_csv(results):
    output_file = os.path.join(os.getcwd(), "ComparisonResults.csv")
    with open(output_file, mode='w', newline='') as csv_file:
        fieldnames = ["Filename", "Average Difference", "Maximum Difference", "T-squared Value", "P-value"]
        writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(results)
    print(f"Comparison results saved to {output_file}")

def main():
    ensure_folders_exist()
    results = process_files()
    export_results_to_csv(results)
    
if __name__ == "__main__":
    main()