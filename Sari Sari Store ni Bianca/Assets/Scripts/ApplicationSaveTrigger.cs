using UnityEngine;

public class ApplicationSaveTrigger : MonoBehaviour
{
    private ShopManager shopManager;

    private void Start()
    {
        // Updated to the modern Unity API standard for better performance
        shopManager = FindFirstObjectByType<ShopManager>();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused && shopManager != null) shopManager.RequestSave();
    }

    private void OnApplicationQuit()
    {
        if (shopManager != null) shopManager.RequestSave();
    }
}