using System.Collections.Generic;

[System.Serializable]
public class PlayerModel
{
    public string PlayerID;
    public int Currency; // Used as currentProfit in your game
    public float RemainingTime; // Option A: Saving the timer

    // We will keep this for future updates if you let players buy permanent upgrades
    public List<string> OwnedItemIDs = new List<string>();

    public bool isDirty = false;
}