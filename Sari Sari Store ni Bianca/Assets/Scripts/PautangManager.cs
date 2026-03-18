using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PautangScenario
{
    public string requestText; // e.g., "Pautang po P500 para sa gatas ng baby."
    public int profitLoss;    // How much profit you lose (e.g., 500)
    public float timeGain = 10f; // Seconds gained
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
        // Pick a random scenario when the panel opens
        if (scenarios.Count > 0)
        {
            currentScenario = scenarios[Random.Range(0, scenarios.Count)];
            requestDisplayText.text = currentScenario.requestText;
        }
    }

    public void ClickYes()
    {
        // Reward: Time | Penalty: Profit
        shopManager.currentProfit -= currentScenario.profitLoss;
        shopManager.timeLeft += currentScenario.timeGain;
        
        shopManager.UpdateUI(); // Refresh the profit display
        gameObject.SetActive(false);
    }

    public void ClickNo()
    {
        // Do nothing, just close
        gameObject.SetActive(false);
    }
}