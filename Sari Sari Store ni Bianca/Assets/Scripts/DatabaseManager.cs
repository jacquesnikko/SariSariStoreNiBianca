using UnityEngine;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    private string saveFilePath;

    private void Awake()
    {
        // Instead of a .db file, we use a .json file
        saveFilePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");
    }

    public PlayerModel LoadPlayerSnapshot()
    {
        // If a save file exists, read it and convert the JSON back into our C# PlayerModel
        if (File.Exists(saveFilePath))
        {
            string jsonContent = File.ReadAllText(saveFilePath);
            PlayerModel loadedPlayer = JsonUtility.FromJson<PlayerModel>(jsonContent);
            return loadedPlayer;
        }
        else
        {
            // If no save file exists (first time playing), create the default starter profile
            PlayerModel newPlayer = new PlayerModel
            {
                PlayerID = "Player1",
                Currency = 0,
                RemainingTime = 90f
            };
            return newPlayer;
        }
    }

    public void SavePlayerRecord(PlayerModel player)
    {
        // Convert the C# PlayerModel into a text string
        // The 'true' formats it beautifully so you can easily read it if you open the file!
        string jsonContent = JsonUtility.ToJson(player, true);

        // Write it to the hard drive
        File.WriteAllText(saveFilePath, jsonContent);

        Debug.Log("Game Saved to: " + saveFilePath);
    }
}