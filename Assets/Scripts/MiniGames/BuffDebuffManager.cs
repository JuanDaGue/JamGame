using GameJam.Core;
using GameJam.Systems;
using UnityEngine;

namespace GameJam.MiniGames
{
    /// <summary>
    /// Este script gestiona la lógica "Mega Man":
    /// Verifica si el jugador tiene máscaras ganadas en OTROS minijuegos para aplicar ventajas/desventajas.
    /// </summary>
    public class BuffDebuffManager : MonoBehaviour
    {
        [Header("Requisitos (Progression System)")]
        [Tooltip("Si tienes esta máscara (o item), obtienes el Buff de Fuego en ESTE nivel")]
        [SerializeField] private CollectibleData requiredItemForFireBuff;

        [Tooltip("Si tienes esta máscara, sufres la Maldición en ESTE nivel")]
        [SerializeField] private CollectibleData cursedItem;

        [Header("Estado Actual (Read Only)")]
        [SerializeField] private bool hasFireBuff = false;
        [SerializeField] private bool hasCurseDebuff = false;

        public bool HasFireBuff => hasFireBuff;
        public bool HasCurseDebuff => hasCurseDebuff;

        private void OnEnable()
        {
            // 1. Suscribirse a la Biblia de Eventos
            GameEvents.OnCollectibleStateChanged += HandleInventoryChange;


        }

        private void OnDisable()
        {
            // SIEMPRE desuscribirse para evitar memory leaks o errores al cambiar de escena
            GameEvents.OnCollectibleStateChanged -= HandleInventoryChange;
        }

        private void Start()
        {
            // 2. Sincronizar estado inicial
            // Lo hacemos en Start para dar tiempo a que InventorySystem se inicialice si estamos en la misma escena
            InitialStateCheck();
        }

        private void InitialStateCheck()
        {
            // Necesitamos acceso al Singleton para la consulta de estado síncrona
            if (InventorySystem.Instance != null)
            {
                if (requiredItemForFireBuff != null)
                    hasFireBuff = InventorySystem.Instance.HasItem(requiredItemForFireBuff);

                if (cursedItem != null)
                    hasCurseDebuff = InventorySystem.Instance.HasItem(cursedItem);

                ApplyEffects();
            }
        }

        private void HandleInventoryChange(CollectibleData item, bool added)
        {
            // Solo actuar si el item modificado es relevante para este minijuego
            bool updateNeeded = false;

            if (requiredItemForFireBuff != null && item.Equals(requiredItemForFireBuff))
            {
                hasFireBuff = added;
                updateNeeded = true;
            }

            if (cursedItem != null && item.Equals(cursedItem))
            {
                hasCurseDebuff = added;
                updateNeeded = true;
            }

            if (updateNeeded)
            {
                ApplyEffects();
            }
        }

        private void ApplyEffects()
        {
            // Aquí iría la lógica específica del minijuego
            // Ej: PlayerStats.DamageMultiplier = hasFireBuff ? 2.0f : 1.0f;

            if (hasFireBuff)
                Debug.Log($"[{gameObject.name}] Fire Buff ACTIVO");
            else
                Debug.Log($"[{gameObject.name}] Fire Buff INACTIVO");

            if (hasCurseDebuff)
                Debug.Log($"[{gameObject.name}] Curse Debuff ACTIVO");
        }
    }
}
