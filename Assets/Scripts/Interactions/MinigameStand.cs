using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.Core;
using GameJam.Systems;

namespace GameJam.Interactions
{
    public class MinigameStand : MonoBehaviour
    {
        [Header("Configuración del Stand")]
        [Tooltip("Nombre de la escena del minijuego a cargar")]
        public string minigameSceneName;

        [Tooltip("La máscara que se gana en este minijuego (para mostrar estado)")]
        public CollectibleData associatedMask;

        [Header("Estado Visual")]
        [Tooltip("Objeto visual que indica que ya completaste este nivel (ej: la máscara flotando)")]
        public GameObject completedIndicator;

        private void Start()
        {
            CheckCompletionStatus();
        }

        private void CheckCompletionStatus()
        {
            if (associatedMask != null && InventorySystem.Instance != null)
            {
                bool isCompleted = InventorySystem.Instance.HasItem(associatedMask);
                
                if (completedIndicator != null)
                {
                    completedIndicator.SetActive(isCompleted);
                }
            }
        }

        /// <summary>
        /// Método para llamar desde un evento de Unity (UI Button, Trigger, etc.)
        /// </summary>
        public void EnterMinigame()
        {
            if (!string.IsNullOrEmpty(minigameSceneName))
            {
                SceneManager.LoadScene(minigameSceneName);
            }
            else
            {
                Debug.LogError($"[MinigameStand] No se ha asignado escena para {gameObject.name}");
            }
        }
    }
}
