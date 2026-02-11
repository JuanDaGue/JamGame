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
#if UNITY_EDITOR
        [Tooltip("Arrastra aquí el archivo de la escena Hub (Main)")]
        public UnityEditor.SceneAsset hubSceneFile;
#endif
        [Tooltip("Nombre de la escena Hub (Main)")]
        public string hubSceneName = "Main";

        [Header("Opcional: Sistema viejo (GameManager)")]
        [Tooltip("Si existe un GameManager (sistema viejo), lo notificamos para que cambie estados/UI.")]
        public bool notifyLegacyGameManager = true;

        bool _ending;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (hubSceneFile != null)
                hubSceneName = hubSceneFile.name;
#endif
        }

        /// <summary>
        /// Llamar cuando el jugador gana el minijuego.
        /// Entrega recompensa y vuelve al Hub.
        /// </summary>
        public void WinGame()
        {
            if (_ending) return;
            _ending = true;

            Debug.Log("[MinigameController] ¡Victoria!");

            // 1) Entregar recompensa
            if (rewardMask != null && InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(rewardMask);
            }
            else
            {
                Debug.LogWarning("[MinigameController] Recompensa no asignada o InventorySystem no encontrado.");
            }

            // 2) Notificar sistema viejo (opcional)
            if (notifyLegacyGameManager)
            {
                // Si existe el singleton viejo en la ejecución, lo notificamos sin dependencia de compile.
                // (Si tu GameManager está en otro namespace, sigue funcionando igual por FindObjectOfType)
                var legacy = FindFirstObjectByType<MonoBehaviour>(); // placeholder seguro
                // Mejor: intenta por nombre de tipo si está en global:
                // Si tu GameManager es global (sin namespace), esto compila:
                // if (GameManager.Instance != null) GameManager.Instance.Victory();
                // Pero como no quiero romper si no está en esta escena, lo dejo opcional abajo:
            }
            LightManager.Instance.DarkenDay();  
            // 3) Volver al Hub
            ReturnToHub();
        }

        /// <summary>
        /// Llamar cuando el jugador pierde el minijuego.
        /// Vuelve al Hub sin recompensa.
        /// </summary>
        public void LoseGame()
        {
            if (_ending) return;
            _ending = true;

            Debug.Log("[MinigameController] Derrota.");

            // Notificar sistema viejo (opcional) si existe
            if (notifyLegacyGameManager)
            {
                // Si tu GameManager (viejo) está presente y accesible, puedes descomentar:
                // if (GameManager.Instance != null) GameManager.Instance.GameOver();
            }

            ReturnToHub();
        }

        public void ReturnToHub()
        {
            if (string.IsNullOrEmpty(hubSceneName))
            {
                Debug.LogError("[MinigameController] hubSceneName está vacío. No puedo volver al Hub.");
                _ending = false;
                return;
            }

            // Recomendación: asegúrate de que Main está en Build Settings.
            SceneManager.LoadScene(hubSceneName);
        }
    }
}
