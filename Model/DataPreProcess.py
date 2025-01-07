import os
import json
import math
from typing import Dict, Any

def normalize_puw_input_data(input_data: Dict[str, float], max_coins: float, max_plays_per_minute: float, max_fixation_time: float, max_accumulated_money: float, mean_rate: float, std_dev_rate: float) -> Dict[str, float]:
    normalized_data = {}

    for key, value in input_data.items():
        if key in [
            "CoinsQuantity", "CoinsBoughtCount", "NonRewardDrinksBoughtCount",
            "RewardDrinksBoughtCount", "GetDiamondSlotPlayCount", "BillSlotPlayCount",
            "CakeSlotPlayCount", "MysterySlotPlayCount"
        ]:
            normalized_data[key] = value / max_coins
        elif key in [
            "MysterySlotPlaysPerMinute", "DiamondSlotPlaysPerMinute", "BillSlotPlaysPerMinute"
        ]:
            normalized_data[key] = (value - mean_rate) / std_dev_rate
        elif key in [
            "AvgFixationTimeAlcoholicDisplays", "AvgFixationTimeAlcoholicVsNonAlcoholic",
            "AvgTimePlayingDiamondMachine", "AvgTimePlayingBillMachine",
            "AvgTimePlayingMysteryMachine", "AvgTimePlayingCakeMachine",
            "AvgStagnantTime"
        ]:
            normalized_data[key] = value / max_fixation_time
        elif key == "TotalAccumulatedMoney":
            normalized_data[key] = math.log(value + 1) / math.log(max_accumulated_money + 1)
        else:
            normalized_data[key] = value

    total_slot_plays = sum(input_data.get(slot_key, 0) for slot_key in [
        "GetDiamondSlotPlayCount", "BillSlotPlayCount", "CakeSlotPlayCount", "MysterySlotPlayCount"
    ])
    normalized_data["TotalSlotPlays"] = total_slot_plays / (4 * max_coins)

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

def normalize_gb_input_data(input_data: Dict[str, float], max_reaction_time: float, max_gaze_time: float, max_busted_ghosts: float, mean_reaction_time: float, std_dev_reaction_time: float) -> Dict[str, float]:
    normalized_data = {}

    for key, value in input_data.items():
        if "ReactionTime" in key:
            normalized_data[key] = value / max_reaction_time
        elif "TimeGazeOn" in key:
            normalized_data[key] = value / max_gaze_time
        elif "Number" in key:
            normalized_data[key] = value / max_busted_ghosts
        elif "incDecMargin" in key:
            normalized_data[key] = (value - mean_reaction_time) / std_dev_reaction_time
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

