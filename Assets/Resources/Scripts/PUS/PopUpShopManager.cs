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
    private bool isPUWScene=false;  

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
            item.increaseButton.onClick.AddListener(UpdateTotalSum);
            item.decreaseButton.onClick.AddListener(UpdateTotalSum);
        }
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    private void Update()
    {
        requiredTimeToBuy += Time.deltaTime;
        TaskProgress.Instance.updateTimer(requiredTimeToBuy);
    }
    private void UpdateTotalSum()
    {
        totalSum = 0f;

        foreach (var item in purchasableItems)
        {
            totalSum += item.itemData.price * item.itemData.quantity;
        }

        _totalSum.text = "Gesamtsumme: " + totalSum.ToString("F2") + " €";
    }

    private void OnBuyButtonClick()
    {
        MoneyManager.instance.UpdateGameAccount(-totalSum);
        GameSettings.Instance.AddPurchaseDuration(requiredTimeToBuy);

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
