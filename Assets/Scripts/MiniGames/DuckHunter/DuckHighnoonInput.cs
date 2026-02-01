using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHighnoonInput : MonoBehaviour
    {
        [Header("Configuración Input System")]
        [Tooltip("Arrastra aquí la referencia a la acción de Disparo (Click Izquierdo)")]
        [SerializeField] private InputActionReference shootAction;

        [Header("Referencias")]
        [SerializeField] private Camera mainCamera;
        [Tooltip("LayerMask para solo pegar a los objetivos")]
        [SerializeField] private LayerMask targetLayer;

        private void OnEnable()
        {
            if (shootAction != null)
            {
                shootAction.action.Enable();
                shootAction.action.performed += OnShoot;
            }
        }

        private void OnDisable()
        {
            if (shootAction != null)
            {
                shootAction.action.performed -= OnShoot;
                shootAction.action.Disable();
            }
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Usamos la posición del mouse para el raycast (Point & Click clásico)
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // Debug para ver el rayo en la Scene View
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow, 1f);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
            {
                if (hit.collider.TryGetComponent<DuckTarget>(out var target))
                {
                    Debug.Log($"[DuckInput] Hit target: {target.name}");
                    target.OnHit();
                }
            }
            else
            {
                // Feedback visual si fallamos
                Debug.Log("[DuckInput] Missed shot.");
            }
        }
    }
}
