using UnityEngine;
using System.Collections;

public class CalculatorUI : MonoBehaviour
{
    [Header("Animation Settings")]
    public Vector2 hiddenAnchoredPos = new Vector2(400, -400); // Bottom-right corner
    public Vector2 centerAnchoredPos = Vector2.zero;         // Exact center
    public Vector3 smallScale = new Vector3(0.5f, 0.5f, 1f);  // Scale when in corner
    public Vector3 largeScale = new Vector3(1.5f, 1.5f, 1f);  // Scale when in center
    public float transitionSpeed = 8f; 

    private RectTransform rectTransform;
    private bool isExpanded = false;
    private Coroutine activeMoveRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Initialize at the bottom-right and small
        rectTransform.anchoredPosition = hiddenAnchoredPos;
        rectTransform.localScale = smallScale;
    }

    public void ToggleCalculator()
    {
        isExpanded = !isExpanded;
        
        Vector2 targetPos = isExpanded ? centerAnchoredPos : hiddenAnchoredPos;
        Vector3 targetScale = isExpanded ? largeScale : smallScale;

        if (activeMoveRoutine != null) StopCoroutine(activeMoveRoutine);
        activeMoveRoutine = StartCoroutine(SmoothMove(targetPos, targetScale));
    }

    IEnumerator SmoothMove(Vector2 targetPos, Vector3 targetScale)
    {
        // Keep moving until we are very close to the target
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPos) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * transitionSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        // Snap to exact values
        rectTransform.anchoredPosition = targetPos;
        rectTransform.localScale = targetScale;
    }

    // Add this to your CalculatorUI script
    public void CloseCalculator()
    {
        if (isExpanded) // Only close if it's currently open
        {
            isExpanded = false;
            if (activeMoveRoutine != null) StopCoroutine(activeMoveRoutine);
            activeMoveRoutine = StartCoroutine(SmoothMove(hiddenAnchoredPos, smallScale));
        }
    }
}