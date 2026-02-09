using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.MiniGames.DuckHunter
{
    /// <summary>
    /// Se encarga de que el arma del jugador apunte hacia la posición del mouse en el mundo 3D.
    /// </summary>
    public class PlayerGunAim : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Punto de referencia del cañón que debe apuntar al objetivo")]
        [SerializeField] private Transform muzzlePoint;
        [Tooltip("Cámara principal para el Raycast")]
        [SerializeField] private Camera mainCamera;

        [Header("Configuración")]
        [Tooltip("Suavizado de la rotación (0 = instantáneo)")]
        [SerializeField] private float smoothSpeed = 15f;
        [Tooltip("Distancia base al plano de apuntado si no golpea nada")]
        [SerializeField] private float defaultDistance = 10f;

        private bool isAimingEnabled = false;

        private void Start()
        {
            if (mainCamera == null)
            {
                // Intentar obtener la cámara principal de forma robusta (compatible con Cinemachine 3.x)
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj != null)
                    mainCamera = camObj.GetComponent<Camera>();

                if (mainCamera == null) mainCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (!isAimingEnabled || muzzlePoint == null || mainCamera == null) return;

            UpdateAim();
        }

        private void UpdateAim()
        {
            // 1. Obtener posición del mouse
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 2. Crear un rayo desde la cámara
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // 3. Determinar el punto objetivo en el mundo
            // Preferimos apuntar a un plano paralelo a la cámara a una distancia fija
            // para evitar que el arma se comporte raro con profundidad variable.
            Vector3 targetPoint = ray.GetPoint(defaultDistance);

            // 4. Calcular la dirección desde el pivot (este transform) hacia el objetivo
            Vector3 directionToTarget = (targetPoint - transform.position).normalized;

            if (directionToTarget != Vector3.zero)
            {
                // Queremos que el cañón (muzzlePoint) mire al objetivo.
                // Como este script está en el root del arma, rotamos el root.
                // Usamos LookRotation pero compensamos si el muzzlePoint no está alineado con el forward local

                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                // Aplicar offset fijo de -90 en Y para corregir la orientación del modelo
                targetRotation *= Quaternion.Euler(0, -90, 0);

                // Aplicar suavizado
                if (smoothSpeed > 0)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }

        public void SetAimEnabled(bool enabled)
        {
            isAimingEnabled = enabled;

#if UNITY_EDITOR
            Debug.Log($"[PlayerGunAim] Aim enabled: {enabled}");
#endif
        }
    }
}
