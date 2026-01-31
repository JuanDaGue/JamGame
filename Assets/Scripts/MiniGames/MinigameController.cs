using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.Core;
using GameJam.Systems;

namespace GameJam.MiniGames
{
    public class MinigameController : MonoBehaviour
    {
        [Header("Recompensa")]
        [Tooltip("La máscara que se entrega al ganar este minijuego")]
        public CollectibleData rewardMask;

        [Header("Navegación")]
        [Tooltip("Nombre de la escena del Hub para volver")]
        public string hubSceneName = "Hub";

        /// <summary>
        /// Llamar cuando el jugador gana el minijuego.
        /// </summary>
        public void WinGame()
        {
            Debug.Log("[MinigameController] ¡Victoria!");

            // 1. Entregar recompensa
            if (rewardMask != null && InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(rewardMask);
            }
            else
            {
                Debug.LogWarning("[MinigameController] Recompensa no asignada o InventorySystem no encontrado.");
            }

            // 2. Volver al Hub (o mostrar UI de victoria antes de volver)
            ReturnToHub();
        }

        /// <summary>
        /// Llamar cuando el jugador pierde.
        /// </summary>
        public void LoseGame()
        {
            Debug.Log("[MinigameController] Derrota.");
            // Lógica de reinicio o volver al Hub sin premio
            ReturnToHub();
        }

        public void ReturnToHub()
        {
            SceneManager.LoadScene(hubSceneName);
        }
    }
}
