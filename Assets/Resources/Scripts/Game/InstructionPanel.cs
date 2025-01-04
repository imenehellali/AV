using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InstructionPanel : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI _testVariable;
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource _audioSourceInstrGame;
    [SerializeField]
    private AudioSource _audioSourceInstrLvl;
    [Header("Objects")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private InputActionReference Menu;
    [SerializeField] private TextMeshProUGUI gameTranscript;
    [SerializeField] private TextMeshProUGUI levelTranscript;

    [Header("General Game")]
    [TextArea]
    public string gameTranscriptText;
    public AudioClip gameVideo;

    [Header("PUW, LS, GB, TM")]
    public List<string> levelTranscripts;
    public List<AudioClip> levelVideos;

    public UnityAction<string> sceneLoaded;

    private int lvlIdx = 0;
    private string currLoadedScene;

    //All additional visits start from 2, 1 is the default count which is mendatory 
    private Dictionary<string, int> perLevelOpenCount = new Dictionary<string, int>();

    private void OnEnable()
    {
        Menu.action.started += OpenInstrPanel;
        sceneLoaded += StartInstructionPanel;
        int count = GameSettings.Instance.LevelDurations.Length;
        for (int i = 0; i < count; i++)
        {
            perLevelOpenCount.TryAdd(GameSettings.Instance.LevelSequence[i], 0);
        }
    }
    private void OnDisable()
    {
        sceneLoaded -= StartInstructionPanel;
        Menu.action.started -= OpenInstrPanel;
    }
    private void StartInstructionPanel(string sceneName)
    {
        currLoadedScene = sceneName;
        instructionPanel.SetActive(true);

        DisplayGameInstruction();
        if (SceneManager.GetSceneByName("StartScene").isLoaded)
        {
            _audioSourceInstrGame.PlayOneShot(gameVideo);
            levelTranscript.text = "Sie befinden sich in der Startszene, bevor eine der eigentlichen Aufgaben beginnt! Dies ist ein Platzhalter. Die spezifischen Anweisungen zu jeder Aufgabe werden hier angezeigt! Die Anweisungen zu jeder Aufgabe werden zu Beginn jeder Aufgabe abgespielt! Sie können sie unten pausieren und wieder fortsetzen oder jederzeit von diesem Panel aus abspielen!";
        }

        else if (SceneManager.GetSceneByName("PUWScene").isLoaded)
        {
            lvlIdx = 0;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (SceneManager.GetSceneByName("LSScene").isLoaded)
        {
            lvlIdx = 1;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (SceneManager.GetSceneByName("GBScene").isLoaded)
        {
            lvlIdx = 2;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (SceneManager.GetSceneByName("TMScene").isLoaded)
        {
            lvlIdx = 3;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }

    }
    //I don't play it automatically when they open, if they wanna play instr they gotta click
    private void OpenInstrPanel(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("entered instr panel");
        if (callbackContext.ReadValueAsButton())
        {
            _testVariable.text = "triggered ME from instruction panel";
            if (instructionPanel.activeSelf)
            {
                if (_audioSourceInstrGame.isPlaying || _audioSourceInstrLvl.isPlaying)
                {
                    _audioSourceInstrGame.Stop();
                    _audioSourceInstrLvl.Stop();
                }
                //First close will start the tasks of course!
                if (perLevelOpenCount[currLoadedScene] < 1)
                {
                    StartLevel();
                }
                perLevelOpenCount[currLoadedScene]++;

                instructionPanel.SetActive(false);
            }
            else
                instructionPanel.SetActive(true);
        }
    }
    public void CloseInstrPanel()
    {
        if (_audioSourceInstrGame.isPlaying || _audioSourceInstrLvl.isPlaying)
        {
            _audioSourceInstrGame.Stop();
            _audioSourceInstrLvl.Stop();
        }

        //First close will start the tasks of course!
        if (perLevelOpenCount[currLoadedScene] < 1)
        {
            StartLevel();
        }
        perLevelOpenCount[currLoadedScene]++;
        instructionPanel.SetActive(false);

    }

    private IEnumerator StartLevelAfterPlay()
    {

        _audioSourceInstrLvl.PlayOneShot(levelVideos[lvlIdx]);
        float length = levelVideos[lvlIdx].length;

        yield return new WaitForSeconds(length);
        perLevelOpenCount[currLoadedScene]++;
        instructionPanel.SetActive(false);
        StartLevel();
        instructionPanel?.SetActive(false);
    }
    private void StartLevel()
    {
        switch(currLoadedScene)
        {
            case "PUWScene":
                PopUpWerkManager.Instance.StartLevel();
                break;
            case "LSScene":
                LifeSaverManager.Instance.StartLevel();
                break;
            case "GBScene":
                GhostBusterManager.Instance.StartTask();
                break;
            case "TMScene":
                ThrillMinerManager.Instance.StartLevel();
                break;
            default:
                break;
        }
    }
    private void DisplayGameInstruction()
    {
        if (!string.IsNullOrEmpty(gameTranscriptText))
        {
            gameTranscript.text = gameTranscriptText;
        }
    }

    private void DisplayLevelInstruction(int index)
    {
        if (levelTranscripts != null && index >= 0 && index < levelTranscripts.Count && !string.IsNullOrEmpty(levelTranscripts[index]))
        {
            levelTranscript.text = levelTranscripts[index].ToString();
        }
    }

    public void PlayPauseGameInstr()
    {
        bool sceneLoaded = SceneManager.GetSceneByName("StartScene").isLoaded;

        if (sceneLoaded && _audioSourceInstrGame.isPlaying)
        {
            Debug.Log("start scene + game playing");
            _audioSourceInstrGame.Pause();
        }
        else if (sceneLoaded && !_audioSourceInstrGame.isPlaying)
        {
            Debug.Log("start scene + game not playing");

            _audioSourceInstrGame.PlayOneShot(gameVideo);
        }
        else if (!sceneLoaded && !_audioSourceInstrGame.isPlaying)
        {
            Debug.Log("other scene + game not playing");

            if (_audioSourceInstrLvl.isPlaying)
            {
                _audioSourceInstrLvl.Stop();
                _audioSourceInstrGame.PlayOneShot(gameVideo);
            }
            else
            _audioSourceInstrGame.UnPause();
            if(!_audioSourceInstrGame.isPlaying)
                _audioSourceInstrGame.PlayOneShot(gameVideo);

        }
        else if (!sceneLoaded && _audioSourceInstrGame.isPlaying)
        {
            Debug.Log("other scene + game playing");
            _audioSourceInstrGame.Pause();
        }
    }
    public void PlayPauseLevelInstr()
    {
        bool sceneLoaded = SceneManager.GetSceneByName("StartScene").isLoaded;
        if (sceneLoaded)
        {

        }
        else if (!sceneLoaded && !_audioSourceInstrLvl.isPlaying)
        {
            Debug.Log("other scene + lvl not playing");
            if (_audioSourceInstrGame.isPlaying)
            {
                _audioSourceInstrGame.Stop();
                _audioSourceInstrLvl.PlayOneShot(levelVideos[lvlIdx]);
            }
            else
                _audioSourceInstrLvl.UnPause();
            
            if(!_audioSourceInstrLvl.isPlaying)
                _audioSourceInstrLvl.PlayOneShot(levelVideos[lvlIdx]);
        }
        else if (!sceneLoaded && _audioSourceInstrLvl.isPlaying)
        {
            Debug.Log("other scene + lvl playing");
            _audioSourceInstrLvl.Pause();
        }
    }
}
