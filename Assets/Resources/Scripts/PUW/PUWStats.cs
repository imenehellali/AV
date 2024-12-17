using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PUWStats 
{
    private static int _coinsQuantity = 0;
    private static int _coinsBoughtCount = 0;
    private static int _nonRewardDrinksBoughtCount = 0;
    private static int _rewardDrinksBoughtCount = 0;
 
    private static int _mysterySlotPlaysPerMinute = 0;
    private static int _billSlotPlaysPerMinute = 0;
    private static int _diamondSlotPlaysPerMinute = 0;

    // Fixation Times
    private static float _totalFixationTimeAlcoholicDisplays = 0f;
    private static float _totalFixationTimeNonAlcoholicDisplays = 0f;
    private static float _overallTaskTime = 0f;

    private static int _diamondSlotPlayCount = 0;
    private static int _billSlotPlayCount = 0;
    private static int _cakeSlotPlayCount = 0;
    private static int _mysterySlotPlayCount = 0;

    private const float diamondSlotDuration = 7.0f;
    private const float billSlotDuration = 5.5f;
    private const float cakeSlotDuration = 5.6f;
    private static float mysterySlotsTotalDurations = 0;


    private static float _totalAccumulatedMoney = 0f;
   
    //Coins
    public static void AddCoins(int amount)
    {
        _coinsQuantity += amount;
        _coinsBoughtCount++;
    }

    public static int GetCoinsQuantity()
    {
        return _coinsQuantity;
    }

    public static int GetCoinsBoughtCount()
    {
        return _coinsBoughtCount;
    }

    // Mystery Slot Machine Plays per Minute
    public static void IncrementMysterySlotPlaysPerMinute()
    {
        _mysterySlotPlaysPerMinute++;
    }

    public static int GetMysterySlotPlaysPerMinute()
    {
        return _mysterySlotPlaysPerMinute;
    }



    public static void IncrementDiamondSlotPlaysPerMinute()
    {
        _diamondSlotPlaysPerMinute++;
    }

    public static int GetDiamondSlotPlaysPerMinute()
    {
        return _diamondSlotPlaysPerMinute;
    }
    public static void IncrementBillSlotPlaysPerMinute()
    {
        _billSlotPlaysPerMinute++;
    }

    public static int GetBillSlotPlaysPerMinute()
    {
        return _billSlotPlaysPerMinute;
    }



    // Drinks
    public static void UpdateNonRewardDrinksBoughtCount(int amount)
    {
        _nonRewardDrinksBoughtCount+=amount;
    }

    public static void UpdateRewardDrinksBoughtCount(int amount)
    {
        _rewardDrinksBoughtCount+=amount;
    }

    public static int GetNonRewardDrinksBoughtCount()
    {
        return _nonRewardDrinksBoughtCount;
    }

    public static int GetRewardDrinksBoughtCount()
    {
        return _rewardDrinksBoughtCount;
    }

    // Fixation Times
    public static void AddFixationTimeAlcoholicDisplays(float time)
    {
        _totalFixationTimeAlcoholicDisplays += time;
    }

    public static void AddFixationTimeNonAlcoholicDisplays(float time)
    {
        _totalFixationTimeNonAlcoholicDisplays += time;
    }

    public static void AddOVerallTaskTime(float time)
    {
        _overallTaskTime = time;
    }
    public static float GetAvgFixationTimeAlcoholicDisplays()
    {
        return _totalFixationTimeAlcoholicDisplays / _overallTaskTime;
    }

    public static float GetAvgFixationTimeAlcoholicVsNonAlcoholic()
    {
        return _totalFixationTimeAlcoholicDisplays / _totalFixationTimeNonAlcoholicDisplays;
    }


    // Slot Machines Play Count
    public static void IncrementDiamondSlotPlayCount()
    {
        _diamondSlotPlayCount++;
    }

    public static void IncrementBillSlotPlayCount()
    {
        _billSlotPlayCount++;
    }

    public static void IncrementCakeSlotPlayCount()
    {
        _cakeSlotPlayCount++;
    }

    public static void IncrementMysterySlotPlayCount()
    {
        _mysterySlotPlayCount++;
        IncrementMysterySlotPlaysPerMinute();
    }

    public static int GetDiamondSlotPlayCount()
    {
        return _diamondSlotPlayCount;
    }

    public static int GetBillSlotPlayCount()
    {
        return _billSlotPlayCount;
    }

    public static int GetCakeSlotPlayCount()
    {
        return _cakeSlotPlayCount;
    }

    public static int GetMysterySlotPlayCount()
    {
        return _mysterySlotPlayCount;
    }

    // Time Tracking
    public static float GetAvgTimePlayingDiamondMachine()
    {
        return (diamondSlotDuration * _diamondSlotPlayCount) / GetTotalTimePlayingMachines();
    }

    public static float GetAvgTimePlayingBillMachine()
    {
        return (billSlotDuration * _billSlotPlayCount) / GetTotalTimePlayingMachines();
    }

    public static void UpdateMysterySlotsTotalDurations(float amount)
    {
        mysterySlotsTotalDurations += amount;
    }
    public static float GetAvgTimePlayingMysteryMachine()
    {
        return mysterySlotsTotalDurations / GetTotalTimePlayingMachines();
    }

    public static float GetAvgTimePlayingCakeMachine()
    {
        return (cakeSlotDuration * _cakeSlotPlayCount) / GetTotalTimePlayingMachines();
    }

    private static float GetTotalTimePlayingMachines()
    {
        return (_diamondSlotPlayCount * diamondSlotDuration)
               + (_billSlotPlayCount * billSlotDuration)
               + (mysterySlotsTotalDurations)
               + (_cakeSlotPlayCount * cakeSlotDuration);
    }

    // Stagnant Time
    public static float GetAvgStagnantTime()
    {
        float stagnantTime = _overallTaskTime - GetTotalTimePlayingMachines();
        return (stagnantTime / _overallTaskTime);
    }

    // Accumulated Money
    public static void AddAccumulatedMoney(float amount)
    {
        _totalAccumulatedMoney += amount;
    }

    public static float GetTotalAccumulatedMoney()
    {
        return _totalAccumulatedMoney;
    }

    

    public static void SaveStatsToParticipantData()
    {
        PUWData.Data.SaveData();  // Ensure this saves the updated stats
    }
}
