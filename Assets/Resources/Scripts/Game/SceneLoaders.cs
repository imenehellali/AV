using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaders : MonoBehaviour
{
    public static SceneLoaders Instance { get; private set; }
    [SerializeField]
    private InstructionPanel _instrPanel;
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
                CalculateAndSaveGameStats();
            }
            else if (!levelName.Equals("StartScene"))
            {
                AsyncOperation _asyncLoad = SceneManager.LoadSceneAsync("PUSScene", LoadSceneMode.Additive);
                while (!_asyncLoad.isDone)
                {
                    yield return null;
                }
                if (_asyncLoad.isDone)
                {
                    yield return new WaitForSeconds(GameSettings.Instance.BetweenSceneDuration);
                    if (SceneManager.GetSceneByName("PUSScene").isLoaded)
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

    private void CalculateAndSaveGameStats()
    {
        //Instr Panel
        List<int> instrCounts = _instrPanel.GetPerLEvelOpenCount();
        Debug.Log($"instruction counts is {instrCounts.Count}");
        GameStats.SaveCheckedInstrCount(instrCounts);

        //PUS
        ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>(" avgPurchaseDuration", GameStats.GetAveragePurchaseDuration()));
        ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>("NonRewardDrinksBoughtCount", (float)GameStats.GetNonRewardDrinksBoughtCount()));
        ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>("RewardDrinksBoughtCount", (float)GameStats.GetRewardDrinksBoughtCount()));

        //Money Manager
        GameStats.SaveSafeAcount();
        GameStats.SaveGameAccount(MoneyManager.instance.GetMoney());
        GameSettings.Instance.OnSceneLoaded("EndScene");
    }
}
