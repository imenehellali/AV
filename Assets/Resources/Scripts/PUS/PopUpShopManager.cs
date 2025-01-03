using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopUpShopManager : MonoBehaviour
{
    [SerializeField] private List<PurchasableItemUI> purchasableItems;  // The UI elements tied to the items
    [SerializeField] private TextMeshProUGUI _totalSum;
    [SerializeField] private TextMeshProUGUI _displayTime;  // Display for the remaining time

    [SerializeField] private Button buyButton;
    [SerializeField] private Canvas shopCanvas;
    [SerializeField] private Button _closeButton;  // New close button

    private int rewardDrinks = 0;
    private int nonRewardDrinks = 0;

    private float totalSum = 0f;
    private float requiredTimeToBuy = 0f;
    private float _pusTime = 0f;
    private bool isPUWScene=false;
    private GameObject _participantPos;

    private void Start()
    {
        shopCanvas.enabled = true;
        isPUWScene = SceneManager.GetSceneByName("PUWScene").isLoaded;
        ResetShop();
        foreach (var item in purchasableItems)
        {
            if (item.gameObject.tag == "Coin")
            {
                if (isPUWScene)
                {
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
        _participantPos = RepositionOnLoad.Instance._participant;
        _pusTime = GameSettings.Instance.BetweenSceneDuration;
        Debug.Log($" PUW Time:   {_pusTime}");
    }

    private void Update()
    {
        gameObject.transform.position = new Vector3(_participantPos.transform.position.x, 2f, _participantPos.transform.position.z + 0.8f);
        requiredTimeToBuy += Time.deltaTime;
        TaskProgress.Instance.updateTimer(requiredTimeToBuy);
        UpdateTimerDisplay(_pusTime- requiredTimeToBuy);
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
        GameSettings.Instance.AddPurchaseDuration(_pusTime-requiredTimeToBuy);

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
            PUWStats.UpdateNonRewardDrinksBoughtCount(nonRewardDrinks);
            PUWStats.UpdateRewardDrinksBoughtCount(rewardDrinks);
        }
        else
        {
            GameSettings.Instance.AddNonRewardDrinksBoughtCount(nonRewardDrinks);
            GameSettings.Instance.AddNonRewardDrinksBoughtCount(rewardDrinks);
        }
        SceneManager.UnloadSceneAsync("PUSScene");
    }

    private void ResetShop()
    {
        foreach (var item in purchasableItems)
        {
            item.itemData.quantity = 0;
            item.UpdateUI();
        }

        UpdateTotalSum();
    }
    private void UpdateTimerDisplay(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer % 60F);
        _displayTime.text ="Zeit:  "+ string.Format("{0:00}:{1:00}", minutes, seconds);
    }
  
}
