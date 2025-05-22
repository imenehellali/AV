import os
import json
import math
from typing import Dict, Any

def normalize_puw_input_data(
    input_data: Dict[str, float],
    max_coins_quantity: float, max_coins_bought: float,
    max_non_reward_drinks: float, max_reward_drinks: float,
    max_diamond_plays: float, max_bill_plays: float,
    max_cake_plays: float, max_mystery_plays: float,
    mean_rate: float, std_dev_rate: float,
    max_fixation_time: float
) -> Dict[str, float]:
    
    normalized_data = {}

    for key, value in input_data.items():
        if key == "CoinsQuantity":
            normalized_data[key] = value / max_coins_quantity if max_coins_quantity else 0
        elif key == "CoinsBoughtCount":
            normalized_data[key] = value / max_coins_bought if max_coins_bought else 0
        elif key == "NonRewardDrinksBoughtCount":
            normalized_data[key] = value / max_non_reward_drinks if max_non_reward_drinks else 0
        elif key == "RewardDrinksBoughtCount":
            normalized_data[key] = value / max_reward_drinks if max_reward_drinks else 0
        elif key == "GetDiamondSlotPlayCount":
            normalized_data[key] = value / max_diamond_plays if max_diamond_plays else 0
        elif key == "BillSlotPlayCount":
            normalized_data[key] = value / max_bill_plays if max_bill_plays else 0
        elif key == "CakeSlotPlayCount":
            normalized_data[key] = value / max_cake_plays if max_cake_plays else 0
        elif key == "MysterySlotPlayCount":
            normalized_data[key] = value / max_mystery_plays if max_mystery_plays else 0
        elif key in [
            "MysterySlotPlaysPerMinute", "DiamondSlotPlaysPerMinute", "BillSlotPlaysPerMinute"
        ]:
            normalized_data[key] = (value - mean_rate) / std_dev_rate if std_dev_rate else 0
        elif key in [
            "AvgFixationTimeAlcoholicDisplays", "AvgFixationTimeAlcoholicVsNonAlcoholic",
            "AvgTimePlayingDiamondMachine", "AvgTimePlayingBillMachine",
            "AvgTimePlayingMysteryMachine", "AvgTimePlayingCakeMachine",
            "AvgStagnantTime"
        ]:
            normalized_data[key] = value / max_fixation_time if max_fixation_time else 0
        else:
            normalized_data[key] = value

    return normalized_data

def normalize_ls_input_data(input_data: Dict[str, Any], max_progress: float, max_time_spent: float) -> Dict[str, Any]:
    normalized_data = {}

    for key, value in input_data.items():
        if key.startswith("ProgressOfSavingWithin") or key.startswith("AvgOfProgressOf"):
            normalized_data[key] = float(value) / max_progress
        elif key.startswith("TimeSpentOn"):
            normalized_data[key] = float(value) / max_time_spent
        else:
            normalized_data[key] = value

    return normalized_data

def normalize_gb_input_data(
    input_data: Dict[str, float],
    max_reaction_time: float,
    max_gaze_time: float,
    max_busted_ghosts: float,
    mean_reaction_time: float,
    std_dev_reaction_time: float
) -> Dict[str, float]:
    
    normalized_data = {}

    for key, value in input_data.items():
        if "ReactionTime" in key:
            normalized_data[key] = value / max_reaction_time if max_reaction_time > 0 else 0
        elif "TimeGazeOn" in key:
            normalized_data[key] = value / max_gaze_time if max_gaze_time > 0 else 0
        elif "Number" in key:
            normalized_data[key] = value / max_busted_ghosts if max_busted_ghosts > 0 else 0
        elif "incDecMargin" in key:
            normalized_data[key] = (value - mean_reaction_time) / std_dev_reaction_time if std_dev_reaction_time > 0 else 0
        else:
            normalized_data[key] = value

    return normalized_data


