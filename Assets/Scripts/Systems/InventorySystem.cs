using System.Collections.Generic;
using UnityEngine;
using GameJam.Core;

namespace GameJam.Systems
{
    public class InventorySystem : MonoBehaviour
    {
        // Patrón Singleton Persistente (Lazy)
        private static InventorySystem _instance;
        public static InventorySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InventorySystem>();
                }
                return _instance;
            }
        }

        [SerializeField]
        [Tooltip("Items que el jugador tiene al iniciar el juego (Debug/Testing)")]
        private List<CollectibleData> initialItems = new List<CollectibleData>();

        // Estado interno
        private HashSet<string> collectedItemIds = new HashSet<string>();
        private Dictionary<string, CollectibleData> collectedItems = new Dictionary<string, CollectibleData>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // Cargar items iniciales
            foreach (var item in initialItems)
            {
                if (item != null) AddItem(item);
            }
        }

        public void AddItem(CollectibleData item)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) return;

            if (!collectedItemIds.Contains(item.id))
            {
                // Actualizar estado interno
                collectedItemIds.Add(item.id);
                collectedItems[item.id] = item;
                
                Debug.Log($"[InventorySystem] Item Agregado: {item.id} - Notificando a GameEvents");

                // INVOCAR BIBLIA DE EVENTOS
                GameEvents.OnCollectibleStateChanged?.Invoke(item, true);
                GameEvents.OnCollectibleCollectedById?.Invoke(item.id);
            }
        }

        public void RemoveItem(CollectibleData item)
        {
            if (item != null && collectedItemIds.Contains(item.id))
            {
                collectedItemIds.Remove(item.id);
                collectedItems.Remove(item.id);

                Debug.Log($"[InventorySystem] Item Removido: {item.id} - Notificando a GameEvents");

                // INVOCAR BIBLIA DE EVENTOS
                GameEvents.OnCollectibleStateChanged?.Invoke(item, false);
            }
        }

        /// <summary>
        /// Consulta directa de estado. Útil cuando un script "nace" (Start/OnEnable)
        /// y necesita saber el estado actual antes de escuchar eventos futuros.
        /// </summary>
        public bool HasItem(CollectibleData item)
        {
            if (item == null) return false;
            return collectedItemIds.Contains(item.id);
        }

        public bool HasItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return collectedItemIds.Contains(itemId);
        }
    }
}
