using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityAction<string> OnPlayMachine;
    public UnityAction<int> OnPurchase;

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
        MoneyManager.instance.ResetMoney();
    }
    private void OnEnable()
    {
        OnPlayMachine+=UpdatePlayCount;
        OnPlayMachine+=ConsumeCoins;
        OnPurchase+=AddCoins;
    }

    private void OnDisable()
    {
        OnPlayMachine-=UpdatePlayCount;
        OnPlayMachine-=ConsumeCoins;
        OnPurchase-=AddCoins;
    }
    public void StartLevel()
    {

        StartCoroutine(ReduceMoneyOverTime());
        StartCoroutine(CheckSlotMachinePlays());
        levelDuration = GameSettings.Instance.LevelDurations[GameSettings.Instance.CurrentLevelIndex - 1];
        PUWStats.AddOVerallTaskTime(levelDuration);
    }
    private void ConsumeCoins(string slotType)
    {
        int coinsRequired = 0;

        switch (slotType)
        {
            case "DiamondSlot":
                coinsRequired = 2;
                break;
            case "BillSlot":
                coinsRequired = 3;
                break;
            case "CakeSlot":
                coinsRequired = 5;
                break;
            case "MysterySlot":
                coinsRequired = 1;
                break;
            default:
                Debug.LogError("Invalid slot type");
                return;
        }

        if (_coins >= coinsRequired)
        {
            _coins -= coinsRequired;
            Debug.Log($"Coins consumed: {coinsRequired}, Coins left: {_coins}");
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
        PUWStats.AddCoins(amount);  // Track the coins in PUWStats as well

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
        float currTime=levelDuration;
        while (currTime>0)
        {

            yield return new WaitForSeconds(60); // Wait for 1 minute
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
        while (GameSettings.Instance.IsSceneLoaded("PUWScene"))
        {
            yield return new WaitForSeconds(60);  // Wait for 1 minute
            MoneyManager.instance.UpdateMoney(MoneyManager.instance.GetMoney() * -0.2f);
        }
    }
}

