using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using System.IO;

[Serializable]
public class GameData 
{

    public float BetweenSceneDuration;
    public List<string> LevelSequence;
    public List<float> LevelDurations;

    public float PUWBGVolume;
    public float PUWGMVolume;
    public float PUWWaiterVolume;
    public float LSHelpVolume;
    public float LSWarnVolume;
    public float GBFeedbackVolume;
    public float TMFeedbackVolume;
    public float TMBGVolume;

}
