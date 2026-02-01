using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Interactions
{
    /// <summary>
    /// Maneja la interacción en el Hub usando el New Input System.
    /// Reemplaza el uso de OnMouseDown.
    /// </summary>
    public class HubInteractionInput : MonoBehaviour
    {
        [Header("Configuración Input")]
        [Tooltip("Referencia a la acción de Interactuar/Click")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Configuración Raycast")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask interactionLayer;

        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.action.Enable();
                interactAction.action.performed += OnInteract;
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.action.performed -= OnInteract;
                interactAction.action.Disable();
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Usamos la posición del mouse si es una interacción de puntero
            // O el centro de la pantalla si es un FPS controller bloqueado.
            // Para un menú/Hub tipo feria, suele ser mouse visible.

            // Vector2 mousePos = Mouse.current.position.ReadValue(); 
            // Pero para ser genéricos con el Input System, mejor leer del puntero actual o asumir centro si no hay puntero.
            // Simplificación: Asumimos Click de Mouse.

            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(pointerPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayer))
            {
                MinigameStand stand = hit.collider.GetComponent<MinigameStand>();
                if (stand != null)
                {
                    stand.EnterMinigame();
                }
            }
        }
    }
}
