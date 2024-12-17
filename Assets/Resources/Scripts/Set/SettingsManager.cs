using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class SettingsManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] 
    private List<LevelSetting> _levels= new List<LevelSetting>();
    private List<LevelSetting> _selectedLevels= new List<LevelSetting>();


    private List<string> levelSequence = new List<string>();
    private List<float> levelDurations = new List<float>();
    
    private float PUWBGVolume = 0.5f;
    private float PUWGMVolume = 0.5f;
    private float PUWWaiterVolume = 0.5f;
    private float lSHelpVolume = 0.5f;
    private float lSWarnVolume = 0.5f;
    private float GBFeedbackVolume = 0.5f;
    private float TMFeedbackVolume = 0.5f;
    private float TMBGVolume = 0.5f;

    //intermediate variable for local saving before sending to gameSettings
    private float PUWduration = 0f;
    private float LSDuration = 0f;
    private float TMDuration = 0f;
    private float GBFDuration = 0f;
    private float PUSDuration = 0f;


    //Setting Volume Functions
    public void SetPUWBGVolume(float volume)=>PUWBGVolume = volume;
    public void SetPUWGMVolume(float volume)=>PUWGMVolume = volume;    
    public void SetPUWWaiterVolume(float volume) =>PUWWaiterVolume = volume;
    public void SetlSHelpVolume(float volume)=>lSHelpVolume = volume;
    public void SetlSWarnVolume(float volume) => lSWarnVolume = volume;
    public void SetGBFeedbackVolume(float volume)=> GBFeedbackVolume = volume;
    public void SetTMFeedbackVolume(float volume)=> TMFeedbackVolume = volume;
    public void SetTMBGVolume(float volume)=>TMBGVolume = volume;
   
    //Setting for the different levels durations Functions
    public void SetPUSDuration(float _val)=>PUSDuration = _val;
    public void SetPUWDuration(float _val)=>PUWduration = _val;
    public void SetLSDuration(float _val)=> LSDuration = _val;
    public void SetTMDuration(float _Val)=>TMDuration = _Val;
    public void SetGBDuration(float _val)=>GBFDuration=_val;

    //Setting the level list and its corresponding durations
   
    public void SaveSettings()
    {
        levelDurations.Clear();
        levelSequence.Clear();

        _levels.ForEach(level =>
        {
            if (level.levelSelected)
               _selectedLevels.Add(level);
        });

        _selectedLevels.OrderBy(level => level.levelOrder);
        _selectedLevels.ForEach(level =>
        {
            levelSequence.Add(level.levelName);
            levelDurations.Add(level.levelDuration);

        });
        // Update and save the new settings
        GameSettings.Instance.UpdateLevelSettings(levelSequence, levelDurations, PUSDuration);
        MoneyManager.instance.InitializeSafeAccount(levelSequence.Count);
        GameSettings.Instance.UpdateVolumeSettings(PUWBGVolume, PUWGMVolume, PUWWaiterVolume, lSHelpVolume, lSWarnVolume, GBFeedbackVolume, TMFeedbackVolume, TMBGVolume);
        
    }
}
