using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem; // IMPORTANT: This line is required now

public class MenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    private bool isStarting = false;

    void Update()
    {
        // New Input System check for "Any Key"
        if (!isStarting && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeAndExit());
        }
        
        // Optional: Also check for Gamepad or Mouse click
        if (!isStarting && (Mouse.current.leftButton.wasPressedThisFrame || 
            (Gamepad.current != null && Gamepad.current.allControls[0].IsPressed())))
        {
             StartCoroutine(FadeAndExit());
        }
    }

    IEnumerator FadeAndExit()
    {
        isStarting = true;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadeGroup != null)
            {
                fadeGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            }
            yield return null;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}