def normalize_tm_input_data(input_data: Dict[str, Any]) -> Dict[str, Any]:
    normalized_data = {}

    for key, value in input_data.items():
        if "PercentageOfProgressPath" in key or "IncreaseOrDecreaseLearningFactorOverPath" in key:
            try:
                float_value = float(value)
                normalized_data[key] = (float_value + 1) / 2.0
            except ValueError:
                normalized_data[key] = value
        else:
            normalized_data[key] = value

    return normalized_data

def normalize_wg_input_data(input_data: Dict[str, float], max_instruction_checks: float, max_safe_account_sum: float, max_non_reward_purchases: float, max_reward_purchases: float) -> Dict[str, float]:
    normalized_data = {}

    for key, value in input_data.items():
        if "CheckedNumberOFTimesInstruction" in key:
            normalized_data[key] = value / max_instruction_checks if max_instruction_checks else 0
        elif "SafeAccountSumLevel" in key:
            normalized_data[key] = value / max_safe_account_sum if max_safe_account_sum else 0
        elif key == "NonRewardDrinksBoughtCount":
            normalized_data[key] = value / max_non_reward_purchases if max_non_reward_purchases else 0
        elif key == "RewardDrinksBoughtCount":
            normalized_data[key] = value / max_reward_purchases if max_reward_purchases else 0
        else:
            normalized_data[key] = value

    return normalized_data

def calculate_puw_normalization_variables(raw_data_folder: str):
    max_coins_quantity = max_coins_bought = 0
    max_non_reward_drinks = max_reward_drinks = 0
    max_diamond_plays = max_bill_plays = max_cake_plays = max_mystery_plays = 0
    max_fixation_time = 0
    mean_rate = std_dev_rate = 0

    play_rates = []

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                raw_data = json.load(file).get("Rows", [])

            for row in raw_data:
                key = row.get("Column1", "")
                value = row.get("Column2", 0)

                if key == "CoinsQuantity":
                    max_coins_quantity = max(max_coins_quantity, float(value))
                elif key == "CoinsBoughtCount":
                    max_coins_bought = max(max_coins_bought, float(value))
                elif key == "NonRewardDrinksBoughtCount":
                    max_non_reward_drinks = max(max_non_reward_drinks, float(value))
                elif key == "RewardDrinksBoughtCount":
                    max_reward_drinks = max(max_reward_drinks, float(value))
                elif key == "GetDiamondSlotPlayCount":
                    max_diamond_plays = max(max_diamond_plays, float(value))
                elif key == "BillSlotPlayCount":
                    max_bill_plays = max(max_bill_plays, float(value))
                elif key == "CakeSlotPlayCount":
                    max_cake_plays = max(max_cake_plays, float(value))
                elif key == "MysterySlotPlayCount":
                    max_mystery_plays = max(max_mystery_plays, float(value))
                elif key in [
                    "MysterySlotPlaysPerMinute", "DiamondSlotPlaysPerMinute", "BillSlotPlaysPerMinute"
                ]:
                    play_rates.append(float(value))
                elif key in [
                    "AvgFixationTimeAlcoholicDisplays", "AvgFixationTimeAlcoholicVsNonAlcoholic",
                    "AvgTimePlayingDiamondMachine", "AvgTimePlayingBillMachine",
                    "AvgTimePlayingMysteryMachine", "AvgTimePlayingCakeMachine", "AvgStagnantTime"
                ]:
                    max_fixation_time = max(max_fixation_time, float(value))
                
    # Calculate mean and standard deviation for slot play rates
    mean_rate = sum(play_rates) / len(play_rates) if play_rates else 0
    variance = sum((x - mean_rate) ** 2 for x in play_rates) / len(play_rates) if play_rates else 0
    std_dev_rate = math.sqrt(variance)

    return {
        "max_coins_quantity": max_coins_quantity,
        "max_coins_bought": max_coins_bought,
        "max_non_reward_drinks": max_non_reward_drinks,
        "max_reward_drinks": max_reward_drinks,
        "max_diamond_plays": max_diamond_plays,
        "max_bill_plays": max_bill_plays,
        "max_cake_plays": max_cake_plays,
        "max_mystery_plays": max_mystery_plays,
        "mean_rate": mean_rate,
        "std_dev_rate": std_dev_rate,
        "max_fixation_time": max_fixation_time
    }

