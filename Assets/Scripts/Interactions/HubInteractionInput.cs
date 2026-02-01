using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Interactions
{
    /// <summary>
    /// Interacción del Hub (Main) usando New Input System.
    /// Hace un Raycast desde la cámara hacia la posición del puntero y, si golpea un Stand,
    /// llama EnterMinigame() en el MinigameStand.
    /// </summary>
    public class HubInteractionInput : MonoBehaviour
    {
        [Header("Configuración Input")]
        [Tooltip("Referencia a la acción de Interactuar/Click")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Configuración Raycast")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask interactionLayer = ~0;

        [Tooltip("Distancia máxima del raycast.")]
        [SerializeField] private float maxDistance = 100f;

        [Header("Fallback (sin puntero)")]
        [Tooltip("Si no hay Pointer.current (ej. gamepad), usa el centro de pantalla.")]
        [SerializeField] private bool useScreenCenterIfNoPointer = true;

        private void OnEnable()
        {
            if (interactAction == null) return;

            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }

        private void OnDisable()
        {
            if (interactAction == null) return;

            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector2 screenPos;

            // Preferimos puntero (mouse/touch). Si no hay, usamos centro de pantalla.
            if (Pointer.current != null)
            {
                screenPos = Pointer.current.position.ReadValue();
            }
            else
            {
                if (!useScreenCenterIfNoPointer) return;
                screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayer, QueryTriggerInteraction.Ignore))
            {
                // IMPORTANTE: robusto si el Collider está en un hijo del Stand.
                MinigameStand stand = hit.collider.GetComponentInParent<MinigameStand>();
                if (stand != null)
                {
                    stand.EnterMinigame();
                }
            }
        }
    }
}
