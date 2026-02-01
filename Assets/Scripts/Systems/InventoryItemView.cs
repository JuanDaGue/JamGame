using UnityEngine;
using UnityEngine.UI;
using GameJam.Core;

public class InventoryItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private string itemId;
    public string ItemId => itemId;

    public void Bind(CollectibleData data)
    {
        itemId = data.Id;

        if (iconImage != null)
        {
            // Ajusta "Icon" al campo real de tu CollectibleData
            iconImage.sprite = data.Icon;
            iconImage.enabled = (data.Icon != null);
        }
    }
}