def calculate_gb_normalization_variables(raw_data_folder: str):
    max_reaction_time = max_gaze_time = max_busted_ghosts = 0
    mean_reaction_time = std_dev_reaction_time = 0
    reaction_times = []

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                raw_data = json.load(file).get("Rows", [])

            for row in raw_data:
                key = row.get("Column5", "")  # Column5 holds the key names for GB
                value = row.get("Column6", 0)  # Column6 holds the corresponding values

                if "ReactionTime" in key:
                    max_reaction_time = max(max_reaction_time, float(value))
                    reaction_times.append(float(value))
                elif "TimeGazeOn" in key:
                    max_gaze_time = max(max_gaze_time, float(value))
                elif "Number" in key:
                    max_busted_ghosts = max(max_busted_ghosts, float(value))

    # Compute mean and standard deviation for reaction times
    mean_reaction_time = sum(reaction_times) / len(reaction_times) if reaction_times else 0
    variance_reaction = sum((x - mean_reaction_time) ** 2 for x in reaction_times) / len(reaction_times) if reaction_times else 0
    std_dev_reaction_time = math.sqrt(variance_reaction)

    return {
        "max_reaction_time": max_reaction_time,
        "max_gaze_time": max_gaze_time,
        "max_busted_ghosts": max_busted_ghosts,
        "mean_reaction_time": mean_reaction_time,
        "std_dev_reaction_time": std_dev_reaction_time
    }

def calculate_ls_normalization_variables(raw_data_folder: str):
    max_progress = max_time_spent = 0

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                raw_data = json.load(file).get("Rows", [])

            for row in raw_data:
                key = row.get("Column3", "")  # Column3 holds the key names for LS
                value = row.get("Column4", 0)  # Column4 holds the corresponding values

                if key.startswith("ProgressOfSavingWithin") or key.startswith("AvgOfProgressOf"):
                    max_progress = max(max_progress, float(value))
                elif key.startswith("TimeSpentOn"):
                    max_time_spent = max(max_time_spent, float(value))

    return {
        "max_progress": max_progress,
        "max_time_spent": max_time_spent
    }

def calculate_wg_normalization_variables(raw_data_folder: str):
    max_instruction_checks = max_safe_account_sum = max_non_reward_purchases = max_reward_purchases = 0

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                raw_data = json.load(file).get("Rows", [])

            for row in raw_data:
                key = row.get("Column9", "")  # Column9 holds the key names for WG
                value = row.get("Column10", 0)  # Column10 holds the corresponding values

                if "CheckedNumberOFTimesInstruction" in key:
                    max_instruction_checks = max(max_instruction_checks, float(value))
                elif "SafeAccountSumLevel" in key:
                    max_safe_account_sum = max(max_safe_account_sum, float(value))
                elif key == "NonRewardDrinksBoughtCount":
                    max_non_reward_purchases = max(max_non_reward_purchases, float(value))
                elif key == "RewardDrinksBoughtCount":
                    max_reward_purchases = max(max_reward_purchases, float(value))

    return {
        "max_instruction_checks": max_instruction_checks,
        "max_safe_account_sum": max_safe_account_sum,
        "max_non_reward_purchases": max_non_reward_purchases,
        "max_reward_purchases": max_reward_purchases
    }

def parse_float_or_string(value):
    if isinstance(value, str):
        value = value.strip().lower()
        if value == "yes":
            return 1.0  # Convert "yes" to 1
        try:
            return float(value)  # Convert valid floats
        except ValueError:
            return value  # Keep other strings as they are
    return float(value) if value not in ["", None] else ""

