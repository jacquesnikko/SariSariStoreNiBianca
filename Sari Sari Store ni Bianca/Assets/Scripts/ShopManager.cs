using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    // --- EVENTS FOR THE UI TO LISTEN TO ---
    public static event Action<float> OnTimeUpdated;
    public static event Action<int> OnProfitUpdated;
    public static event Action<string> OnOrderGenerated;
    public static event Action<int> OnBayadReceived;
    public static event Action<int> OnChangeUpdated;
    public static event Action<int> OnTotalUpdated;
    public static event Action<string> OnNotesUpdated;
    public static event Action<bool, int> OnGameOver;

    [Header("Inventory")]
    public List<ItemData> inventory;

    [Header("Customer Settings")]
    public SpriteRenderer customerRenderer;
    public Animator customerAnimator;
    public float customerWaitTime = 15f;
    private float currentCustomerTimer;
    private bool isCustomerPresent = false;
    private bool isBoy;

    [Header("Audio System")]
    public AudioSource voiceAudioSource;
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;
    public AudioSource jeepneyAudioSource;
    public AudioSource fanAudioSource;
    public AudioClip pabiliBoy, pabiliGirl, pautangBoy, pautangGirl, backgroundMusicClip, buttonClickClip, jeepneyClip, fanClip;

    [Header("Pautang System")]
    public GameObject pautangPanel;
    public float chanceToPop = 0.15f;
    public int targetProfit = 250;

    [Header("Customer Positioning")]
    public float customerYOffset = -1.5f;
    public float walkSpeed = 2.5f;

    // --- DATA LAYER INTEGRATION ---
    private DatabaseManager dbManager;
    public PlayerModel currentPlayer { get; private set; }

    // --- GAME STATE ---
    private int runningRetailTotal = 0;
    private int expectedOrderTotal = 0;
    private int runningCostTotal = 0;
    private int paymentReceived = 0;
    private int playerTypedChange = 0;
    private int mistakesCount = 0;
    private bool isGameOver = false;
    private bool isTimerRunning = false;

    // Changed from string to ItemData to pass to the Database for the "Transaction Log" rubric
    private List<ItemData> clickedItemsList = new List<ItemData>();

    void Start()
    {
        dbManager = FindFirstObjectByType<DatabaseManager>();
        currentPlayer = dbManager.LoadPlayerProfile("Player1"); // Rubric: Fetch by unique ID

        if (currentPlayer.RemainingTime <= 0) currentPlayer.RemainingTime = 90f;

        PlayAmbientAudio();

        StartCoroutine(RandomPautangTrigger());
        StartCoroutine(CustomerArrivalSequence());

        OnProfitUpdated?.Invoke(currentPlayer.Currency);
        customerRenderer.color = Color.white;
    }

    void PlayAmbientAudio()
    {
        if (bgmAudioSource != null && backgroundMusicClip != null) { bgmAudioSource.clip = backgroundMusicClip; bgmAudioSource.loop = true; bgmAudioSource.Play(); }
        if (jeepneyAudioSource != null && jeepneyClip != null) { jeepneyAudioSource.clip = jeepneyClip; jeepneyAudioSource.loop = true; jeepneyAudioSource.Play(); }
        if (fanAudioSource != null && fanClip != null) { fanAudioSource.clip = fanClip; fanAudioSource.loop = true; fanAudioSource.Play(); }
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null && buttonClickClip != null) sfxAudioSource.PlayOneShot(buttonClickClip);
    }

    void Update()
    {
        if (isGameOver) return;

        if (currentPlayer.RemainingTime > 0)
        {
            if (isTimerRunning)
            {
                currentPlayer.RemainingTime -= Time.deltaTime;
                OnTimeUpdated?.Invoke(currentPlayer.RemainingTime);
            }
        }
        else
        {
            currentPlayer.RemainingTime = 0;
            EndGame();
        }
    }

    public void SelectItem(ItemData data)
    {
        if (isGameOver || !isCustomerPresent) return;

        runningRetailTotal += data.retailPrice;
        runningCostTotal += data.costPrice;
        clickedItemsList.Add(data); // Store the actual item object for the database log

        OnTotalUpdated?.Invoke(runningRetailTotal);
        UpdateNoteLogic();
    }

    public void ResetPressedItems()
    {
        clickedItemsList.Clear();
        runningRetailTotal = 0;
        runningCostTotal = 0;

        UpdateNoteLogic();
        OnTotalUpdated?.Invoke(0);
    }

    void UpdateNoteLogic()
    {
        string noteContent = "";
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (ItemData item in clickedItemsList)
        {
            if (counts.ContainsKey(item.itemName)) counts[item.itemName]++;
            else counts[item.itemName] = 1;
        }

        foreach (var pair in counts) noteContent += pair.Value + " " + pair.Key + "\n";
        OnNotesUpdated?.Invoke(noteContent);
    }

    public void AddChangeDigit(int digit)
    {
        if (isGameOver || !isCustomerPresent) return;
        playerTypedChange = (playerTypedChange * 10) + digit;
        OnChangeUpdated?.Invoke(playerTypedChange);
    }

    public void DeleteDigit()
    {
        playerTypedChange /= 10;
        OnChangeUpdated?.Invoke(playerTypedChange);
    }

    public void CheckTransaction()
    {
        if (isGameOver || !isCustomerPresent) return;

        int expectedChange = paymentReceived - expectedOrderTotal;
        bool isCorrect = (playerTypedChange == expectedChange && runningRetailTotal == expectedOrderTotal);

        if (isCorrect)
        {
            int netProfit = runningRetailTotal - runningCostTotal;

            // --- RUBRIC: ATOMICITY AND CONCURRENCY ---
            // Send the entire transaction to the SQLite database. 
            // It will only return true if the Currency update AND the Transaction Log both succeed.
            bool dbSuccess = dbManager.ProcessSaleTransaction(currentPlayer.PlayerID, netProfit, clickedItemsList);

            if (dbSuccess)
            {
                currentPlayer.Currency += netProfit;
            }
            else
            {
                Debug.LogError("Database Rollback: Transaction failed to save securely.");
                isCorrect = false; // Force customer to leave angry because the system dropped the transaction
            }
        }
        else
        {
            // Simple penalty save
            dbManager.UpdateCurrency(currentPlayer.PlayerID, -10);
            currentPlayer.Currency -= 10;
            mistakesCount++;
        }

        OnProfitUpdated?.Invoke(currentPlayer.Currency);
        StartCoroutine(CustomerDeparture(isCorrect));
    }

    IEnumerator CustomerDeparture(bool wasHappy)
    {
        isCustomerPresent = false;
        isTimerRunning = false;

        FindFirstObjectByType<CalculatorUI>()?.CloseCalculator();

        if (!wasHappy)
        {
            customerAnimator.Play(isBoy ? "Angry_Boy" : "Angry_Girl");
            yield return new WaitForSeconds(1.0f);
        }

        Vector2 currentPos = customerRenderer.transform.localPosition;
        Vector2 exitPos = new Vector2(8, currentPos.y);
        yield return StartCoroutine(MoveCustomer(currentPos, exitPos));

        ResetTransactionLogic();
    }

    void ResetTransactionLogic()
    {
        runningRetailTotal = 0;
        runningCostTotal = 0;
        playerTypedChange = 0;
        paymentReceived = 0;
        clickedItemsList.Clear();

        OnNotesUpdated?.Invoke("");
        OnTotalUpdated?.Invoke(0);
        OnChangeUpdated?.Invoke(0);
        OnBayadReceived?.Invoke(0);
    }

    void EndGame()
    {
        isGameOver = true;
        // Save the final time to the database
        dbManager.UpdateTime(currentPlayer.PlayerID, currentPlayer.RemainingTime);
        bool isWin = currentPlayer.Currency >= targetProfit;
        OnGameOver?.Invoke(isWin, mistakesCount);
    }

    public void RequestSave()
    {
        // In our new SQLite setup, currency and inventory save instantly on purchase. 
        // We only need this trigger to rescue the ticking timer if the app is suddenly closed!
        if (currentPlayer != null && dbManager != null)
        {
            dbManager.UpdateTime(currentPlayer.PlayerID, currentPlayer.RemainingTime);
        }
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    // --- COROUTINES AND SPAWNING LOGIC ---

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

    IEnumerator CustomerArrivalSequence()
    {
        while (!isGameOver)
        {
            isBoy = UnityEngine.Random.value > 0.5f;
            Vector2 startPos = new Vector2(-8, customerYOffset);
            Vector2 centerPos = new Vector2(0, customerYOffset);

            yield return StartCoroutine(MoveCustomer(startPos, centerPos));

            isTimerRunning = true;

            if (voiceAudioSource != null)
            {
                AudioClip clipToPlay = isBoy ? pabiliBoy : pabiliGirl;
                if (clipToPlay != null) voiceAudioSource.PlayOneShot(clipToPlay);
            }

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

    IEnumerator RandomPautangTrigger()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(10f);

            if (UnityEngine.Random.value < chanceToPop && pautangPanel != null && !pautangPanel.activeSelf)
            {
                FindFirstObjectByType<CalculatorUI>()?.CloseCalculator();
                pautangPanel.SetActive(true);

                if (voiceAudioSource != null)
                {
                    AudioClip clipToPlay = isBoy ? pautangBoy : pautangGirl;
                    if (clipToPlay != null) voiceAudioSource.PlayOneShot(clipToPlay);
                }
            }
        }
    }

    void GenerateNewOrder()
    {
        if (inventory.Count == 0) return;

        int numberOfItems = UnityEngine.Random.Range(1, 4);
        expectedOrderTotal = 0;
        string finalOrderString = "";
        Dictionary<string, int> orderQuantities = new Dictionary<string, int>();

        for (int i = 0; i < numberOfItems; i++)
        {
            ItemData randomItem = inventory[UnityEngine.Random.Range(0, inventory.Count)];
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

        OnOrderGenerated?.Invoke(finalOrderString);

        int[] bills = { 20, 50, 100, 200, 500 };
        int payment = 0;
        float strategy = UnityEngine.Random.value;

        if (strategy < 0.4f) payment = expectedOrderTotal;
        else
        {
            foreach (int bill in bills) { if (bill >= expectedOrderTotal) { payment = bill; break; } }
            if (strategy > 0.7f) payment += (UnityEngine.Random.Range(0, 2) == 0 ? 5 : 10);
        }

        if (payment == 0) payment = 20;
        paymentReceived = payment;

        OnBayadReceived?.Invoke(paymentReceived);
    }
}