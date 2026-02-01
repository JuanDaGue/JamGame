using System.Collections.Generic;
using UnityEngine;
using GameJam.Core;
using GameJam.Systems;

public class InventoryHubUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentRoot;              // Grid/Vertical Layout
    [SerializeField] private InventoryItemView itemViewPrefab;   // Prefab con Image

    private readonly Dictionary<string, InventoryItemView> viewsById = new();

    private void OnEnable()
    {
        // 1) Sync inicial (lo que ya existe en inventario)
        RebuildFromInventory();

        // 2) Escuchar cambios por eventos
        GameEvents.OnCollectibleStateChanged += OnCollectibleStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnCollectibleStateChanged -= OnCollectibleStateChanged;
    }

    private void RebuildFromInventory()
    {
        // Limpia UI
        foreach (var kv in viewsById)
        {
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        }
        viewsById.Clear();

        var inv = InventorySystem.Instance;
        if (inv == null) return;

        foreach (var item in inv.GetCollectedItems())
        {
            AddView(item);
        }
    }

    private void OnCollectibleStateChanged(CollectibleData item, bool isCollected)
    {
        if (item == null || string.IsNullOrEmpty(item.Id)) return;

        if (isCollected)
        {
            AddView(item);
        }
        else
        {
            RemoveView(item.Id);
        }
    }

    private void AddView(CollectibleData item)
    {
        if (viewsById.ContainsKey(item.Id)) return;

        if (itemViewPrefab == null || contentRoot == null) return;

        var view = Instantiate(itemViewPrefab, contentRoot);
        view.Bind(item);
        viewsById[item.Id] = view;
    }

    private void RemoveView(string itemId)
    {
        if (!viewsById.TryGetValue(itemId, out var view)) return;

        if (view != null) Destroy(view.gameObject);
        viewsById.Remove(itemId);
    }
}
