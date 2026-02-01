using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.Core;
using GameJam.Systems;

namespace GameJam.Interactions
{
    /// <summary>
    /// Stand de minijuego que se activa automáticamente
    /// cuando el Player entra en su Trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MinigameStand : MonoBehaviour
    {
        [Header("Configuración del Stand")]
#if UNITY_EDITOR
        [Tooltip("Arrastra aquí el archivo de la escena")]
        public UnityEditor.SceneAsset minigameSceneFile;
#endif
        [Tooltip("Nombre de la escena (se rellena automáticamente)")]
        public string minigameSceneName;

        [Tooltip("La máscara que se gana en este minijuego")]
        public CollectibleData associatedMask;

        [Header("Estado Visual")]
        [Tooltip("Objeto visual que indica que ya completaste este nivel")]
        public GameObject completedIndicator;

        [Header("Trigger Settings")]
        [Tooltip("Evita múltiples cargas si el jugador entra varias veces")]
        public bool singleUse = true;

        private bool _hasEntered;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (minigameSceneFile != null)
                minigameSceneName = minigameSceneFile.name;
#endif
        }

        private void Start()
        {
            CheckCompletionStatus();

            // Asegurar que el collider sea Trigger
            Collider col = GetComponent<Collider>();
            if (col && !col.isTrigger)
                col.isTrigger = true;
        }

        private void CheckCompletionStatus()
        {
            if (associatedMask != null && InventorySystem.Instance != null)
            {
                bool isCompleted = InventorySystem.Instance.HasItem(associatedMask);
                if (completedIndicator != null)
                    completedIndicator.SetActive(isCompleted);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (singleUse && _hasEntered) return;

            if (!other.CompareTag(GameConstants.PLAYER_TAG))
                return;

            _hasEntered = true;
            EnterMinigame();
        }

        public void EnterMinigame()
        {
            if (string.IsNullOrEmpty(minigameSceneName))
            {
                Debug.LogError($"[MinigameStand] No se ha asignado escena para {gameObject.name}");
                return;
            }

            SceneManager.LoadScene(minigameSceneName);
        }
    }
}
