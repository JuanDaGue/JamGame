using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHighnoonInput : MonoBehaviour
    {
        [Header("Configuración Input System")]
        [Tooltip("Referencia a la acción de Disparo")]
        [SerializeField] private InputActionReference shootAction;

        [Header("Referencias")]
        [SerializeField] private Camera mainCamera;
        [Tooltip("LayerMask para objetivos")]
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

            // Raycast desde mouse
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow, 1f);
#endif

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
            {
                // Buscamos el componente en el objeto golpeado o sus padres
                DuckTarget target = hit.collider.GetComponentInParent<DuckTarget>();

                if (target != null)
                {
#if UNITY_EDITOR
                    Debug.Log($"[DuckInput] Hit target: {target.name}");
#endif
                    target.OnHit();
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("[DuckInput] Missed shot.");
#endif
            }
        }
    }
}
