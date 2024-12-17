using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaders : MonoBehaviour
{
    public static SceneLoaders Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadSceneAndNotify(levelName));
    }

    private IEnumerator LoadSceneAndNotify(string levelName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        if (asyncLoad.isDone)
        {
            GameSettings.Instance.OnSceneLoaded(levelName);
        }
       
    }

    public void UnloadCurrentScene(string levelName)
    {
        SceneManager.UnloadSceneAsync(levelName);
    }
    private IEnumerator LoadPUSSceneAndNotify()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PUSScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameSettings.Instance.OnSceneLoaded("PUSScene");
    }
    public void LoadPUSScene()
    {
        if (GameSettings.Instance.CurrentLevelIndex >= GameSettings.Instance.LevelSequence.Count)
        {
            LoadEndScene();
        }
        else
        {
            StartCoroutine(LoadPUSSceneAndNotify());
            StartCoroutine(UnloadPUSScene());
        }

    }

    private IEnumerator UnloadPUSScene()
    {
        yield return new WaitForSeconds(GameSettings.Instance.BetweenSceneDuration);
        UnloadCurrentScene("PUSScene");

    }
    private void LoadEndScene()
    {
        float avgPusPurchaseDur = 0f;
        GameSettings.Instance.GetPurchaseDurations().ForEach(duration => { avgPusPurchaseDur += duration; });
        avgPusPurchaseDur /= GameSettings.Instance.GetPurchaseDurations().Count * GameSettings.Instance.BetweenSceneDuration;
        //PUWStats.Instance.SetAVGAvgPurchaseTimeFromPUS(avgPusPurchaseDur);

        StartCoroutine(LoadSceneAndNotify("EndScene"));
    }
}
