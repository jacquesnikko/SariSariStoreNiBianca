using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class AnyKeyTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private UIBreathing breathingScript; 
    [SerializeField] private string sceneToLoad = "Gameplay";

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1.0f;

    private bool isTriggered = false;

    void Update()
    {
        if (isTriggered) return;

        // Modern Input System Check
        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartTransition();
        }
    }

    private void StartTransition()
    {
        isTriggered = true;

        // Tell the breathing script to stop
        if (breathingScript != null)
        {
            breathingScript.StopEffect();
        }

        StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadeGroup != null)
                fadeGroup.alpha = Mathf.MoveTowards(fadeGroup.alpha, 1, Time.deltaTime / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}