def process_and_normalize_files(raw_data_folder: str, normalized_data_folder: str):
    wg_variables = calculate_wg_normalization_variables(raw_data_folder)
    puw_variables=calculate_puw_normalization_variables(raw_data_folder)
    gb_variables=calculate_gb_normalization_variables( raw_data_folder)
    ls_variables=calculate_ls_normalization_variables( raw_data_folder)

    files = [os.path.join(raw_data_folder, f) for f in os.listdir(raw_data_folder) if f.endswith(".json")]

    if not os.path.exists(normalized_data_folder):
        os.makedirs(normalized_data_folder)

    for file_path in files:
        with open(file_path, 'r') as file:
            data = json.load(file)
            rows = data.get("Rows", [])

        normalized_rows = []
        for row in rows:
            puw_data = {row.get("Column1", ""): float(row.get("Column2", 0)) if row.get("Column2", "") not in ["", None] else ""}
            ls_data = {row.get("Column3", ""): parse_float_or_string(row.get("Column4", ""))}
            gb_data = {row.get("Column5", ""): float(row.get("Column6", 0)) if row.get("Column6", "") not in ["", None] else ""}
            tm_data = {row.get("Column7", ""): float(row.get("Column8", 0)) if row.get("Column8", "") not in ["", None] else ""}
            wg_data = {row.get("Column9", ""): float(row.get("Column10", 0)) if row.get("Column10", "") not in ["", None] else ""}
            
            normalized_puw = normalize_puw_input_data(
                puw_data,
                puw_variables["max_coins_quantity"],
                puw_variables["max_coins_bought"],
                puw_variables["max_non_reward_drinks"],
                puw_variables["max_reward_drinks"],
                puw_variables["max_diamond_plays"],
                puw_variables["max_bill_plays"],
                puw_variables["max_cake_plays"],
                puw_variables["max_mystery_plays"],
                puw_variables["mean_rate"],
                puw_variables["std_dev_rate"],
                puw_variables["max_fixation_time"],
            )
            normalized_ls = normalize_ls_input_data(ls_data, ls_variables["max_progress"], ls_variables["max_time_spent"])
            normalized_gb = normalize_gb_input_data(
                gb_data,
                gb_variables["max_reaction_time"],
                gb_variables["max_gaze_time"],
                gb_variables["max_busted_ghosts"],
                gb_variables["mean_reaction_time"],
                gb_variables["std_dev_reaction_time"]
                
            )
            normalized_tm = normalize_tm_input_data(tm_data)
            normalized_wg = normalize_wg_input_data(wg_data,wg_variables["max_instruction_checks"],wg_variables["max_safe_account_sum"],wg_variables["max_non_reward_purchases"],wg_variables["max_reward_purchases"])
            
            normalized_row = {
                "Column1": list(normalized_puw.keys())[0], "Column2": list(normalized_puw.values())[0],
                "Column3": list(normalized_ls.keys())[0], "Column4": list(normalized_ls.values())[0],
                "Column5": list(normalized_gb.keys())[0], "Column6": list(normalized_gb.values())[0],
                "Column7": list(normalized_tm.keys())[0], "Column8": list(normalized_tm.values())[0],
                "Column9": list(normalized_wg.keys())[0], "Column10": list(normalized_wg.values())[0]
            }
            normalized_rows.append(normalized_row)

        normalized_data = {"Rows": normalized_rows}

        normalized_file_path = os.path.join(normalized_data_folder, os.path.basename(file_path))
        with open(normalized_file_path, 'w') as normalized_file:
            json.dump(normalized_data, normalized_file, indent=4)

        print(f"Processed and saved: {normalized_file_path}")

def main():
    process_and_normalize_files("RawData", "NormalizedData")

if __name__ == "__main__":
    main()
