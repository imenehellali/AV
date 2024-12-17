
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static ParticipantData;

public class ParticipantSettings : MonoBehaviour
{
    private string participantFilePath;

    private string ParticipantID;

    //Data from all the differetn levels to fetch
    private Dictionary<string, float> PUWdataDict = new Dictionary<string, float>();
    private Dictionary<string, float> LSdataDict = new Dictionary<string, float>();
    private Dictionary<string, float> TMdataDict = new Dictionary<string, float>();
    private Dictionary<string, float> GBdataDict = new Dictionary<string, float>();
    private Dictionary<string, float> WholeGameDict = new Dictionary<string, float>();
    private string strategy = "";
    public string getStrategy() => strategy;

    private List<Dictionary<string, float>> levelsMeasures = new List<Dictionary<string, float>>();


    //parsing varibales into a JSON file
    /// <summary>
    /// [CG0001; PUWString?0,PUWfloat?0, LSString?0, LSfloat?0, GBstring?0, GBfloat?0, TMString?0, TMfloat?0]
    /// </summary>
    /// <param name="UID"></param>
    /// 

    public UnityAction<KeyValuePair<string, float>> WholeGamePair;
    public UnityAction<KeyValuePair<string, float>> PUWDataPair;
    public UnityAction<KeyValuePair<string, float>> GBDataPair;
    public UnityAction<KeyValuePair<string, float>> TMDataPair;
    public UnityAction<KeyValuePair<string, float>> LSDataPair;
    public UnityAction<string> LSStrategy;
    public UnityAction<string> PID;

    private void OnEnable()
    {
        WholeGamePair += FillWholeGameDict;
        PUWDataPair += FillPUWDataDict;
        GBDataPair += FillGBDataDict;
        TMDataPair += FillTMDataDict;
        LSDataPair += FillLSDataDict;
        LSStrategy += FillStrategy;
        PID += UpdateParticipantData;
    }
    private void OnDisable()
    {
        WholeGamePair -= FillWholeGameDict;
        PUWDataPair -= FillPUWDataDict;
        GBDataPair -= FillGBDataDict;
        TMDataPair -= FillTMDataDict;
        LSDataPair -= FillLSDataDict;
        LSStrategy -= FillStrategy;
        PID -= UpdateParticipantData;
    }
    //Contains purchaseDurations, GameQAccount, sAfeAccount
    private void FillWholeGameDict(KeyValuePair<string, float> pair)
    {
        WholeGameDict.Add(pair.Key, pair.Value);
    }
    private void FillPUWDataDict(KeyValuePair<string, float> pair)
    {
        PUWdataDict.Add(pair.Key, pair.Value);
    }
    private void FillGBDataDict(KeyValuePair<string, float> pair)
    {
        GBdataDict.Add(pair.Key, pair.Value);
    }
    private void FillLSDataDict(KeyValuePair<string, float> pair)
    {
        LSdataDict.Add(pair.Key, pair.Value);
    }
    private void FillTMDataDict(KeyValuePair<string, float> pair)
    {
        TMdataDict.Add(pair.Key, pair.Value);
    }
    private void FillStrategy(string strategy) => this.strategy = strategy;
    private void UpdateParticipantData(string UID)
    {
        ParticipantID = UID;
    }
    public static ParticipantSettings Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int getLongestDict()
    {
        int lengthOfColumn = PUWdataDict.Count;
        int Dict = 0;

        if (LSdataDict.Count > lengthOfColumn)
        {
            lengthOfColumn = LSdataDict.Count;
            Dict = 1;
        }
        if (TMdataDict.Count > lengthOfColumn)
        {
            lengthOfColumn = TMdataDict.Count;
            Dict = 2;
        }
        if (GBdataDict.Count > lengthOfColumn)
        {
            lengthOfColumn = GBdataDict.Count;
            Dict = 3;
        }
        if (WholeGameDict.Count > lengthOfColumn)
        {
            lengthOfColumn = WholeGameDict.Count;
            Dict = 4;
        }


        return Dict;
    }
    private ParticipantData GeneratePdata()
    {

        int Dict = getLongestDict();
        ParticipantData _pData = new ParticipantData();

        switch (Dict)
        {
            case 0:

                {
                    int idx = 0;
                    foreach (KeyValuePair<string, float> keyValuePair in PUWdataDict)
                    {
                        if (idx == 0)
                        {
                            ParticipantRow _pRow = new ParticipantRow(keyValuePair.Key,
                            keyValuePair.Value, "Strategy", strategy,
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                            );
                            _pData.Rows.Add(_pRow);
                        }
                        else
                        {
                            ParticipantRow _pRow = new ParticipantRow(keyValuePair.Key,
                                keyValuePair.Value,
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Key : "",
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Value : 0f,
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                                );
                            _pData.Rows.Add(_pRow);
                        }

                        ++idx;
                    }
                    break;
                }
            case 1:
                {
                    int idx = 0;
                    foreach (KeyValuePair<string, float> keyValuePair in LSdataDict)
                    {
                        if (idx == 0)
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                            "Strategy", strategy,
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                            );
                            _pData.Rows.Add(_pRow);
                        }
                        else
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                                keyValuePair.Key,
                                keyValuePair.Value,
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                                );
                            _pData.Rows.Add(_pRow);
                        }

