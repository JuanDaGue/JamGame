using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    // Los colores visuales de los objetivos (fijos)
    public enum EnemyType
    {
        Duck,       // Horizontal
        Balloon,    // Vertical
        Bird        // ZigZag
    }

    public enum TargetType
    {
        Real,       // Verde - El que REALMENTE debes disparar (suma puntos)
        Decoy,      // Rojo - La TRAMPA (la UI mentirosa dice que dispares a este)
        Neutral     // Azul - Neutral (no disparar, cuenta como error)
    }

    public enum MovementPattern
    {
        Linear,
        ZigZag,
        Vertical
    }

    [RequireComponent(typeof(Renderer))]
    public class DuckTarget : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private EnemyType type; // Tipo de enemigo (Duck, Balloon, Bird)
        [SerializeField] private MovementPattern _movementPattern;
        public MovementPattern Pattern => _movementPattern;
        [SerializeField] private bool flipVisuals = true; // Si debe rotar al cambiar de dirección
        [SerializeField] private float speed = 5f;
        [SerializeField] private float amplitude = 2f; // Para ZigZag
        [SerializeField] private float frequency = 2f; // Para ZigZag

        // Límites de Movimiento (Asignados por Spawner)
        private float minBound;
        private float maxBound;

        private Vector3 moveDirection;
        private float baseHeight; // Para ZigZag (altura central de oscilación)
        private DuckHunterManager manager;
        private bool initialized = false;
        private readonly WaitForSeconds flashDuration = new(0.05f);
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void Initialize(EnemyType enemyType, float moveSpeed, DuckHunterManager gameManager, float minLimit, float maxLimit)
        {
            // Nota: movementPattern se toma del Inspector del Prefab
            type = enemyType;
            speed = moveSpeed;
            manager = gameManager;
            minBound = minLimit;
            maxBound = maxLimit;

            // Dirección inicial
            if (Pattern == MovementPattern.Vertical)
            {
                // Moverse en Y (Arriba o Abajo aleatorio)
                moveDirection = (Random.value > 0.5f) ? Vector3.up : Vector3.down;
            }
            else
            {
                // Moverse en X (hacia el centro)
                moveDirection = (transform.position.x > 0) ? Vector3.left : Vector3.right;
            }

            baseHeight = transform.position.y;
            initialized = true;

            UpdateFacingDirection(); // Orientar al inicio

#if UNITY_EDITOR
            Debug.Log($"[DuckTarget] Init. Pattern: {Pattern}, Speed: {speed}, Dir: {moveDirection}");
#endif
        }

        private void Update()
        {
            if (!initialized) return;

            // Movimiento diferenciado
            if (Pattern == MovementPattern.Vertical)
            {
                // Movimiento Vertical (Balloon)
                transform.Translate(speed * Time.deltaTime * moveDirection, Space.World);

                // Rebote Vertical (Topes configurables)
                if (transform.position.y > maxBound && moveDirection.y > 0)
                {
                    moveDirection = Vector3.down;
                }
                else if (transform.position.y < minBound && moveDirection.y < 0)
                {
                    moveDirection = Vector3.up;
                }
            }
            else
            {
                // Movimiento Horizontal Base (Linear y ZigZag)
                transform.Translate((speed * Time.deltaTime * moveDirection.x) * Vector3.right, Space.World);

                // Rebote Horizontal
                if (transform.position.x > maxBound && moveDirection.x > 0)
                {
                    moveDirection.x = -1f;
                    if (flipVisuals) UpdateFacingDirection();
                }
                else if (transform.position.x < minBound && moveDirection.x < 0)
                {
                    moveDirection.x = 1f;
                    if (flipVisuals) UpdateFacingDirection();
                }

                // Extra para ZigZag
                if (Pattern == MovementPattern.ZigZag)
                {
                    float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
                    Vector3 pos = transform.position;
                    // Mantener Y relativa si cayera, pero por ahora baseHeight
                    pos.y = baseHeight + yOffset;
                    transform.position = pos;
                }
            }
        }

        private void UpdateFacingDirection()
        {
            if (!flipVisuals) return;

            // Asumiendo que el modelo mira a la derecha por defecto (X positive)
            // Si vamos a la izquierda, rotar 180 en Y.
            if (moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); // Mirar Izquierda
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0); // Mirar Derecha
            }
        }

        public void OnHit()
        {
            StartCoroutine(DieSequence());
        }

        private System.Collections.IEnumerator DieSequence()
        {
            // 1. Notificar al Manager (para Score y VFX)
            if (manager != null)
            {
                if (manager != null)
                {
                    manager.RegisterHit(type, transform.position);
                }
            }

            // 2. Flash visual (Blanco)
            if (_renderer != null)
            {
                // Usaremos Color blanco standard / Emission
                _renderer.material.color = Color.white;
                _renderer.material.EnableKeyword("_EMISSION");
                _renderer.material.SetColor("_EmissionColor", Color.white * 2f); // HDR Intensity
            }

            // 3. Esperar un frame o brevísimo tiempo par que se vea el flash
            yield return flashDuration;

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // Visualizar límites actuales
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;

            if (Pattern == MovementPattern.Vertical)
            {
                // Dibujar línea vertical de límites
                Gizmos.DrawLine(new Vector3(center.x, minBound, center.z), new Vector3(center.x, maxBound, center.z));
                Gizmos.DrawSphere(new Vector3(center.x, minBound, center.z), 0.2f);
                Gizmos.DrawSphere(new Vector3(center.x, maxBound, center.z), 0.2f);
            }
            else
            {
                // Dibujar línea horizontal de límites
                Gizmos.DrawLine(new Vector3(minBound, center.y, center.z), new Vector3(maxBound, center.y, center.z));
                Gizmos.DrawSphere(new Vector3(minBound, center.y, center.z), 0.2f);
                Gizmos.DrawSphere(new Vector3(maxBound, center.y, center.z), 0.2f);
            }
        }
    }
}
