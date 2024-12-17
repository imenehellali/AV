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
    private float timer;
    private float avgTimeToPurchase = 0f;
    private void Start()
    {
        // Set the timer to the BetweenSceneDuration from GameSettings
        timer = GameSettings.Instance.BetweenSceneDuration;

        shopCanvas.enabled = true;
        UpdateTotalSum();
        foreach (var item in purchasableItems)
        {
            if (item.gameObject.tag == "Coin")
            {
                if (IsPUWSceneLoaded())
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
        _closeButton.onClick.AddListener(UnloadPUSScene);  // Close the panel when the close button is clicked

        StartCoroutine(SceneTimerCoroutine()); // Start the timer
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

        // Update the _coins in PopUpWerkManager only if PUWScene is loaded
        if (IsPUWSceneLoaded())
        {
            int totalCoins = 0;
            foreach (var item in purchasableItems)
            {
                if (item.gameObject.tag == "Coin")
                {
                    totalCoins += item.itemData.quantity;
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
            PopUpWerkManager.Instance.AddCoins(totalCoins);
            PUWStats.UpdateNonRewardDrinksBoughtCount(nonRewardDrinks);
            PUWStats.UpdateRewardDrinksBoughtCount(rewardDrinks);

        }

        ResetShop();
        UnloadPUSScene();
    }
    private bool IsPUWSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "PUWScene")
            {
                return true;
            }
        }
        return false;
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

    private void UnloadPUSScene()
    {
        shopCanvas.enabled = false;
        avgTimeToPurchase = GameSettings.Instance.BetweenSceneDuration - timer;
        GameSettings.Instance.AddPurchaseDuration(avgTimeToPurchase);
        string pusSceneName = "PUSScene"; // Ensure this matches the exact name of the scene
        SceneLoaders.Instance.UnloadCurrentScene(pusSceneName);  // Unload only the PUSScene
    }
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer % 60F);
        _displayTime.text ="Zeit:  "+ string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private IEnumerator SceneTimerCoroutine()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerDisplay();  // Update the timer display every frame
            yield return null;
        }
        UnloadPUSScene();
    }
}
