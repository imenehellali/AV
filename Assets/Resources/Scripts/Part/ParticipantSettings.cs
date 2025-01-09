
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ParticipantSettings : MonoBehaviour
{
    private string ParticipantID;

    //Data from all the differetn levels to fetch
    private Dictionary<string, float> PUWdataDict = new Dictionary<string, float>();
    private Dictionary<string, object> LSdataDict = new Dictionary<string, object>();
    private Dictionary<string, object> TMdataDict = new Dictionary<string, object>();
    private Dictionary<string, float> GBdataDict = new Dictionary<string, float>();
    private Dictionary<string, float> WholeGameDict = new Dictionary<string, float>();



    //parsing varibales into a JSON file
    /// <summary>
    /// [CG0001; PUWString?0,PUWfloat?0, LSString?0, LSfloat?0, GBstring?0, GBfloat?0, TMString?0, TMfloat?0]
    /// </summary>
    /// <param name="UID"></param>
    /// 

    public UnityAction<KeyValuePair<string, float>> WholeGamePair;
    public UnityAction<KeyValuePair<string, float>> PUWDataPair;
    public UnityAction<KeyValuePair<string, float>> GBDataPair;
    public UnityAction<KeyValuePair<string, object>> TMDataPair;
    public UnityAction<KeyValuePair<string, object>> LSDataPair;
    public UnityAction<string> PID;

    private void OnEnable()
    {
        WholeGamePair += FillWholeGameDict;
        PUWDataPair += FillPUWDataDict;
        GBDataPair += FillGBDataDict;
        TMDataPair += FillTMDataDict;
        LSDataPair += FillLSDataDict;
        PID += UpdateParticipantData;
    }
    private void OnDisable()
    {
        WholeGamePair -= FillWholeGameDict;
        PUWDataPair -= FillPUWDataDict;
        GBDataPair -= FillGBDataDict;
        TMDataPair -= FillTMDataDict;
        LSDataPair -= FillLSDataDict;
        PID -= UpdateParticipantData;
    }
    //Contains purchaseDurations, GameQAccount, sAfeAccount
    private void FillWholeGameDict(KeyValuePair<string, float> pair)
    {
        if (!WholeGameDict.ContainsKey(pair.Key))
            WholeGameDict.Add(pair.Key, pair.Value);
        else
            WholeGameDict[pair.Key] = pair.Value;
    }
    private void FillPUWDataDict(KeyValuePair<string, float> pair)
    {
        if (!PUWdataDict.ContainsKey(pair.Key))
            PUWdataDict.Add(pair.Key, pair.Value);
        else
            PUWdataDict[pair.Key] = pair.Value;
    }
    private void FillGBDataDict(KeyValuePair<string, float> pair)
    {
        if (!GBdataDict.ContainsKey(pair.Key))
            GBdataDict.Add(pair.Key, pair.Value);
        else
            GBdataDict[pair.Key] = pair.Value;
    }
    private void FillLSDataDict(KeyValuePair<string, object> pair)
    {
        if (!LSdataDict.ContainsKey(pair.Key))
            LSdataDict.Add(pair.Key, pair.Value);
        else
            LSdataDict[pair.Key] = pair.Value;
    }
    private void FillTMDataDict(KeyValuePair<string, object> pair)
    {
        if (!TMdataDict.ContainsKey(pair.Key))
            TMdataDict.Add(pair.Key, pair.Value);
        else
            TMdataDict[pair.Key] = pair.Value;
    }

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
            Dict = 3;
        }
        if (GBdataDict.Count > lengthOfColumn)
        {
            lengthOfColumn = GBdataDict.Count;
            Dict = 2;
        }
        if (WholeGameDict.Count > lengthOfColumn)
        {
            lengthOfColumn = WholeGameDict.Count;
            Dict = 4;
        }
        return Dict;
    }
    private async Task<ParticipantData> GeneratePdata()
    {
        int Dict = getLongestDict();
        ParticipantData _pData = new ParticipantData();

        switch (Dict)
        {
            case 0:

                {
                    int idx = 0;
                    string Column1 = ""; float Column2 = 0f; string Column3 = ""; string Column4 = ""; string Column5 = "";
                    float Column6 = 0f; string Column7 = ""; string Column8 = ""; string Column9 = ""; float Column10 = 0f;

                    foreach (KeyValuePair<string, float> keyValuePair in PUWdataDict)
                    {
                        if (PUWdataDict.Any())
                        {
                            Column1 = keyValuePair.Key;
                            Column2 = keyValuePair.Value;

                        }
                        if (LSdataDict.Any())
                        {

                            Column3 = LSdataDict.ElementAtOrDefault(idx).Key;
                            Column4 = LSdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (GBdataDict.Any())
                        {

                            Column5 = GBdataDict.ElementAtOrDefault(idx).Key;
                            Column6 = GBdataDict.ElementAtOrDefault(idx).Value;
                        }
                        if (TMdataDict.Any())
                        {
                            Column7 = TMdataDict.ElementAtOrDefault(idx).Key;
                            Column8 = TMdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (WholeGameDict.Any())
                        {
                            Column9 = WholeGameDict.ElementAtOrDefault(idx).Key;
                            Column10 = WholeGameDict.ElementAtOrDefault(idx).Value;
                        }
                        ParticipantRow _pRow = new ParticipantRow(Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10);
                        _pData.Rows.Add(_pRow);
                        ++idx;

                    }
                    break;
                }

            case 1:
                {
                    int idx = 0;
                    string Column1 = ""; float Column2 = 0f; string Column3 = ""; string Column4 = ""; string Column5 = "";
                    float Column6 = 0f; string Column7 = ""; string Column8 = ""; string Column9 = ""; float Column10 = 0f;

                    foreach (KeyValuePair<string, object> keyValuePair in LSdataDict)
                    {
                        if (PUWdataDict.Any())
                        {
                            Column1 = PUWdataDict.ElementAtOrDefault(idx).Key;
                            Column2 = PUWdataDict.ElementAtOrDefault(idx).Value;

                        }
                        if (LSdataDict.Any())
                        {

                            Column3 = keyValuePair.Key;
                            Column4 = keyValuePair.Value.ToString();
                        }
                        if (GBdataDict.Any())
                        {

                            Column5 = GBdataDict.ElementAtOrDefault(idx).Key;
                            Column6 = GBdataDict.ElementAtOrDefault(idx).Value;
                        }
                        if (TMdataDict.Any())
                        {
                            Column7 = TMdataDict.ElementAtOrDefault(idx).Key;
                            Column8 = TMdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (WholeGameDict.Any())
                        {
                            Column9 = WholeGameDict.ElementAtOrDefault(idx).Key;
                            Column10 = WholeGameDict.ElementAtOrDefault(idx).Value;
                        }
                        ParticipantRow _pRow = new ParticipantRow(Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10);
                        _pData.Rows.Add(_pRow);
                        ++idx;

                    }
                    break;
                }
            case 2:

                {
                    int idx = 0;
                    string Column1 = ""; float Column2 = 0f; string Column3 = ""; string Column4 = ""; string Column5 = "";
                    float Column6 = 0f; string Column7 = ""; string Column8 = ""; string Column9 = ""; float Column10 = 0f;

                    foreach (KeyValuePair<string, float> keyValuePair in GBdataDict)
                    {
                        if (PUWdataDict.Any())
                        {
                            Column1 = PUWdataDict.ElementAtOrDefault(idx).Key;
                            Column2 = PUWdataDict.ElementAtOrDefault(idx).Value;

                        }
                        if (LSdataDict.Any())
                        {

                            Column3 = LSdataDict.ElementAtOrDefault(idx).Key;
                            Column4 = LSdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (GBdataDict.Any())
                        {

                            Column5 = keyValuePair.Key;
                            Column6 = keyValuePair.Value;
                        }
                        if (TMdataDict.Any())
                        {
                            Column7 = TMdataDict.ElementAtOrDefault(idx).Key;
                            Column8 = TMdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (WholeGameDict.Any())
                        {
                            Column9 = WholeGameDict.ElementAtOrDefault(idx).Key;
                            Column10 = WholeGameDict.ElementAtOrDefault(idx).Value;
                        }
                        ParticipantRow _pRow = new ParticipantRow(Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10);
                        _pData.Rows.Add(_pRow);
                        ++idx;
                    }
                    break;
                }
            case 3:
                {
                    int idx = 0;
                    string Column1 = ""; float Column2 = 0f; string Column3 = ""; string Column4 = ""; string Column5 = "";
                    float Column6 = 0f; string Column7 = ""; string Column8 = ""; string Column9 = ""; float Column10 = 0f;

                    foreach (KeyValuePair<string, object> keyValuePair in TMdataDict)
                    {
                        if (PUWdataDict.Any())
                        {
                            Column1 = PUWdataDict.ElementAtOrDefault(idx).Key;
                            Column2 = PUWdataDict.ElementAtOrDefault(idx).Value;

                        }
                        if (LSdataDict.Any())
                        {

                            Column3 = LSdataDict.ElementAtOrDefault(idx).Key;
                            Column4 = LSdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (GBdataDict.Any())
                        {

                            Column5 = GBdataDict.ElementAtOrDefault(idx).Key;
                            Column6 = GBdataDict.ElementAtOrDefault(idx).Value;
                        }
                        if (TMdataDict.Any())
                        {
                            Column7 = keyValuePair.Key;
                            Column8 = keyValuePair.Value.ToString();
                        }
                        if (WholeGameDict.Any())
                        {
                            Column9 = WholeGameDict.ElementAtOrDefault(idx).Key;
                            Column10 = WholeGameDict.ElementAtOrDefault(idx).Value;
                        }
                        ParticipantRow _pRow = new ParticipantRow(Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10);
                        _pData.Rows.Add(_pRow);
                        ++idx;
                    }
                    break;
                }
            case 4:
                {
                    int idx = 0;
                    string Column1 = ""; float Column2 = 0f; string Column3 = ""; string Column4 = ""; string Column5 = "";
                    float Column6 = 0f; string Column7 = ""; string Column8 = ""; string Column9 = ""; float Column10 = 0f;

                    foreach (KeyValuePair<string, float> keyValuePair in WholeGameDict)
                    {
                        if (PUWdataDict.Any())
                        {
                            Column1 = PUWdataDict.ElementAtOrDefault(idx).Key;
                            Column2 = PUWdataDict.ElementAtOrDefault(idx).Value;

                        }
                        if (LSdataDict.Any())
                        {

                            Column3 = LSdataDict.ElementAtOrDefault(idx).Key;
                            Column4 = LSdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (GBdataDict.Any())
                        {

                            Column5 = GBdataDict.ElementAtOrDefault(idx).Key;
                            Column6 = GBdataDict.ElementAtOrDefault(idx).Value;
                        }
                        if (TMdataDict.Any())
                        {
                            Column7 = TMdataDict.ElementAtOrDefault(idx).Key;
                            Column8 = TMdataDict.ElementAtOrDefault(idx).Value.ToString();
                        }
                        if (WholeGameDict.Any())
                        {
                            Column9 = keyValuePair.Key;
                            Column10 = keyValuePair.Value;
                        }
                        ParticipantRow _pRow = new ParticipantRow(Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10);
                        _pData.Rows.Add(_pRow);
                        ++idx;
                    }
                    break;
                }

        }
        return _pData;
    }

    /// <summary>
    /// /TEST Do not forget to redo  
    /// string _path = Path.Combine(Application.persistentDataPath, $"{ParticipantID}.json");
    /// </summary>
    public async void SaveRawParticipantData()
    {

        string _path = Path.Combine(Application.persistentDataPath, $"{ParticipantID}.json");
        ParticipantData data = GeneratePdata().Result;
        try
        {
            string json = JsonUtility.ToJson(data, true);
            await Task.Run(() => File.WriteAllText(_path, json));
            Debug.Log("Data saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save settings data: " + e.Message);
        }

    }

    public void ResetParticipantData()
    {
        PUWdataDict.Clear();
        LSdataDict.Clear();
        GBdataDict.Clear();
        TMdataDict.Clear();
        WholeGameDict.Clear();
        ParticipantID = "";
    }
}