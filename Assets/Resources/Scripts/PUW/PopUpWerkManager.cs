using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PopUpWerkManager : MonoBehaviour
{
    public static PopUpWerkManager Instance { get; private set; }

    private int _coins = 0;

    private int diamondSlotPlaysInMinute = 0;
    private int billSlotPlaysInMinute = 0;
    private int mysterySlotPlaysInMinute = 0;


    private int diamondSlotPlays = 0;
    private int billSlotPlays = 0;
    private int cakeSlotPlays = 0;
    private int mysterySlotPlays = 0;

    private float levelDuration;
    private float levelTimer = 0f;
    private float _time = 0f;

    public UnityAction<string> OnPlayMachine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        levelDuration = GameSettings.Instance.LevelDurations[GameSettings.Instance.CurrLvlIdx];

    }
    private void OnEnable()
    {
        OnPlayMachine += UpdatePlayCount;
        OnPlayMachine += ConsumeCoins;
    }

    private void OnDisable()
    {
        OnPlayMachine -= UpdatePlayCount;
        OnPlayMachine -= ConsumeCoins;
    }
    private IEnumerator StartLevelTimer()
    {
        while (levelTimer < levelDuration)
        {
            levelTimer += Time.deltaTime;
            _time = levelDuration - levelTimer;
            TaskProgress.Instance.updateTimer(_time);
            yield return null;
        }
        if (levelTimer >= levelDuration)
        {
            EndLevel();
        }
    }
    private void EndLevel()
    {
        StopAllCoroutines();
        PUWStats.SaveStatsToParticipantData();
        MoneyManager.instance.StoreMoneyInSafeAccount(GameSettings.Instance.CurrLvlIdx);
        NonRewardObject[] nonRewardObjects = FindObjectsOfType<NonRewardObject>();
        RewardObject[] rewardObjects = FindObjectsOfType<RewardObject>();

        foreach (var nonRewardObject in nonRewardObjects)
        {
            nonRewardObject.UpdateTotalFixationTime();
        }
        foreach (var rewardObject in rewardObjects)
        {
            rewardObject.UpdateTotalFixationTime();
        }
        GameSettings.Instance.LoadNextScene();
    }
    private IEnumerator ShowPUS()
    {
        yield return new WaitForSeconds(15f);
        if (SceneManager.GetSceneByName("PUSScene").isLoaded)
            SceneManager.UnloadSceneAsync("PUSScene");
    }
    private IEnumerator StartPUSRandShow()
    {
        while (levelTimer < levelDuration)
        {
            SceneManager.LoadSceneAsync("PUSScene", LoadSceneMode.Additive);
            StartCoroutine(ShowPUS());
            yield return new WaitForSeconds(45f);
        }
    }
    public void StartLevel()
    {
        MoneyManager.instance.ResetMoney();
        StartCoroutine(StartLevelTimer());
        StartCoroutine(StartPUSRandShow());
        StartCoroutine(ReduceMoneyOverTime());
        StartCoroutine(CheckSlotMachinePlays());
        PUWStats.AddOVerallTaskTime(levelDuration);
    }
    private void ConsumeCoins(string slotType)
    {
        switch (slotType)
        {
            case "DiamondSlot":
                _coins -= 2;
                break;
            case "BillSlot":
                _coins -= 3;
                break;
            case "CakeSlot":
                _coins-= 5;
                break;
            case "MysterySlot":
                _coins -= 1;
                break;
            default:
                Debug.LogError("Invalid slot type");
                return;
        }
    }
    private void UpdatePlayCount(string slotType)
    {
        switch (slotType)
        {
            case "DiamondSlot":
                diamondSlotPlays++;
                PUWStats.IncrementDiamondSlotPlayCount();
                diamondSlotPlaysInMinute++;
                break;
            case "BillSlot":
                billSlotPlays++;
                PUWStats.IncrementBillSlotPlayCount();
                billSlotPlaysInMinute++;
                break;
            case "CakeSlot":
                cakeSlotPlays++;
                PUWStats.IncrementCakeSlotPlayCount();
                break;
            case "MysterySlot":
                mysterySlotPlays++;
                PUWStats.IncrementMysterySlotPlayCount();
                mysterySlotPlaysInMinute++;
                break;
            default:
                Debug.LogError("Invalid slot type");
                break;
        }
    }

    public void AddCoins(int amount)
    {
        _coins += amount;

        Debug.Log($"coins amount:  {_coins}");
        PUWStats.AddCoins(amount);

    }
    public int GetDiamondSlotPlays() => diamondSlotPlays;
    public int GetBillSlotPlays() => billSlotPlays;
    public int GetCakeSlotPlays() => cakeSlotPlays;
    public int GetMysterySlotPlays() => mysterySlotPlays;
    public int GetCoins()
    {
        return _coins;
    }
    private IEnumerator CheckSlotMachinePlays()
    {
        float currTime = levelDuration;
        while (currTime > 0)
        {
            yield return new WaitForSeconds(60);
            currTime -= 60;
            if (diamondSlotPlaysInMinute >= 4 || billSlotPlaysInMinute >= 4)
            {
                MoneyManager.instance.UpdateGameAccount(MoneyManager.instance.GetGameAccount() * 0.2f);
            }
            PUWStats.IncrementMysterySlotPlaysPerMinute();
            PUWStats.IncrementDiamondSlotPlaysPerMinute();
            PUWStats.IncrementBillSlotPlaysPerMinute();
            // Reset counter for the next minute
            mysterySlotPlaysInMinute = 0;
            // Reset counters for the next minute
            diamondSlotPlaysInMinute = 0;
            billSlotPlaysInMinute = 0;
        }
    }
    private IEnumerator ReduceMoneyOverTime()
    {
        float _1min = 0f;
        while (_1min < levelDuration)
        {
            _1min += 60f;
            yield return new WaitForSeconds(60);  // Wait for 1 minute
            MoneyManager.instance.UpdateGameAccount(MoneyManager.instance.GetGameAccount() * -0.2f);
        }
    }
}