                        ++idx;
                    }
                    break;
                }
            case 2:

                {
                    int idx = 0;
                    foreach (KeyValuePair<string, float> keyValuePair in GBdataDict)
                    {
                        if (idx == 0)
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                            "Strategy", strategy,
                             keyValuePair.Key,
                             keyValuePair.Value,
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                            );
                            _pData.Rows.Add(_pRow);
                        }
                        else
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Key : "",
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Value : 0f,
                                keyValuePair.Key,
                                keyValuePair.Value,
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                                );
                            _pData.Rows.Add(_pRow);
                        }

                        ++idx;
                    }
                    break;
                }
            case 3:
                {
                    int idx = 0;
                    foreach (KeyValuePair<string, float> keyValuePair in TMdataDict)
                    {
                        if (idx == 0)
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                            "Strategy", strategy,
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                             keyValuePair.Key,
                             keyValuePair.Value,
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                            !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                            );
                            _pData.Rows.Add(_pRow);
                        }
                        else
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Key : "",
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Value : 0f,
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                                keyValuePair.Key,
                                keyValuePair.Value,
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Key : "",
                                !WholeGameDict.ElementAt(idx).IsUnityNull() ? WholeGameDict.ElementAt(idx).Value : 0f
                                );
                            _pData.Rows.Add(_pRow);
                        }

                        ++idx;
                    }
                    break;
                }
            case 4:
                {
                    int idx = 0;
                    foreach (KeyValuePair<string, float> keyValuePair in WholeGameDict)
                    {
                        if (idx == 0)
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                            !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                            "Strategy", strategy,
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                            !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                            !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f,
                             keyValuePair.Key,
                             keyValuePair.Value
                            );
                            _pData.Rows.Add(_pRow);
                        }
                        else
                        {
                            ParticipantRow _pRow = new ParticipantRow(
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Key : "",
                                !PUWdataDict.ElementAt(idx).IsUnityNull() ? PUWdataDict.ElementAt(idx).Value : 0f,
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Key : "",
                                !LSdataDict.ElementAt(idx).IsUnityNull() ? LSdataDict.ElementAt(idx).Value : 0f,
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Key : "",
                                !GBdataDict.ElementAt(idx).IsUnityNull() ? GBdataDict.ElementAt(idx).Value : 0f,
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Key : "",
                                !TMdataDict.ElementAt(idx).IsUnityNull() ? TMdataDict.ElementAt(idx).Value : 0f, 
                                keyValuePair.Key,
                                keyValuePair.Value );
                            _pData.Rows.Add(_pRow);
                        }

                        ++idx;
                    }
                    break;
                }

        }
        return _pData;
    }

    private void SaveRawParticipantData()
    {
        participantFilePath = Path.Combine(Application.persistentDataPath, $"{PID}.json");
        ParticipantData _pData = GeneratePdata();
        try
        {
            // Convert the data object to a JSON string
            string jsonData =JsonUtility.ToJson(_pData,true);

            // Write the JSON string to the specified file path
            File.WriteAllText(participantFilePath, jsonData);

            Debug.Log("Settings data saved to: " +participantFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save settings data: " + e.Message);
        }

    }
    private void SaveNormalizedParticipantData()
    {
        participantFilePath = Path.Combine(Application.persistentDataPath, $"{PID}Normalized.json");

    }

    //PUW Data generation
    public async Task<Dictionary<string, float>> GeneratePUWInputData()
    {
        return await Task.Run(()=> NormalizePUWInputData(PUWdataDict));
    }

    public static Dictionary<string, float> NormalizePUWInputData(Dictionary<string, float> inputData)
    {
        Dictionary<string, float> normalizedData = new Dictionary<string, float>();

        // Min and Max values for Min-Max Normalization (assumed or predefined based on knowledge of data distribution)
        float maxCoins = 500; // Example assumed upper bound
        float maxPlaysPerMinute = 10; // Example assumed upper bound for slot plays per minute
        float maxFixationTime = 20; // Example assumed upper bound for fixation times
        float maxTimePlaying = 15; // Example assumed upper bound for playing time
        float maxAccumulatedMoney = 1000; // Example assumed upper bound for total accumulated money

        // Mean and Standard Deviation for Z-score Normalization (assumed values)
        float meanRate = 5.0f; // Example assumed mean
        float stdDevRate = 2.0f; // Example assumed standard deviation

        foreach (var entry in inputData)
        {
            switch (entry.Key)
            {
                // Min-Max Normalization for counts and quantities
                case "CoinsQuantity":
                case "CoinsBoughtCount":
                case "NonRewardDrinksBoughtCount":
                case "RewardDrinksBoughtCount":
                case "GetDiamondSlotPlayCount":
                case "BillSlotPlayCount":
                case "CakeSlotPlayCount":
                case "MysterySlotPlayCount":
                    normalizedData[entry.Key] = entry.Value / maxCoins;
                    break;

                // Z-score Normalization for rates (plays per minute)
                case "MysterySlotPlaysPerMinute":
                case "DiamondSlotPlaysPerMinute":
                case "BillSlotPlaysPerMinute":
                    normalizedData[entry.Key] = (entry.Value - meanRate) / stdDevRate;
                    break;

                // Min-Max Normalization for average times
                case "AvgFixationTimeAlcoholicDisplays":
                case "AvgFixationTimeAlcoholicVsNonAlcoholic":
                case "AvgTimePlayingDiamondMachine":
                case "AvgTimePlayingBillMachine":
                case "AvgTimePlayingMysteryMachine":
                case "AvgTimePlayingCakeMachine":
                case "AvgStagnantTime":
                    normalizedData[entry.Key] = entry.Value / maxFixationTime;
                    break;

                // Log Transformation for total accumulated money to reduce skewness
                case "TotalAccumulatedMoney":
                    normalizedData[entry.Key] = (float)Math.Log(entry.Value + 1) / (float)Math.Log(maxAccumulatedMoney + 1); // Log transformation and Min-Max scaling
                    break;

                default:
                    normalizedData[entry.Key] = entry.Value; // If no specific rule, keep value as is
                    break;
            }
        }

        // Composite Feature: TotalSlotPlays
        float totalSlotPlays = inputData.ContainsKey("GetDiamondSlotPlayCount") ? inputData["GetDiamondSlotPlayCount"] : 0;
        totalSlotPlays += inputData.ContainsKey("BillSlotPlayCount") ? inputData["BillSlotPlayCount"] : 0;
        totalSlotPlays += inputData.ContainsKey("CakeSlotPlayCount") ? inputData["CakeSlotPlayCount"] : 0;
        totalSlotPlays += inputData.ContainsKey("MysterySlotPlayCount") ? inputData["MysterySlotPlayCount"] : 0;
        normalizedData["TotalSlotPlays"] = totalSlotPlays / (4 * maxCoins); // Normalizing the composite feature

        foreach (var item in normalizedData)
        {
            Debug.Log($"{item.Key}: {item.Value}\n");
        }

        return normalizedData;
    }

    //LS Data generation

    public async Task<Dictionary<string, float>> GenerateLSInputData()
    {
        return await Task.Run(() => NormalizeLSInputData(LSdataDict));
    }

    public static Dictionary<string, float> NormalizeLSInputData(Dictionary<string, float> inputData)
    {
        Dictionary<string, float> normalizedData = new Dictionary<string, float>();

        // Min and Max values for Min-Max Normalization (assumed or predefined based on knowledge of data distribution)
        float maxProgress = 100; // Example assumed upper bound for progress percentages
        float maxTimeSpent = 1000; // Example assumed upper bound for time spent in seconds

        foreach (var entry in inputData)
        {
            switch (entry.Key)
            {
                // Min-Max Normalization for progress percentages (e.g., percentageDone)
                case "ProgressOfSavingWithinCH4":
                case "ProgressOfSavingWithinCH3":
                case "ProgressOfSavingWithinCH2":
                case "ProgressOfSavingWithinCH1":
                case "ProgressOfSavingWithinCA4":
                case "ProgressOfSavingWithinCA3":
                case "ProgressOfSavingWithinCA2":
                case "ProgressOfSavingWithinCA1":
                case "ProgressOfSavingWithinAStrategy":
                case "AvgOfProgressOfHumanCases":
                case "AvgOfProgressOfAnimalCases":
                    normalizedData[entry.Key] = entry.Value / maxProgress;
                    break;

                // Min-Max Normalization for time spent (e.g., SpenTimeOnCase)
                case "TimeSpentOnCH4":
                case "TimeSpentOnCH3":
                case "TimeSpentOnCH2":
                case "TimeSpentOnCH1":
                case "TimeSpentOnCA4":
                case "TimeSpentOnCA3":
                case "TimeSpentOnCA2":
                case "TimeSpentOnCA1":
                    normalizedData[entry.Key] = entry.Value / maxTimeSpent;
                    break;

                // Binary values (e.g., healed, dead, watched video)
                case "CH4DiscoveredAndSaved?":
                case "CH4DiscoveredAndKilled?":
                case "CH4WatchedVideo":
                case "CH3DiscoveredAndSaved?":
                case "CH3DiscoveredAndKilled?":
                case "CH32WatchedVideo":
                case "CH2DiscoveredAndSaved?":
                case "CH2DiscoveredAndKilled?":
                case "CH2WatchedVideo":
                case "CH1DiscoveredAndSaved?":
                case "CH1DiscoveredAndKilled?":
                case "CH1WatchedVideo":
                case "CA4DiscoveredAndSaved?":
                case "CA4DiscoveredAndKilled?":
                case "CA4WatchedVideo":
                case "CA3DiscoveredAndSaved?":
                case "CA3DiscoveredAndKilled?":
                case "CA3WatchedVideo":
                case "CA2DiscoveredAndSaved?":
                case "CA2DiscoveredAndKilled?":
                case "CA2WatchedVideo":
                case "CA1DiscoveredAndSaved?":
                case "CA1DiscoveredAndKilled?":
                case "CA1WatchedVideo":
                    normalizedData[entry.Key] = entry.Value; // No normalization needed for binary values
                    break;

                default:
                    normalizedData[entry.Key] = entry.Value; // If no specific rule, keep value as is
                    break;
            }
        }

        return normalizedData;

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


    //GB Data generation

    public async Task<Dictionary<string, float>> GenerateGBInputData()
    {
        return await Task.Run(() => NormalizeGBInputData(GBdataDict));
    }
    public static Dictionary<string, float> NormalizeGBInputData(Dictionary<string, float> inputData)
    {
        Dictionary<string, float> normalizedData = new Dictionary<string, float>();

        // Min and Max values for Min-Max Normalization (assumed or predefined based on knowledge of data distribution)
        float maxReactionTime = 10.0f; // Assumed upper bound for reaction times (seconds)
        float maxGazeTime = 10.0f; // Assumed upper bound for gaze time (seconds)
        float maxBustedGhosts = 10.0f; // Assumed upper bound for number of ghosts busted

        // Mean and Standard Deviation for Z-score Normalization (assumed values)
        float meanReactionTime = 5.0f; // Assumed mean for reaction times
        float stdDevReactionTime = 2.0f; // Assumed standard deviation for reaction times

        foreach (var entry in inputData)
        {
            switch (entry.Key)
            {
                // Min-Max Normalization for reaction times
                case "q1AvgReactionTimeToBustAnyGhost":
                case "q1AvgReactionTimeToBustCorrectGhost":
                case "q1AvgReactionTimeToBustWrongGhost":
                case "q2AvgReactionTimeToBustAnyGhost":
                case "q2AvgReactionTimeToBustCorrectGhost":
                case "q2AvgReactionTimeToBustWrongGhost":
                case "q12AvgReactionTimeToBustAnyGhost":
                case "q12AvgReactionTimeToBustCorrectGhost":
                case "q12AvgReactionTimeToBustWrongGhost":
                    normalizedData[entry.Key] = entry.Value / maxReactionTime;
                    break;

                // Min-Max Normalization for gaze times
                case "q1AvgTimeGazeOnCorrectGhost":
                case "q1AvgTimeGazeOnWrongGhost":
                case "q2AvgTimeGazeOnCorrectGhost":
                case "q2AvgTimeGazeOnWrongGhost":
                case "q12AvgTimeGazeOnCorrectGhost":
                case "q12AvgTimeGazeOnWrongGhost":
                case "q3AvgTimeGazeOnCorrectGhost":
                case "q3AvgTimeGazeOnWrongGhost":
                case "q4AvgTimeGazeOnCorrectGhost":
                case "q4AvgTimeGazeOnWrongGhost":
                    normalizedData[entry.Key] = entry.Value / maxGazeTime;
                    break;

                // Min-Max Normalization for number of ghosts busted
                case "q1AvgNumberCorrectBustedGhosts":
                case "q1AvgNumberWrongBusterGhosts":
                case "q2AvgNumberCorrectBustedGhosts":
                case "q2AvgNumberWrongBusterGhosts":
                    normalizedData[entry.Key] = entry.Value / maxBustedGhosts;
                    break;

                // Z-score Normalization for increase/decrease margins (assuming normally distributed data)
                case "incDecMarginReactionTimeAnyQ3To21":
                case "incDecMarginReactionTimeCorrectQ3To21":
                case "incDecMarginReactionTimeWrongQ3To21":
                case "incDecMarginReactionTimeAnyQ4To3":
                case "incDecMarginReactionTimeCorrectQ4To3":
                case "incDecMarginReactionTimeWrongQ4To3":
                    normalizedData[entry.Key] = (entry.Value - meanReactionTime) / stdDevReactionTime;
                    break;

                // Min-Max Normalization for increase/decrease margins for bust counts
                case "incDecMarginNumberBustAnyQ3To21":
                case "incDecMarginNumberBustCorrectQ3To21":
                case "incDecMarginNumberBustWrongQ3To21":
                case "incDecMarginNumberBustAnyQ4To3":
                case "incDecMarginNumberBustCorrectQ4To3":
                case "incDecMarginNumberBustWrongQ4To3":
                    normalizedData[entry.Key] = entry.Value / maxBustedGhosts;
                    break;

                default:
                    normalizedData[entry.Key] = entry.Value; // If no specific rule, keep value as is
                    break;
            }
        }

        foreach (var item in normalizedData)
        {
            Debug.Log($"{item.Key}: {item.Value}\n");
        }

        return normalizedData;
    }



    //TM data generation

    public async Task<Dictionary<string, float>> GenerateTMInputData()
    {
        return await Task.Run(() => NormalizePUWInputData(TMdataDict));
    }





    public static float CalculateStandardDeviation(IEnumerable<float> values)
    {
        float mean = values.Average();
        float sumOfSquaresOfDifferences = values.Select(val => (val - mean) * (val - mean)).Sum();
        float stdDev = (float)Math.Sqrt(sumOfSquaresOfDifferences / values.Count());
        return stdDev;
    }

}