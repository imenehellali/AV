using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopUpShopManager : MonoBehaviour
{
    [SerializeField] private List<PurchasableItemUI> purchasableItems;  
    [SerializeField] private TextMeshProUGUI _totalSum;
    [SerializeField] private TextMeshProUGUI _displayTime;  
    [SerializeField] private GameObject shopCanvas;
   
    private int rewardDrinks = 0;
    private int nonRewardDrinks = 0;

    private float totalSum = 0f;
    private float requiredTimeToBuy = 0f;
    private float _pusTime = 0f;
    private bool isPUSScene = false;
    private bool isPUWScene = false;

    public UnityAction<bool> PUSSceneLoaded;
    public UnityAction<bool> PUWSceneLoaded;

    private void OnEnable()
    {
        PUSSceneLoaded += SetPUSScene;
        PUWSceneLoaded += SetPUWScene;
    }
    private void OnDisable()
    {
        PUSSceneLoaded -= SetPUSScene;
        PUWSceneLoaded -= SetPUWScene;
    }
    private void SetPUSScene(bool val)
    {
        isPUSScene = val;
        requiredTimeToBuy=_pusTime;
        ResetShop();
        shopCanvas.SetActive(val);  
    }
    private void SetPUWScene(bool val)
    {
        isPUWScene = val;
        ResetShop();
    }
    private void Start()
    {
        _pusTime = GameSettings.Instance.BetweenSceneDuration;
    }
    private void Update()
    {
        
        if (isPUSScene)
        {
            requiredTimeToBuy -= Time.deltaTime;
            UpdateTimerDisplay(requiredTimeToBuy);
        }
    }
    public void UpdateTotalSum()
    {
        totalSum = 0f;

        foreach (var item in purchasableItems)
        {
            totalSum += item.itemData.price * item.itemData.quantity;
        }

        _totalSum.text = "Gesamtsumme: " + totalSum.ToString("F2") + " €";
    }

    public void OnBuyButtonClick()
    {
        MoneyManager.instance.UpdateGameAccount(-totalSum);
        Debug.Log($"total game sum:   {MoneyManager.instance.GetGameAccount()}");
        GameSettings.Instance.AddPurchaseDuration(_pusTime - requiredTimeToBuy);

        int totalCoins = 0;
        foreach (var item in purchasableItems)
        {
            if (item.gameObject.tag == "Coin")
            {
                totalCoins += item.itemData.quantity * (int)item.itemData.price;
            }
            if (item.gameObject.tag == "Reward")
            {
                rewardDrinks += item.itemData.quantity;
            }
            if (item.gameObject.tag == "NonReward")
            {
                nonRewardDrinks += item.itemData.quantity;
            }
        }
        if (isPUWScene)
        {
            PopUpWerkManager.Instance.AddCoins(totalCoins);
        }
        Debug.Log($"adding alcohol bought {rewardDrinks}");
        Debug.Log($"adding Non alcohol bought {nonRewardDrinks}");
        GameSettings.Instance.AddNonRewardDrinksBoughtCount(nonRewardDrinks);
        GameSettings.Instance.AddRewardDrinksBoughtCount(rewardDrinks);

        SceneManager.UnloadSceneAsync("PUSScene");
        SetPUSScene(false);
    }
    public void ClosePopUpShop()
    {
        SceneManager.UnloadSceneAsync("PUSScene");
       SetPUSScene(false);
    }

    private void ResetShop()
    {
        foreach (var item in purchasableItems)
        {
            item.itemData.quantity = 0;
            item.UpdateUI();
        }
        Debug.Log($"from pop up shop manager  PUWScene? {isPUWScene} ");
        purchasableItems[0].gameObject.SetActive(isPUWScene);
        UpdateTotalSum();
    }
    private void UpdateTimerDisplay(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer % 60F);
        _displayTime.text = "Zeit:  " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
