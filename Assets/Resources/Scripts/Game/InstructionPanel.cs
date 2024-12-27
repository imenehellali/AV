using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public string _testVariableSceneName="LSScene";
    private int lvlIdx = 0;

    private void OnDestroy()
    {
        Menu.action.started -= OpenInstrPanel;
    }
    private void OnEnable()
    {
        sceneLoaded += StartInstructionPanel;

    }
    private void OnDisable()
    {
        sceneLoaded-=StartInstructionPanel;
    }
    private void StartInstructionPanel(string sceneName)
    {
        instructionPanel.SetActive(true);

        DisplayGameInstruction();
        if (IsSceneLoaded("StartScene"))
        {
            _audioSourceInstr.PlayOneShot(gameVideo); 
            levelTranscript.text = "Sie befinden sich in der Startszene, bevor eine der eigentlichen Aufgaben beginnt! Dies ist ein Platzhalter. Die spezifischen Anweisungen zu jeder Aufgabe werden hier angezeigt! Die Anweisungen zu jeder Aufgabe werden zu Beginn jeder Aufgabe abgespielt! Sie können sie unten pausieren und wieder fortsetzen oder jederzeit von diesem Panel aus abspielen!";
        }

        else if (IsSceneLoaded("PUWScene"))
        {
            lvlIdx = 0;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (IsSceneLoaded("LSScene"))
        {
            lvlIdx = 1;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (IsSceneLoaded("GBScene"))
        {
            lvlIdx = 2; 
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
        else if (IsSceneLoaded("TMScene"))
        {
            lvlIdx = 3;
            DisplayLevelInstruction(lvlIdx);
            StartCoroutine(StartLevelAfterPlay());
        }
       
    }
    private void Start()
    {
        
        Menu.action.started += OpenInstrPanel;
        StartInstructionPanel(_testVariableSceneName);
    }

    private void OpenInstrPanel(InputAction.CallbackContext callbackContext)
    {
        instructionPanel.SetActive(!instructionPanel.activeSelf);
    }
    public void CloseInstrPanel()
    {
        instructionPanel.SetActive(false);
        FindAnyObjectByType<ThrillMinerManager>()?.StartLevel();
    }
    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName && scene.isLoaded)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator StartLevelAfterPlay()
    {

        _audioSourceInstr.PlayOneShot(levelVideos[lvlIdx]);
        float length= levelVideos[lvlIdx].length;

       yield return new WaitForSeconds(length);

        instructionPanel?.SetActive(false);
        FindAnyObjectByType<GhostBusterManager>()?.StartTask();
        FindAnyObjectByType<ThrillMinerManager>()?.StartLevel();
        FindAnyObjectByType<LifeSaverManager>()?.StartLevel();
        FindAnyObjectByType<PopUpWerkManager>()?.StartLevel();
        instructionPanel?.SetActive(false);
        GameSettings.Instance?.StartLevelTimer();

    }
     private void DisplayGameInstruction()
    {
        if (!string.IsNullOrEmpty(gameTranscriptText))
        {
            //gameTranscript.text = gameTranscriptText;
            gameTranscript.text = gameTranscriptText; 
        }
    }

    private void DisplayLevelInstruction(int index)
    {
        if (levelTranscripts != null && index >= 0 && index < levelTranscripts.Count && !string.IsNullOrEmpty(levelTranscripts[index]))
        {
            //levelTranscript.text = levelTranscripts[index];
            levelTranscript.text = levelTranscripts[index].ToString();
        }
    }

    public void PlayOrUnpauseGameInstr()
    {
        
        if ( IsSceneLoaded("StartScene") && _audioSourceInstr.clip.Equals(gameVideo))
        {
            _audioSourceInstr.UnPause();
        }
        else if(IsSceneLoaded("StartScene") && !_audioSourceInstr.clip.Equals(gameVideo) )
        {
            _audioSourceInstr.PlayOneShot(gameVideo);
        
        }
        else if (!IsSceneLoaded("StartScene") && !_audioSourceInstr.clip.Equals(gameVideo) )
        {
            _audioSourceInstr.PlayOneShot(gameVideo);
        }
        else if (!IsSceneLoaded("StartScene") && _audioSourceInstr.clip.Equals(gameVideo))
        {
            _audioSourceInstr.UnPause();
        }
    }
    public void PlayOrUnpauseLevelInstr()
    {

        if (!IsSceneLoaded("StartScene") && 
            _audioSourceInstr.clip.Equals(gameVideo))
        {
            _audioSourceInstr.PlayOneShot(levelVideos[lvlIdx]);
        }
        else if (!IsSceneLoaded("StartScene") && !_audioSourceInstr.clip.Equals(gameVideo))
        {
            _audioSourceInstr.UnPause();
        }
    }
}
