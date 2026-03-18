using UnityEngine;
using UnityEngine.UI;

public class CalculatorButton : MonoBehaviour
{
    public int numberValue; // Set this in the Inspector (0, 1, 2, etc.)
    private ShopManager gameManager;

   void Start()
{
    gameManager = Object.FindFirstObjectByType<ShopManager>();
    // Change "InputChangeDigit" to "AddChangeDigit"
    GetComponent<Button>().onClick.AddListener(() => gameManager.AddChangeDigit(numberValue));
}
}