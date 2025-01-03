using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;

public class GameSettings : MonoBehaviour
{

    public bool goNextLevel = false;

    private string _path;
    public static GameSettings Instance { get; private set; }

    public UnityAction<string> OnSceneLoaded;
    public UnityAction<float> OnTimeUp;

    public float BetweenSceneDuration { get; private set; }
    public List<string> LevelSequence { get; private set; }
    public int CurrLvlIdx { get; private set; }
    public List<float> LevelDurations { get; private set; }


    private float _PUWBGVolume;
    private float _PUWGMVolume;
    private float _PUWWaiterVolume;
    private float _LSHelpVolume;
    private float _LSWarnVolume;
    private float _GBFeedbackVolume;
    private float _TMFeedbackVolume;
    private float _TMBGVolume;


    private List<float> purchaseDurations = new List<float>();
    private int NonRewardDrinkBoughtCount = 0;
    private int RewardDrinkBoughtCount = 0;

    public void AddNonRewardDrinksBoughtCount(int amount) => NonRewardDrinkBoughtCount += amount;
    public void AddRewardDrinksBoughtCount(int amount) => RewardDrinkBoughtCount += amount;

    private float avgTimeToBuy = 0f;

    public void AddPurchaseDuration(float amount)
    {
        if (amount != 0f)
            purchaseDurations.Add(amount);
        avgTimeToBuy=purchaseDurations.Average();
    }
    public List<float> GetPurchaseDurations() => purchaseDurations;
    public float GetPUWBGVolume() { return _PUWBGVolume; }
    public float GetPUWGMVolume() { return _PUWGMVolume; }
    public float GetPUWWaiterVolume() { return _PUWWaiterVolume; }
    public float GetLSHelpVolume() { return _LSHelpVolume; }
    public float GetLSWarnVolume() { return _LSWarnVolume; }
    public float GetGBFeedbackVolume() { return _GBFeedbackVolume; }
    public float GetTMFeedbackVolume() { return _TMFeedbackVolume; }
    public float GetTMBGVolume() { return _TMBGVolume; }

    private async void Awake()
    {
        CurrLvlIdx = 0;
        LevelDurations = new List<float>();
        LevelSequence = new List<string>();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _path = Path.Combine(Application.persistentDataPath, "GameData.json");
            Debug.Log($"Initialized path: {_path}");

            await Task.Run(async () => await LoadOrCreateData());
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void OnEnable()
    {
        OnSceneLoaded += SceneLoaded;
    }
    private void OnDisable()
    {
        OnSceneLoaded -= SceneLoaded;
    }

    public void UpdateLevelSettings(List<string> newSequence, List<float> newLevelDuration, float newBetweenSceneDuration)
    {
        LevelSequence = new List<string>();
        LevelDurations = new List<float>();

        newSequence.ForEach(sequence => { LevelSequence.Add(sequence); });
        newLevelDuration.ForEach(levelDuration => { LevelDurations.Add(levelDuration); });

        BetweenSceneDuration = newBetweenSceneDuration;
        SaveData();

    }
    public void UpdateVolumeSettings(float PUWBGVolume, float PUWGMVolume, float PUWWaiterVolume, float lSHelpVolume, float lSWarnVolume, float GBFeedbackVolume, float TMFeedbackVolume, float TMBGVolume)
    {
        _PUWBGVolume = PUWBGVolume;
        _PUWGMVolume = PUWGMVolume;
        _PUWWaiterVolume = PUWWaiterVolume;
        _LSHelpVolume = lSHelpVolume;
        _LSWarnVolume = lSWarnVolume;
        _GBFeedbackVolume = GBFeedbackVolume;
        _TMFeedbackVolume = TMFeedbackVolume;
        _TMBGVolume = TMBGVolume;
    }
    private async Task LoadOrCreateData()
    {
        if (string.IsNullOrEmpty(_path))
        {
            Debug.LogError("Path is not initialized during LoadOrCreateData.");
            return;
        }

        if (File.Exists(_path))
        {
            try
            {
                // Load data asynchronously from JSON file
                string json = await Task.Run(() => File.ReadAllText(_path));
                GameData data = JsonUtility.FromJson<GameData>(json);
                BetweenSceneDuration = data.BetweenSceneDuration;
                LevelSequence = data.LevelSequence;
                LevelDurations = data.LevelDurations;
                _PUWBGVolume = data.PUWBGVolume;
                _PUWGMVolume = data.PUWGMVolume;
                _PUWWaiterVolume = data.PUWWaiterVolume;
                _LSHelpVolume = data.LSHelpVolume;
                _LSWarnVolume = data.LSWarnVolume;
                _GBFeedbackVolume = data.GBFeedbackVolume;
                _TMFeedbackVolume = data.TMFeedbackVolume;
                _TMBGVolume = data.TMBGVolume;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load JSON: {ex.Message}");
                BetweenSceneDuration = 15f;
                LevelSequence = new List<string> { "PUWScene", "LSScene", "GBScene", "TMScene", };
                LevelDurations = new List<float> { 300f, 300f, 300f, 300f, };
                _PUWBGVolume = .5f;
                _PUWGMVolume = .5f;
                _PUWWaiterVolume = .5f;
                _LSHelpVolume = .5f;
                _LSWarnVolume = .5f;
                _GBFeedbackVolume = .5f;
                _TMFeedbackVolume = .5f;
                _TMBGVolume = .5f;
            }
        }
        else
        {
            Debug.Log("JSON file not found. Initializing with default values.");
            BetweenSceneDuration = 15f;
            LevelSequence = new List<string> { "PUWScene", "LSScene", "GBScene", "TMScene", };
            LevelDurations = new List<float> { 300f, 300f, 300f, 300f, };
            _PUWBGVolume = .5f;
            _PUWGMVolume = .5f;
            _PUWWaiterVolume = .5f;
            _LSHelpVolume = .5f;
            _LSWarnVolume = .5f;
            _GBFeedbackVolume = .5f;
            _TMFeedbackVolume = .5f;
            _TMBGVolume = .5f;
            await Task.Run(async () => await SaveData());
        }
    }
    private async Task SaveData()
    {
        GameData data = new GameData
        {
            BetweenSceneDuration = BetweenSceneDuration,
            LevelSequence = LevelSequence,
            LevelDurations = LevelDurations,
            PUWBGVolume = _PUWBGVolume,
            PUWGMVolume = _PUWGMVolume,
            PUWWaiterVolume = _PUWWaiterVolume,
            LSHelpVolume = _LSHelpVolume,
            LSWarnVolume = _LSWarnVolume,
            GBFeedbackVolume = _GBFeedbackVolume,
            TMFeedbackVolume = _TMFeedbackVolume,
            TMBGVolume = _TMBGVolume,
        };
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


    public void LoadNextScene()
    {
        if (CurrLvlIdx < LevelSequence.Count)
        {
            SceneLoaders.Instance.LoadLevel(LevelSequence[CurrLvlIdx]);
            ++CurrLvlIdx;
        }
        else
        {
            SceneLoaders.Instance.LoadLevel("EndScene");
        }
    }

    public void SceneLoaded(string sceneName)
    {
        InstructionPanel instructionPanel = FindObjectOfType<InstructionPanel>();
        if (instructionPanel != null)
        {
            instructionPanel.sceneLoaded.Invoke(sceneName);
        }

    }
    private void Update()
    {
        if(goNextLevel)
        {
            goNextLevel = false;
            LoadNextScene();
        }
    }
}
