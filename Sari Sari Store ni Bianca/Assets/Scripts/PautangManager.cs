using UnityEngine;
using TMPro;
using System.Collections.Generic;

// THIS WAS THE MISSING PIECE!
// It tells Unity how to build your custom Pautang list in the Inspector.
[System.Serializable]
public class PautangScenario
{
    public string requestText;
    public int profitLoss;
    public float timeGain = 10f;
}

public class PautangManager : MonoBehaviour
{
    public TextMeshProUGUI requestDisplayText;
    public ShopManager shopManager;

    [Header("Scenarios")]
    public List<PautangScenario> scenarios;

    private PautangScenario currentScenario;

    void OnEnable()
    {
        if (scenarios.Count > 0)
        {
            currentScenario = scenarios[Random.Range(0, scenarios.Count)];
            requestDisplayText.text = currentScenario.requestText;
        }
    }

    public void ClickYes()
    {
        // Alter the PlayerModel directly through the Logic Layer
        shopManager.currentPlayer.Currency -= currentScenario.profitLoss;
        shopManager.currentPlayer.RemainingTime += currentScenario.timeGain;
        shopManager.currentPlayer.isDirty = true;

        // (Optional) If you want the UI to instantly visually update the moment they click Yes,
        // you would call a method on ShopManager here to broadcast the new profit/time values.

        gameObject.SetActive(false);
    }

    public void ClickNo() => gameObject.SetActive(false);
}