using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class MoneyManager : MonoBehaviour
{
    private float _money;
    public float GetMoney() => _money;
   
    private float _gameAccount = 2000f;
    public float GetGameAccount() => _gameAccount;

    public UnityAction OnMoneyWon;
    public UnityAction OnLevelEnd;


    public static MoneyManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void ResetGameAccount() => _gameAccount = 2000f;
    public void ResetMoney() 
    { 
        _money = 0; 
        TaskProgress.Instance.updateMoney();
    }
    public void UpdateGameAccount(float amount)
    {
        _gameAccount += amount;
    }

    public void UpdateMoney(float amount)
    {
        _money += amount;
        OnMoneyWon.Invoke();  // Notify listeners that the money has been updated
    }
    public void StoreMoneyInSafeAccount(int levelIndex){
        Debug.Log($"saving {_money} in current level {levelIndex}");
        GameStats.UpdateSafeAccount(levelIndex, _money);
        _money = 0;
    }

}

