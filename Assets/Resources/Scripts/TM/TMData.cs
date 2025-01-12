using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ThrillMinerManager;

public class TMData : MonoBehaviour
{
   
    public static TMData Data { get; private set; }

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
    public void SaveData(List<ThrillMinerManager.ChosenPath> _pathList)
    {
        List<ThrillMinerManager.ChosenPath> pathList = _pathList;
        TMStats.ChosenPath(pathList);
    }
    
}
