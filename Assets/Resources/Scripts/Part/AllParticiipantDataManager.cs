
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class AllParticiipantDataManager : MonoBehaviour
{

    private string _path = "";
    private int _CGidx = 0;
    private int _ADidx = 0;
    public int getCGidx() => _CGidx;
    public int getADidx() => _ADidx;



    public static AllParticiipantDataManager Instance { get; private set; }

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _path = Path.Combine(Application.persistentDataPath, "Indeces.json");
            Debug.Log($"Initialized path: {_path}");

            await Task.Run(async () => await LoadOrCreateData());


        }
        else
        {
            Destroy(gameObject);
        }
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
                AllParticipantData data = JsonUtility.FromJson<AllParticipantData>(json);
                _CGidx = data.CGidx;
                _ADidx = data.ADidx;
                Debug.Log($"Loaded data: CGidx = {_CGidx}, ADidx = {_ADidx}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load JSON: {ex.Message}");
                _CGidx = 0;
                _ADidx = 0;
            }
        }
        else
        {
            Debug.Log("JSON file not found. Initializing with default values.");
            _CGidx = 0;
            _ADidx = 0;
            await Task.Run(async () => await SaveData());
        }
    }
    private async Task SaveData()
    {
        try
        {
            AllParticipantData data = new AllParticipantData
            {
                CGidx = _CGidx,
                ADidx = _ADidx
            };

            string json = JsonUtility.ToJson(data, true);
            await Task.Run(() => File.WriteAllText(_path, json));
            Debug.Log("Data saved successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save JSON: {ex.Message}");
        }
    }
    public async Task IncCGidxAsync()
    {
        _CGidx++;
        await Task.Run(async () => await SaveData());
    }

    public async Task IncADidxAsync()
    {
        _ADidx++;
        await Task.Run(async () => await SaveData());
    }

}
