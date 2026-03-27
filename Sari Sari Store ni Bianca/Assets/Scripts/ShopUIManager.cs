using UnityEngine;
using TMPro;
using System.Collections;

public class ShopUIManager : MonoBehaviour
{
    [Header("Text Elements")]
    public TextMeshProUGUI orderListText;
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI calculatorTotalText;
    public TextMeshProUGUI bayadDisplay;
    public TextMeshProUGUI inputChangeText;
    public TextMeshProUGUI itemsPressedNoteText;
    public TextMeshProUGUI dialogueText;

    [Header("Panels")]
    public GameObject speechBubble;
    public GameObject successPanel;
    public GameObject gameOverPanel;
    public GameObject threeStarsUI;
    public GameObject twoStarsUI;
    public GameObject oneStarUI;
    public float slideSpeed = 5f;

    private void OnEnable()
    {
        // Subscribe to Logic Events
        ShopManager.OnTimeUpdated += UpdateTime;
        ShopManager.OnProfitUpdated += UpdateProfit;
        ShopManager.OnTotalUpdated += UpdateTotal;
        ShopManager.OnChangeUpdated += UpdateChange;
        ShopManager.OnBayadReceived += UpdateBayad;
        ShopManager.OnNotesUpdated += UpdateNotes;
        ShopManager.OnOrderGenerated += UpdateOrder;
        ShopManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        // Prevent memory leaks
        ShopManager.OnTimeUpdated -= UpdateTime;
        ShopManager.OnProfitUpdated -= UpdateProfit;
        ShopManager.OnTotalUpdated -= UpdateTotal;
        ShopManager.OnChangeUpdated -= UpdateChange;
        ShopManager.OnBayadReceived -= UpdateBayad;
        ShopManager.OnNotesUpdated -= UpdateNotes;
        ShopManager.OnOrderGenerated -= UpdateOrder;
        ShopManager.OnGameOver -= HandleGameOver;
    }

    private void UpdateTime(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        timerText.text = string.Format("Timer: {0}:{1:00}", minutes, seconds);
    }

    private void UpdateProfit(int amount) => profitText.text = "Profit: P" + amount;
    private void UpdateTotal(int amount) => calculatorTotalText.text = "TOTAL: P" + amount;
    private void UpdateChange(int amount) => inputChangeText.text = "SUKLI: P" + amount;
    private void UpdateBayad(int amount) => bayadDisplay.text = "BAYAD: P" + amount;
    private void UpdateNotes(string notes) => itemsPressedNoteText.text = notes;
    private void UpdateOrder(string order) { dialogueText.text = order; speechBubble.SetActive(true); }

    private void HandleGameOver(bool isWin, int mistakesCount)
    {
        timerText.text = "TIME'S UP!";
        speechBubble.SetActive(false);

        if (isWin)
        {
            successPanel.SetActive(true);
            EvaluateStars(mistakesCount);
            StartCoroutine(SlidePanelUp(successPanel.GetComponent<RectTransform>()));
        }
        else
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(SlidePanelUp(gameOverPanel.GetComponent<RectTransform>()));
        }
    }

    private void EvaluateStars(int mistakes)
    {
        if (threeStarsUI != null) threeStarsUI.SetActive(mistakes == 0);
        if (twoStarsUI != null) twoStarsUI.SetActive(mistakes > 0 && mistakes <= 3);
        if (oneStarUI != null) oneStarUI.SetActive(mistakes > 3);
    }

    IEnumerator SlidePanelUp(RectTransform panelRect)
    {
        Vector2 targetPos = Vector2.zero;
        while (Vector2.Distance(panelRect.anchoredPosition, targetPos) > 0.1f)
        {
            panelRect.anchoredPosition = Vector2.Lerp(panelRect.anchoredPosition, targetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        panelRect.anchoredPosition = targetPos;
    }
}