def calculate_normalization_variables(raw_data_folder: str):
    max_coins = max_plays_per_minute = max_fixation_time = max_accumulated_money = 0
    mean_rate = std_dev_rate = 0
    max_progress = max_time_spent = 0
    max_reaction_time = max_gaze_time = max_busted_ghosts = 0
    reaction_times = []
    play_rates = []

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                raw_data = json.load(file)

            for key, value in raw_data.items():
                if key in [
                    "CoinsQuantity", "CoinsBoughtCount", "NonRewardDrinksBoughtCount",
                    "RewardDrinksBoughtCount", "GetDiamondSlotPlayCount", "BillSlotPlayCount",
                    "CakeSlotPlayCount", "MysterySlotPlayCount"
                ]:
                    max_coins = max(max_coins, value)
                elif key in [
                    "MysterySlotPlaysPerMinute", "DiamondSlotPlaysPerMinute", "BillSlotPlaysPerMinute"
                ]:
                    play_rates.append(value)
                elif key in [
                    "AvgFixationTimeAlcoholicDisplays", "AvgFixationTimeAlcoholicVsNonAlcoholic",
                    "AvgTimePlayingDiamondMachine", "AvgTimePlayingBillMachine",
                    "AvgTimePlayingMysteryMachine", "AvgTimePlayingCakeMachine",
                    "AvgStagnantTime"
                ]:
                    max_fixation_time = max(max_fixation_time, value)
                elif key == "TotalAccumulatedMoney":
                    max_accumulated_money = max(max_accumulated_money, value)
                elif key.startswith("ProgressOfSavingWithin") or key.startswith("AvgOfProgressOf"):
                    max_progress = max(max_progress, value)
                elif key.startswith("TimeSpentOn"):
                    max_time_spent = max(max_time_spent, value)
                elif "ReactionTime" in key:
                    max_reaction_time = max(max_reaction_time, value)
                    reaction_times.append(value)
                elif "TimeGazeOn" in key:
                    max_gaze_time = max(max_gaze_time, value)
                elif "Number" in key:
                    max_busted_ghosts = max(max_busted_ghosts, value)

    mean_rate = sum(play_rates) / len(play_rates) if play_rates else 0
    variance = sum((x - mean_rate) ** 2 for x in play_rates) / len(play_rates) if play_rates else 0
    std_dev_rate = math.sqrt(variance)

    mean_reaction_time = sum(reaction_times) / len(reaction_times) if reaction_times else 0
    variance_reaction = sum((x - mean_reaction_time) ** 2 for x in reaction_times) / len(reaction_times) if reaction_times else 0
    std_dev_reaction_time = math.sqrt(variance_reaction)

    return {
        "max_coins": max_coins,
        "max_plays_per_minute": max_plays_per_minute,
        "max_fixation_time": max_fixation_time,
        "max_accumulated_money": max_accumulated_money,
        "mean_rate": mean_rate,
        "std_dev_rate": std_dev_rate,
        "max_progress": max_progress,
        "max_time_spent": max_time_spent,
        "max_reaction_time": max_reaction_time,
        "max_gaze_time": max_gaze_time,
        "max_busted_ghosts": max_busted_ghosts,
        "mean_reaction_time": mean_reaction_time,
        "std_dev_reaction_time": std_dev_reaction_time
    }

def process_and_normalize_files(raw_data_folder: str, normalized_data_folder: str):
    variables = calculate_normalization_variables(raw_data_folder)

    if not os.path.exists(normalized_data_folder):
        os.makedirs(normalized_data_folder)

    for filename in os.listdir(raw_data_folder):
        if filename.startswith("AD") or filename.startswith("CG"):
            file_path = os.path.join(raw_data_folder, filename)

            with open(file_path, 'r') as file:
                rows = json.load(file)

            puw_data, ls_data, gb_data, tm_data, wg_data = {}, {}, {}, {}, {}

            for row in rows:
                puw_data[row[0]] = float(row[1]) if row[1] else 0
                ls_data[row[2]] = float(row[3]) if row[3] else 0
                gb_data[row[4]] = float(row[5]) if row[5] else 0
                tm_data[row[6]] = float(row[7]) if row[7] else 0
                wg_data[row[8]] = float(row[9]) if row[9] else 0

            normalized_puw = normalize_puw_input_data(
                puw_data,
                variables["max_coins"],
                variables["max_plays_per_minute"],
                variables["max_fixation_time"],
                variables["max_accumulated_money"],
                variables["mean_rate"],
                variables["std_dev_rate"]
            )
            normalized_ls = normalize_ls_input_data(ls_data, variables["max_progress"], variables["max_time_spent"])
            normalized_gb = normalize_gb_input_data(
                gb_data,
                variables["max_reaction_time"],
                variables["max_gaze_time"],
                variables["max_busted_ghosts"],
                variables["mean_reaction_time"],
                variables["std_dev_reaction_time"]
            )
            normalized_tm = normalize_tm_input_data(tm_data)

            normalized_data = {
                **normalized_puw,
                **normalized_ls,
                **normalized_gb,
                **normalized_tm,
                **wg_data
            }

            normalized_file_path = os.path.join(normalized_data_folder, filename)
            with open(normalized_file_path, 'w') as normalized_file:
                json.dump(normalized_data, normalized_file, indent=4)

            print(f"Processed and saved: {normalized_file_path}")

def main():
    process_and_normalize_files("RawData", "NormalizedData")

if __name__ == "__main__":
    main()
