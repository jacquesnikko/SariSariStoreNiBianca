using UnityEngine;

public class UIBreathing : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pulseSpeed = 2.0f;
    [SerializeField] private float pulseAmount = 0.1f;

    private Vector3 originalScale;
    private bool isPaused = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isPaused) return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale + new Vector3(pulse, pulse, 0);
    }

    public void StopEffect()
    {
        isPaused = true;
        transform.localScale = originalScale; // Reset to normal
    }
}