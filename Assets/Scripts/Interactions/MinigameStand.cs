using GameJam.Core;
using GameJam.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Interactions
{
    public class MinigameStand : MonoBehaviour
    {
        [Header("Configuración del Stand")]
#if UNITY_EDITOR
        [Tooltip("Arrastra aquí el archivo de la escena")]
        [SerializeField] private UnityEditor.SceneAsset minigameSceneFile;
#endif
        [Tooltip("Nombre de la escena (Se llena automático al arrastrar el archivo arriba)")]
        [SerializeField] private string minigameSceneName;

        [Tooltip("La máscara que se gana en este minijuego (para mostrar estado)")]
        [SerializeField] private CollectibleData associatedMask;

        [Header("Estado Visual")]
        [Tooltip("Objeto visual que indica que ya completaste este nivel (ej: la máscara flotando)")]
        [SerializeField] private GameObject completedIndicator;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (minigameSceneFile != null)
            {
                minigameSceneName = minigameSceneFile.name;
            }
#endif
        }

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
