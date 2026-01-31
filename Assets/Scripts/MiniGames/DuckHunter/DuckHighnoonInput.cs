using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHighnoonInput : MonoBehaviour
    {
        [Header("Configuración Input System")]
        [Tooltip("Arrastra aquí la referencia a la acción de Disparo (Click Izquierdo)")]
        public InputActionReference shootAction;

        [Header("Referencias")]
        public Camera mainCamera;
        [Tooltip("LayerMask para solo pegar a los objetivos")]
        public LayerMask targetLayer;

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

            // Raycast al centro de la pantalla si es FPS estático,
            // o a la posición del mouse si es estilo shooter 2D.
            // El usuario pidió "FPS Estático y click izquierdo". 
            // Asumimos cursor bloqueado al centro? O puntero libre?
            // "FPS estático... player fijo en un punto, se puede mover es la mira" 
            // -> Esto suena a que mueves la cámara con el mouse. El disparo va al centro.
            
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            // Si "se mueve la mira" significa cursor libre en pantalla fija, sería ScreenPointToRay(Mouse.position).
            // Ante la duda en FPS, el centro es lo estándar.
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
            {
                DuckTarget target = hit.collider.GetComponent<DuckTarget>();
                if (target != null)
                {
                    target.OnHit();
                }
            }
        }
    }
}
