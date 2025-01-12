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

    }
    public async void SaveData(Dictionary<string,Case> Cases)
    {
       
        Dictionary<string,Case> visitedCases = Cases;
        Debug.Log($"got the cases dictionary {Cases.Count}");
        string strategy = (await LSStats.FollowedStrategy(visitedCases)).Value;
        Debug.Log($"Strategy finished?  {strategy}");
        ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("Strategy",strategy));
    }
}
