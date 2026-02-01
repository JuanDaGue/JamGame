using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    // Los colores visuales de los objetivos (fijos)
    public enum TargetColor
    {
        Green,
        Red,
        Blue
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
        ZigZag
    }

    [RequireComponent(typeof(Renderer))]
    public class DuckTarget : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private TargetColor color; // El color visual (fijo)
        [SerializeField] private MovementPattern movementPattern;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float amplitude = 2f; // Para ZigZag
        [SerializeField] private float frequency = 2f; // Para ZigZag

        private Vector3 moveDirection;
        private float baseHeight; // Para ZigZag (altura central de oscilación)
        private DuckHunterManager manager;
        private Renderer cachedRenderer; // Cache del Renderer
        private bool initialized = false;

        public void Initialize(TargetColor targetColor, MovementPattern pattern, float moveSpeed, DuckHunterManager gameManager)
        {
            color = targetColor;
            movementPattern = pattern;
            speed = moveSpeed;
            manager = gameManager;

            // Determinar dirección inicial basada en posición (ir hacia el centro)
            moveDirection = (transform.position.x > 0) ? Vector3.left : Vector3.right;

            baseHeight = transform.position.y;
            initialized = true;

            Debug.Log($"[DuckTarget] Initialized. Pattern: {movementPattern}, Speed: {speed}, Direction: {moveDirection}");

            // Asignar color visual (cacheamos el renderer en primera llamada)
            if (cachedRenderer == null)
                cachedRenderer = GetComponent<Renderer>();

            if (cachedRenderer != null)
            {
                cachedRenderer.material.color = targetColor switch
                {
                    TargetColor.Green => Color.green,
                    TargetColor.Red => Color.red,
                    TargetColor.Blue => Color.blue,
                    _ => Color.white
                };
            }
        }

        private void Update()
        {
            if (!initialized) return;

            // 1. Movimiento Horizontal (Incremental)
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            // 2. Rebote en bordes (Horizontal)
            if (transform.position.x > 15f && moveDirection.x > 0)
            {
                moveDirection = Vector3.left;
            }
            else if (transform.position.x < -15f && moveDirection.x < 0)
            {
                moveDirection = Vector3.right;
            }

            // 3. Patrón ZigZag (Oscilación en Y)
            if (movementPattern == MovementPattern.ZigZag)
            {
                float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
                Vector3 pos = transform.position;
                pos.y = baseHeight + yOffset;
                transform.position = pos;
            }
        }

        public void OnHit()
        {
            if (manager != null)
            {
                manager.RegisterHit(color);
            }
            Destroy(gameObject);
        }
    }
}
