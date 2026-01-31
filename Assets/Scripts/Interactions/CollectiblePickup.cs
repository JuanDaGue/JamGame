using UnityEngine;
using GameJam.Core;
using GameJam.Systems;

namespace GameJam.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class CollectiblePickup : MonoBehaviour
    {
        [Tooltip("Datos del coleccionable que representa este objeto")]
        public CollectibleData data;

        [Tooltip("Si destruir el objeto tras recogerlo")]
        public bool destroyOnPickup = true;

        [Tooltip("Efecto de particulas o sonido al recoger (opcional - placeholder)")]
        public GameObject pickupEffect;

        private void OnTriggerEnter(Collider other)
        {
            // Asumimos que el jugador tiene el tag "Player". 
            // O simplemente chequeamos si es alguien que puede recoger cosas.
            // Para simplificar, aceptamos cualquier trigger por ahora o filtramos por tag.
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (data == null)
            {
                Debug.LogError($"El objeto {gameObject.name} no tiene asignado un CollectibleData.");
                return;
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(data);
                
                // Feedback visual/sonoro
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Desactivar collider o visuales para no recoger de nuevo inmediatamente
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("No se encontró una instancia de InventorySystem en la escena.");
            }
        }
    }
}
