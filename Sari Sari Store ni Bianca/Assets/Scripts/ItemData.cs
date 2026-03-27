using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    public string itemID; // ADD THIS: Unique ID for the database
    public string itemName;
    public int retailPrice;
    public int costPrice;
}