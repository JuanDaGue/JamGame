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
        private bool initialized = false;
        private readonly WaitForSeconds flashDuration = new(0.05f);

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
        }

        private void Update()
        {
            if (!initialized) return;

            // 1. Movimiento Horizontal (Incremental)
            transform.Translate(speed * Time.deltaTime * moveDirection, Space.World);

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
            StartCoroutine(DieSequence());
        }

        private System.Collections.IEnumerator DieSequence()
        {
            // 1. Notificar al Manager (para Score y VFX)
            if (manager != null)
            {
                manager.RegisterHit(color, transform.position);
            }

            // 2. Flash visual (Blanco)
            if (TryGetComponent(out Renderer r))
            {
                // Opción A: Cambiar color a blanco puro
                // Opción B: Si tienes un shader con "Emission", subirla a tope.
                // Usaremos Color blanco para standard shader.
                r.material.color = Color.white;
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", Color.white * 2f); // HDR Intensity
            }

            // 3. Esperar un frame o brevísimo tiempo par que se vea el flash
            yield return flashDuration;

            Destroy(gameObject);
        }
    }
}
