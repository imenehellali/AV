using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PurchasableItemUI : MonoBehaviour
{
    public PurchasableItem itemData;
    public Image itemImageUI;
    public TextMeshProUGUI itemNameUI;
    public TextMeshProUGUI itemQuantityUI;
    public TextMeshProUGUI itemPriceUI;
    public Button increaseButton;
    public Button decreaseButton;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        itemImageUI.sprite = itemData.itemImage;
        itemNameUI.text = itemData.itemName;
        itemQuantityUI.text = itemData.quantity.ToString();
        itemPriceUI.text = itemData.price.ToString("F2") + " €";
    }

    public void IncreaseQuantity()
    {
        itemData.quantity++;
        UpdateUI();
    }

    public void DecreaseQuantity()
    {
        if (itemData.quantity > 0)
        {
            itemData.quantity--;
            UpdateUI();
        }
    }
}
