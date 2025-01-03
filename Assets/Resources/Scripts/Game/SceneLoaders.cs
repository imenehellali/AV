using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
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
            RepositionOnLoad.Instance.repositionOnLoad(levelName);
            if (levelName.Equals("EndScene"))
            {
                float avgPusPurchaseDur = 0f;
                GameSettings.Instance.GetPurchaseDurations().ForEach(duration => { avgPusPurchaseDur += duration; });
                avgPusPurchaseDur /= GameSettings.Instance.GetPurchaseDurations().Count * GameSettings.Instance.BetweenSceneDuration;
            }
            else
            {
                AsyncOperation _asyncLoad = SceneManager.LoadSceneAsync("PUSScene", LoadSceneMode.Additive);
                while (!_asyncLoad.isDone)
                {
                    yield return null;
                }
                if (_asyncLoad.isDone)
                {
                    yield return new WaitForSeconds(GameSettings.Instance.BetweenSceneDuration);
                    if (SceneManager.GetSceneByName("PUScene").isLoaded)
                    {
                        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("PUSScene");
                        while (!unloadOp.isDone)
                        {
                            yield return null;
                        }
                        if (unloadOp.isDone)
                            GameSettings.Instance.OnSceneLoaded(levelName);
                    }
                    else GameSettings.Instance.OnSceneLoaded(levelName);
                }

            }
        }

    }
}
