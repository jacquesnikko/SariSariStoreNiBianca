using UnityEngine;

public class ApplicationSaveTrigger : MonoBehaviour
{
    private ShopManager shopManager;

    private void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
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