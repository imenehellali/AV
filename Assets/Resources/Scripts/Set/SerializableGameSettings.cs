using System.Collections.Generic;

[System.Serializable]
public class SerializableGameSettings
{
    public List<string> LevelSequence { get; set; }
    public List<float> LevelDurations { get; set; }
    public float BetweenSceneDuration { get; set; }

    public float PUWBGVolume { get; set; }
    public float PUWGMVolume { get; set; }
    public float PUWWaiterVolume { get; set; }
    public float LSHelpVolume { get; set; }
    public float LSWarnVolume { get; set; }
    public float GBFeedbackVolume { get; set; }
    public float TMFeedbackVolume { get; set; }
    public float TMBGVolume { get; set; }

    public SerializableGameSettings(GameSettings settings)
    {
        LevelSequence = settings.LevelSequence;
        LevelDurations = settings.LevelDurations;
        BetweenSceneDuration = settings.BetweenSceneDuration;

        PUWBGVolume = settings.GetPUWBGVolume();
        PUWGMVolume = settings.GetPUWGMVolume();
        PUWWaiterVolume = settings.GetPUWWaiterVolume();
        LSHelpVolume = settings.GetLSHelpVolume();
        LSWarnVolume = settings.GetLSWarnVolume();
        GBFeedbackVolume = settings.GetGBFeedbackVolume();
        TMFeedbackVolume = settings.GetTMFeedbackVolume();
        TMBGVolume = settings.GetTMBGVolume();
    }
}