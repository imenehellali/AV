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

    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private InputActionReference Menu;
    [SerializeField]
    private AudioSource _audioSourceInstr;
    [SerializeField] private TextMeshProUGUI gameTranscript;

    [SerializeField] private TextMeshProUGUI levelTranscript;
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
        if (!Menu.IsUnityNull()) 
            Menu.action.started += OpenInstrPanel;
        sceneLoaded += StartInstructionPanel;
        int count = GameSettings.Instance.LevelDurations.Count;
        for (int i = 0; i < count; i++)
        {
            perLevelOpenCount.TryAdd(GameSettings.Instance.LevelSequence[i], 0);
        }
    }
    private void OnDisable()
    {
        sceneLoaded -= StartInstructionPanel;
        if (!Menu.IsUnityNull()) 
            Menu.action.started -= OpenInstrPanel;
    }
    private void StartInstructionPanel(string sceneName)
    {
        currLoadedScene = sceneName;
        instructionPanel.SetActive(true);

        DisplayGameInstruction();
        if (SceneManager.GetSceneByName("StartScene").isLoaded)
        {
            _audioSourceInstr.PlayOneShot(gameVideo);
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
        if (instructionPanel.activeSelf)
        {
            if (_audioSourceInstr.isPlaying)
                _audioSourceInstr.Stop();
            //Case 1st close with menuButton
            if (lvlIdx > 0)
            {
                //First close will start the tasks of course!
                if (perLevelOpenCount[currLoadedScene] < 1)
                {
                    FindAnyObjectByType<GhostBusterManager>()?.StartTask();
                    FindAnyObjectByType<ThrillMinerManager>()?.StartLevel();
                    FindAnyObjectByType<LifeSaverManager>()?.StartLevel();
                    FindAnyObjectByType<PopUpWerkManager>()?.StartLevel();
                }
                perLevelOpenCount[currLoadedScene]++;
            }
            instructionPanel.SetActive(false);
        }
        instructionPanel.SetActive(true);
       
    }
    public void CloseInstrPanel()
    {
        if (_audioSourceInstr.isPlaying)
            _audioSourceInstr.Stop();

        if (lvlIdx > 0)
        {
            //First close will start the tasks of course!
            if (perLevelOpenCount[currLoadedScene] < 1)
            {
                FindAnyObjectByType<GhostBusterManager>()?.StartTask();
                FindAnyObjectByType<ThrillMinerManager>()?.StartLevel();
                FindAnyObjectByType<LifeSaverManager>()?.StartLevel();
                FindAnyObjectByType<PopUpWerkManager>()?.StartLevel();
            }
            perLevelOpenCount[currLoadedScene]++;
        }
        instructionPanel.SetActive(false);

    }

    private IEnumerator StartLevelAfterPlay()
    {

        _audioSourceInstr.PlayOneShot(levelVideos[lvlIdx]);
        float length = levelVideos[lvlIdx].length;

        yield return new WaitForSeconds(length);
        perLevelOpenCount[currLoadedScene]++;
        instructionPanel.SetActive(false);

        FindAnyObjectByType<GhostBusterManager>()?.StartTask();
        FindAnyObjectByType<ThrillMinerManager>()?.StartLevel();
        FindAnyObjectByType<LifeSaverManager>()?.StartLevel();
        FindAnyObjectByType<PopUpWerkManager>()?.StartLevel();
        instructionPanel?.SetActive(false);
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

    public void PlayOrUnpauseGameInstr()
    {
        string currName = _audioSourceInstr.clip.name;
        bool sceneLoaded = SceneManager.GetSceneByName("StartScene").isLoaded;

        if (sceneLoaded && currName.Equals(gameVideo.name))
        {
            _audioSourceInstr.UnPause();
        }
        else if (sceneLoaded && !currName.Equals(gameVideo.name))
        {
            if (_audioSourceInstr.isPlaying)
                _audioSourceInstr.Stop();
            _audioSourceInstr.PlayOneShot(gameVideo);
        }
        else if (!sceneLoaded && !currName.Equals(gameVideo.name))
        {
            if (_audioSourceInstr.isPlaying)
                _audioSourceInstr.Stop();
            _audioSourceInstr.PlayOneShot(gameVideo);
        }
        else if (!sceneLoaded && currName.Equals(gameVideo.name))
        {
            _audioSourceInstr.UnPause();
        }
    }
    public void PlayOrUnpauseLevelInstr()
    {
        bool sceneLoaded = SceneManager.GetSceneByName("StartScene").isLoaded;
        string currName = _audioSourceInstr.clip.name;
        if (sceneLoaded)
        {

        }
        else if (!sceneLoaded && currName.Equals(gameVideo.name))
        {
            if (_audioSourceInstr.isPlaying)
                _audioSourceInstr.Stop();
            _audioSourceInstr.PlayOneShot(levelVideos[lvlIdx]);
        }
        else if (!sceneLoaded && currName.Equals(levelVideos[lvlIdx].name))
        {
            _audioSourceInstr.UnPause();
        }
    }
}
