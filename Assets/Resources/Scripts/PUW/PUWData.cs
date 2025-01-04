using System;
using System.Collections.Generic;
using UnityEngine;

public class PUWData : MonoBehaviour
{
    public static PUWData Data { get; private set; }

    private void Awake()
    {
        if (Data == null)
        {
            Data = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void SaveData()
    {

        ParticipantSettings.Instance.PUWDataPair.Invoke( new KeyValuePair<string, float>("CoinsQuantity", (float) PUWStats.GetCoinsQuantity()));
        
        
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("CoinsBoughtCount", (float)PUWStats.GetCoinsBoughtCount()));

        // Mystery Slot Machine Plays per Minute
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("MysterySlotPlaysPerMinute",(float)PUWStats.GetMysterySlotPlaysPerMinute()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("DiamondSlotPlaysPerMinute",(float)PUWStats.GetDiamondSlotPlaysPerMinute()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("BillSlotPlaysPerMinute",(float)PUWStats.GetBillSlotPlaysPerMinute()));


        // Drinks
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("NonRewardDrinksBoughtCount",(float)PUWStats.GetNonRewardDrinksBoughtCount()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("RewardDrinksBoughtCount",(float)PUWStats.GetRewardDrinksBoughtCount()));


        // Fixation Times
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgFixationTimeAlcoholicDisplays",(float)PUWStats.GetAvgFixationTimeAlcoholicDisplays()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgFixationTimeAlcoholicVsNonAlcoholic",(float)PUWStats.GetAvgFixationTimeAlcoholicVsNonAlcoholic()));

        // Slot Machines Play Count
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("DiamondSlotPlayCount",(float)PUWStats.GetDiamondSlotPlayCount()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("BillSlotPlayCount",(float)PUWStats.GetBillSlotPlayCount()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("CakeSlotPlayCount",(float)PUWStats.GetCakeSlotPlayCount()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("MysterySlotPlayCount",(float)PUWStats.GetMysterySlotPlayCount()));

        // Time Tracking
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgTimePlayingDiamondMachine",(float)PUWStats.GetAvgTimePlayingDiamondMachine()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgTimePlayingBillMachine",(float)PUWStats.GetAvgTimePlayingBillMachine()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgTimePlayingMysteryMachine",(float)PUWStats.GetAvgTimePlayingMysteryMachine()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgTimePlayingCakeMachine",(float)PUWStats.GetAvgTimePlayingCakeMachine()));

        // Stagnant Time
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("AvgStagnantTime",(float)PUWStats.GetAvgStagnantTime()));
        ParticipantSettings.Instance.PUWDataPair.Invoke(new KeyValuePair<string, float>("TotalAccumulatedMoney",(float)PUWStats.GetTotalAccumulatedMoney()));
    
    }

    /*
    Further Steps in Training (Pseudo-code Explanation):

    1. During model training, include Batch Normalization layers to help stabilize and speed up convergence.
    2. Use Dropout layers between dense layers to prevent overfitting.
    3. Test the model iteratively:
       - Run training using the normalized dataset.
       - Evaluate the model's performance metrics (e.g., accuracy, loss).
       - Adjust hyperparameters or normalization scaling if needed.
    4. Continue experimenting with other composite metrics based on model performance and feedback.
    */


}
