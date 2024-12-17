using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MoneyManager: MonoBehaviour
{
    private float _money;
    public float GetMoney() => _money;
    public float ResetMoney() => _money = 0; 

    private List<float> _safeAccount;
    public List<float> GetSafeAccount() => _safeAccount;

    private  float _gameAccount=2000f;
    public float GetGameAccount() => _gameAccount;

    public UnityAction OnMoneyWon;
    public UnityAction OnLevelEnd;

    public static MoneyManager instance;
    private void Awake()
    {
        if(instance == null)    
            { instance = this; }
        else
        {
            Destroy(gameObject);
        }
        
    }
    public void UpdateGameAccount(float amount)
    {
        _gameAccount += amount;
        SaveGameAccount();  // Save the updated game account to ParticipantData
    }

    private void SaveGameAccount()
    {
        ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>("GameAccount", _gameAccount));
    }
    private void SaveSafeAccount()
    {
        ParticipantSettings.Instance.WholeGamePair.Invoke(new KeyValuePair<string, float>("SafeAccount", _gameAccount));
    }


    public void InitializeSafeAccount(int numberOfLevels)
    {
        _safeAccount = new List<float>(new float[numberOfLevels]);
    }

    public void UpdateMoney(float amount)
    {
        _money += amount;
        OnMoneyWon.Invoke();  // Notify listeners that the money has been updated
    }
    public void StoreMoneyInSafeAccount(int levelIndex)
    {
        if (_safeAccount != null && levelIndex >= 0 && levelIndex < _safeAccount.Count)
        {
            _safeAccount[levelIndex] = _money;
            _money = 0;  // Reset the money for the next level
        }
    }

    
}

