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
    "Urgency": (11, 44),
    "Lack of premeditation": (10, 40),
    "Lack of perseverance": (10, 40),
    "Sensation seeking": (12, 48),
    "Extraversion": (8, 40),
    "Introversion": (lambda x: 40 - x, "Extraversion"),
    "Agreeableness": (9, 45),
    "Antagonism": (lambda x: 45 - x, "Agreeableness"),
    "Conscientiousness": (9, 45),
    "Lack of direction": (lambda x: 45 - x, "Conscientiousness"),
    "Neuroticism": (8, 40),
    "Emotional stability": (lambda x: 40 - x, "Neuroticism"),
    "Openness": (10, 50),
    "Closedness to experience": (lambda x: 50 - x, "Openness"),
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
    for i, row in enumerate(predicted_data):
        row_differences = {}
        for score_name, predicted_value in row.items():
            real_value = real_data[i][score_name]
            if not validate_value(predicted_value, score_name, real_values=row):
                print(f"Validation failed for row {i + 1}, column {score_name}: Predicted={predicted_value}, Real={real_value}")
            difference = abs(predicted_value - real_value)
            row_differences[score_name] = difference
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

    return round(sample_size)
#
#
#
def process_files():
    comparison_results = []
    all_differences = []

    for filename in os.listdir(predicted_folder):
        predicted_path = os.path.join(predicted_folder, filename)
        real_path = os.path.join(real_folder, filename)

        if not os.path.exists(real_path):
            print(f"Real file missing for: {filename}")
            continue

        with open(predicted_path, 'r') as f:
            predicted_data = json.load(f)

        with open(real_path, 'r') as f:
            real_data = json.load(f)

        differences = calculate_differences(predicted_data, real_data)
        avg_diff = np.mean([d for row in differences for d in row.values()])
        max_diff = np.max([d for row in differences for d in row.values()])
        comparison_results.append({"Filename": filename, "Average Difference": avg_diff, "Maximum Difference": max_diff})
        all_differences.extend([d for row in differences for d in row.values()])

    t_squared_values = perform_t_squared_test(all_differences)
    p_values = compute_p_values(all_differences)
    sample_size = calculate_sample_size(all_differences)
    print(f"Required sample size for significance: {sample_size}")

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