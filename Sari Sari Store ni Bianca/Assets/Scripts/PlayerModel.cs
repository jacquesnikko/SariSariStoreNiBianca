using System.Collections.Generic;

[System.Serializable] // This tag is MANDATORY for JsonUtility to work
public class PlayerModel
{
    public string PlayerID;
    public int Currency; // This tracks your currentProfit
    public float RemainingTime; // This tracks your countdown timer

    // We keep this list ready for future updates if you want players to unlock permanent items
    public List<string> OwnedItemIDs = new List<string>();

    // This flag tells the ApplicationSaveTrigger when to write to the hard drive.
    // [System.NonSerialized] hides it from the JSON file because it's only needed while the game is running.
    [System.NonSerialized]
    public bool isDirty = false;
}