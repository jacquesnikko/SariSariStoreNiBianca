using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("Inventory")]
    public List<ItemData> inventory; 

    [Header("UI Elements")]
    public TextMeshProUGUI orderListText; 
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI calculatorTotalText;
    public TextMeshProUGUI bayadDisplay; 
    public TextMeshProUGUI inputChangeText; 
    public CalculatorUI calculatorUIScript;

    [Header("Customer Settings")]
    public SpriteRenderer customerRenderer;
    public Animator customerAnimator;
    public float customerWaitTime = 15f;
    private float currentCustomerTimer;
    private bool isCustomerPresent = false;
    private bool isBoy; 

    [Header("Dialogue System")]
    public GameObject speechBubble; 
    public TextMeshProUGUI dialogueText;

    [Header("Note System")]
    public TextMeshProUGUI itemsPressedNoteText; 
    private List<string> clickedItemsList = new List<string>(); 

    [Header("Audio System - Voices")]
    public AudioSource voiceAudioSource; 
    public AudioClip pabiliBoy; 
    public AudioClip pabiliGirl;
    public AudioClip pautangBoy;
    public AudioClip pautangGirl;

    [Header("Audio System - Music & SFX")]
    public AudioSource bgmAudioSource; 
    public AudioSource sfxAudioSource; 
    public AudioClip backgroundMusicClip;
    public AudioClip buttonClickClip;

    // --- NEW AMBIENT AUDIO ADDITIONS HERE ---
    [Header("Audio System - Ambient Sounds")]
    public AudioSource jeepneyAudioSource;
    public AudioSource fanAudioSource;
    public AudioClip jeepneyClip;
    public AudioClip fanClip;
    // ----------------------------------------

    [Header("Pautang System")]
    public GameObject pautangPanel;
    public float chanceToPop = 0.15f;

    [Header("End Game UI")]
    public GameObject successPanel;
    public GameObject gameOverPanel;
    public int targetProfit = 250; 
    public float slideSpeed = 5f;

    [Header("Star System UI")]
    public GameObject threeStarsUI; 
    public GameObject twoStarsUI;   
    public GameObject oneStarUI;
    private int mistakesCount = 0; 

    [Header("Game State")]
    public float timeLeft = 90f; 
    public int currentProfit = 0;
    private int runningRetailTotal = 0; 
    private int expectedOrderTotal = 0;
    private int runningCostTotal = 0;   
    private int paymentReceived = 0; 
    private int playerTypedChange = 0;
    private bool isGameOver = false;
    private bool isTimerRunning = false;

    [Header("Customer Positioning")]
    public float customerYOffset = -1.5f; 
    public float walkSpeed = 2.5f;

    void Start()
    {
        // Play main music
        if (bgmAudioSource != null && backgroundMusicClip != null)
        {
            bgmAudioSource.clip = backgroundMusicClip;
            bgmAudioSource.loop = true; 
            bgmAudioSource.Play();
        }

        // --- NEW AMBIENT LOGIC ---
        // Play jeepney sounds
        if (jeepneyAudioSource != null && jeepneyClip != null)
        {
            jeepneyAudioSource.clip = jeepneyClip;
            jeepneyAudioSource.loop = true; 
            jeepneyAudioSource.Play();
        }

        // Play fan sounds
        if (fanAudioSource != null && fanClip != null)
        {
            fanAudioSource.clip = fanClip;
            fanAudioSource.loop = true; 
            fanAudioSource.Play();
        }
        // -------------------------

        StartCoroutine(RandomPautangTrigger());
        StartCoroutine(CustomerArrivalSequence()); 
        UpdateUI();
        customerRenderer.color = Color.white;
        if(speechBubble != null) speechBubble.SetActive(false);
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null && buttonClickClip != null)
        {
            sfxAudioSource.PlayOneShot(buttonClickClip);
        }
    }

    IEnumerator RandomPautangTrigger()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(10f);
            
            if (Random.value < chanceToPop && pautangPanel != null && !pautangPanel.activeSelf)
            {
                if (calculatorUIScript != null) calculatorUIScript.CloseCalculator();
                pautangPanel.SetActive(true);

                if (voiceAudioSource != null)
                {
                    AudioClip clipToPlay = isBoy ? pautangBoy : pautangGirl;
                    if (clipToPlay != null) voiceAudioSource.PlayOneShot(clipToPlay);
                }
            }
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (timeLeft > 0)
        {
            if (isTimerRunning) 
            {
                timeLeft -= Time.deltaTime;
                int minutes = Mathf.FloorToInt(timeLeft / 60);
                int seconds = Mathf.FloorToInt(timeLeft % 60);
                timerText.text = string.Format("Timer: {0}:{1:00}", minutes, seconds);
            }
        }
        else
        {
            EndGame();
        }
    }

    IEnumerator CustomerArrivalSequence()
    {
        while (!isGameOver)
        {
            isBoy = Random.value > 0.5f;
            Vector2 startPos = new Vector2(-8, customerYOffset);
            Vector2 centerPos = new Vector2(0, customerYOffset);

            yield return StartCoroutine(MoveCustomer(startPos, centerPos)); 
            
            isTimerRunning = true;
            if (voiceAudioSource != null)
            {
                AudioClip clipToPlay = isBoy ? pabiliBoy : pabiliGirl;
                if (clipToPlay != null) voiceAudioSource.PlayOneShot(clipToPlay);
            }

            if (speechBubble != null) speechBubble.SetActive(true);
            customerAnimator.Play(isBoy ? "Happy_Boy" : "Happy_Girl");
            
            GenerateNewOrder();
            
            isCustomerPresent = true;
            currentCustomerTimer = customerWaitTime;

            while (currentCustomerTimer > 0 && isCustomerPresent)
            {
                currentCustomerTimer -= Time.deltaTime;
                yield return null;
            }

            if (isCustomerPresent)
            {
                yield return StartCoroutine(CustomerDeparture(false));
            }
            else 
            {
                while (Vector2.Distance(customerRenderer.transform.localPosition, centerPos) < 1f)
                {
                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(1.5f); 
        }
    }

   void GenerateNewOrder()
   {
    if (inventory.Count == 0) return;

    int numberOfItems = Random.Range(1, 4); 
    expectedOrderTotal = 0; 
    string finalOrderString = "";
    Dictionary<string, int> orderQuantities = new Dictionary<string, int>();

    for (int i = 0; i < numberOfItems; i++)
    {
        ItemData randomItem = inventory[Random.Range(0, inventory.Count)];
        expectedOrderTotal += randomItem.retailPrice; 

        if (orderQuantities.ContainsKey(randomItem.itemName)) 
            orderQuantities[randomItem.itemName]++;
        else 
            orderQuantities[randomItem.itemName] = 1;
    }

    foreach (var pair in orderQuantities)
    {
        finalOrderString += pair.Value + " " + pair.Key + "\n";
    }
    
    if (dialogueText != null) dialogueText.text = finalOrderString;

    int[] bills = { 20, 50, 100, 200, 500 };
    int payment = 0;
    float strategy = Random.value;
    
    if (strategy < 0.4f) payment = expectedOrderTotal; 
    else 
    {
        foreach (int bill in bills) { if (bill >= expectedOrderTotal) { payment = bill; break; } } 
        if (strategy > 0.7f) payment += (Random.Range(0, 2) == 0 ? 5 : 10); 
    }
    
    if (payment == 0) payment = 20; 
    paymentReceived = payment;
    bayadDisplay.text = "BAYAD: P" + paymentReceived;
   }

    public void SelectItem(ItemData data)
    {
        if (isGameOver || !isCustomerPresent) return;

        runningRetailTotal += data.retailPrice;
        runningCostTotal += data.costPrice;
        calculatorTotalText.text = "TOTAL: P" + runningRetailTotal;

        clickedItemsList.Add(data.itemName);
        UpdateNoteUI();
    }

    public void ResetPressedItems() 
    {
        clickedItemsList.Clear();
        runningRetailTotal = 0;
        runningCostTotal = 0;
        
        UpdateNoteUI();
        if (calculatorTotalText != null) calculatorTotalText.text = "TOTAL: P0";
    }

    void UpdateNoteUI()
    {
        if (itemsPressedNoteText == null) return;
        
        string noteContent = "";
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (string item in clickedItemsList)
        {
            if (counts.ContainsKey(item)) counts[item]++;
            else counts[item] = 1;
        }

        foreach (var pair in counts)
        {
            noteContent += pair.Value + " " + pair.Key + "\n";
        }

        itemsPressedNoteText.text = noteContent;
    }

    public void AddChangeDigit(int digit)
    {
        if (isGameOver || !isCustomerPresent) return;
        playerTypedChange = (playerTypedChange * 10) + digit;
        inputChangeText.text = "SUKLI: P" + playerTypedChange;
    }

    public void DeleteDigit()
    {
        playerTypedChange /= 10;
        inputChangeText.text = "SUKLI: P" + playerTypedChange;
    }

   public void CheckTransaction()
   {
    if (isGameOver || !isCustomerPresent) return;
    
    int expectedChange = paymentReceived - expectedOrderTotal;
    
    bool isCorrect = (playerTypedChange == expectedChange && runningRetailTotal == expectedOrderTotal);

    if (isCorrect)
    {
        currentProfit += (runningRetailTotal - runningCostTotal);
    }
    else
    {
        currentProfit -= 10;
        mistakesCount++;
    }

    StartCoroutine(CustomerDeparture(isCorrect));
    if (calculatorUIScript != null) calculatorUIScript.CloseCalculator();
    UpdateUI();
   }

    IEnumerator CustomerDeparture(bool wasHappy)
    {
        isCustomerPresent = false;
        isTimerRunning = false;
        if (speechBubble != null) speechBubble.SetActive(false); 
        if (calculatorUIScript != null) calculatorUIScript.CloseCalculator();
        if (!wasHappy)
        {
            customerAnimator.Play(isBoy ? "Angry_Boy" : "Angry_Girl");
            yield return new WaitForSeconds(1.0f); 
        }

        Vector2 currentPos = customerRenderer.transform.localPosition;
        Vector2 exitPos = new Vector2(8, currentPos.y); 
        yield return StartCoroutine(MoveCustomer(currentPos, exitPos));
        
        ResetTransactionUI();
    }

    void ResetTransactionUI()
    {
        runningRetailTotal = 0;
        runningCostTotal = 0;
        playerTypedChange = 0;
        paymentReceived = 0;
        
        clickedItemsList.Clear();
        if (itemsPressedNoteText != null) itemsPressedNoteText.text = "";
        
        if (calculatorTotalText != null) calculatorTotalText.text = "TOTAL: P0";
        if (inputChangeText != null) inputChangeText.text = "SUKLI: P0";
        if (bayadDisplay != null) bayadDisplay.text = "BAYAD: P0";
    }

    IEnumerator MoveCustomer(Vector2 start, Vector2 end)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * walkSpeed;
            customerRenderer.transform.localPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
    }

    public void UpdateUI() => profitText.text = "Profit: P" + currentProfit;

    void EndGame()
    {
        isGameOver = true;
        timerText.text = "TIME'S UP!";
        if (currentProfit >= targetProfit)
        {
            successPanel.SetActive(true);
            EvaluateStars();
            StartCoroutine(SlidePanelUp(successPanel.GetComponent<RectTransform>()));
        }
        else
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(SlidePanelUp(gameOverPanel.GetComponent<RectTransform>()));
        }
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

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void EvaluateStars()
    {
        if (threeStarsUI != null) threeStarsUI.SetActive(false);
        if (twoStarsUI != null) twoStarsUI.SetActive(false);
        if (oneStarUI != null) oneStarUI.SetActive(false);

        if (mistakesCount == 0)
        {
            if (threeStarsUI != null) threeStarsUI.SetActive(true);
        }
        else if (mistakesCount <= 3)
        {
            if (twoStarsUI != null) twoStarsUI.SetActive(true);
        }
        else
        {
            if (oneStarUI != null) oneStarUI.SetActive(true);
        }
    }
}