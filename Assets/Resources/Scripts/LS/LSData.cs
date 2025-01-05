using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSData : MonoBehaviour
{

    public string strategy = "";
    private Dictionary<string, Case> visitedCases;
    public static LSData Data { get; private set; }

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
        // _IDataService = new FileDataService(new JsonSerializer());

    }
    public async void SaveData()
    {
        visitedCases = LifeSaverManager.Instance.GetCases();
        string strategy = (await LSStats.FollowedStrategy(visitedCases)).Value;
        ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("Strategy",strategy));
    }
}
