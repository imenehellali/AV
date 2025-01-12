
using System.Collections.Generic;
using System.Linq;
public static class GameStats
{
    //PUS
    private static int nonRewardDrinksBoughtCount = 0;
    private static int rewardDrinksBoughtCount = 0;
    private static float avgPurchaseDuration = 0f;

    //Money Manager
    private static Dictionary<int,float> safeAccount = new Dictionary<int, float>();


    // PUS Drinks Counts Functions
    public static void UpdateNonRewardDrinksBoughtCount(int amount) => nonRewardDrinksBoughtCount += amount;
    public static void UpdateRewardDrinksBoughtCount(int amount) => rewardDrinksBoughtCount += amount;
    public static int GetNonRewardDrinksBoughtCount() => nonRewardDrinksBoughtCount;
    public static int GetRewardDrinksBoughtCount() => rewardDrinksBoughtCount;
    public static void ResetDrinksBoughtCount()
    {
        nonRewardDrinksBoughtCount = 0;
        rewardDrinksBoughtCount = 0;
    }
    public static float GetAveragePurchaseDuration()
    {
        GameSettings.Instance.GetPurchaseDurations().ForEach(duration => { avgPurchaseDuration += duration; });
        avgPurchaseDuration /= GameSettings.Instance.GetPurchaseDurations().Count * GameSettings.Instance.BetweenSceneDuration;
        return avgPurchaseDuration;
    }

    //Money Manager Functions
    public static void UpdateSafeAccount(int idx, float amount)
    {
       //Resetting if this is a new entry avoid doing extra function for that
           
        if (idx >= 0 && idx < GameSettings.Instance.LevelSequence.Length && idx <= safeAccount.Count)
        {
            bool suc=safeAccount.TryAdd(idx, amount);
            if (!suc)
            {
                safeAccount[idx] = amount;
            }
        }
        else
        {
                safeAccount.Clear();
        }
    }
    public static void SaveSafeAcount()
    {
        for (int i = 0; i < GameSettings.Instance.LevelSequence.Length; i++)
            ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>($"SafeAccountSumLevel{GameSettings.Instance.LevelSequence[i]}", safeAccount[i]));
    }
    public static List<float> GetSafeAccount() => safeAccount.Values.ToList();
    public static void SaveGameAccount(float amount) => ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>("GameAccountFinalSum", amount));

    //Instruction Functions
    public static void SaveCheckedInstrCount(List<int> checkedInstrCount)
    {

        for (int i = 0; i < GameSettings.Instance.LevelSequence.Length; i++)
            ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>($"CheckedNumberOFTimesInstructionOfLevel{GameSettings.Instance.LevelSequence[i]}", checkedInstrCount[i]));

    }
}

