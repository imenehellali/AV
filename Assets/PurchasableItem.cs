using UnityEngine;

[CreateAssetMenu(fileName = "NewPurchasableItem", menuName = "ScriptableObjects/PurchasableItem", order = 1)]
public class PurchasableItem : ScriptableObject
{
    public string itemName;
    public Sprite itemImage;
    public int quantity;
    public float price;
}
