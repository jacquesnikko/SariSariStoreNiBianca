using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; 

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData itemData; 
    public GameObject tooltipPanel; 
    public TextMeshProUGUI tooltipText;
    
    private bool isHovering = false;
    private RectTransform tooltipRect;

    void Awake()
    {
        if (tooltipPanel != null)
        {
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            // Ensure the tooltip itself doesn't block mouse events
            if (tooltipPanel.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
            {
                cg.blocksRaycasts = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData == null || tooltipPanel == null) return;
        isHovering = true;
        tooltipPanel.SetActive(true);
        tooltipText.text = itemData.itemName;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (isHovering && tooltipRect != null)
        {
            // Get mouse position from the New Input System
            Vector2 mousePos = Pointer.current.position.ReadValue();
            
            // Offset the tooltip to the bottom-right of the cursor to avoid overlap
            tooltipRect.position = mousePos + new Vector2(40, -40);
        }
    }

    public void OnItemClicked()
    {
        if (itemData != null)
        {
            // Update the calculator total in ShopManager
            FindObjectOfType<ShopManager>().SelectItem(itemData);
        }
    }
}