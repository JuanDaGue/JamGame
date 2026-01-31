using UnityEngine;
using GameJam.Core;
using GameJam.Systems;

namespace GameJam.MiniGames
{
    /// <summary>
    /// Este script puede estar en CUALQUIER escena (Exploración, Minijuego 1, Minijuego 2).
    /// Verifica el estado al activarse y escucha cambios en tiempo real.
    /// </summary>
    public class BuffDebuffManager : MonoBehaviour
    {
        [Header("Configuración de Buffs/Debuffs")]
        public CollectibleData requiredItemForFireBuff;
        public CollectibleData cursedItem;

        [Header("Estado Actual (Read Only)")]
        public bool hasFireBuff = false;
        public bool hasCurseDebuff = false;

        private void OnEnable()
        {
            // 1. Suscribirse a la Biblia de Eventos
            GameEvents.OnCollectibleStateChanged += HandleInventoryChange;

            // 2. Sincronizar estado inicial
            // (Es posible que el item ya se tenga desde antes de cargar esta escena)
            InitialStateCheck();
        }

        private void OnDisable()
        {
            // SIEMPRE desuscribirse para evitar memory leaks o errores al cambiar de escena
            GameEvents.OnCollectibleStateChanged -= HandleInventoryChange;
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
