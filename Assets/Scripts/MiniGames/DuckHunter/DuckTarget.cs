using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    public enum TargetType
    {
        Real,       // El que suma puntos (Oculto)
        Decoy,      // La trampa (El que la UI dice que dispares)
        Neutral     // Relleno (Cuenta como error si disparas)
    }

    public enum MovementPattern
    {
        Linear,
        ZigZag
    }

    public class DuckTarget : MonoBehaviour
    {
        [Header("Configuración")]
        public TargetType type;
        public MovementPattern movementPattern;
        public float speed = 5f;
        public float amplitude = 2f; // Para ZigZag
        public float frequency = 2f; // Para ZigZag

        private Vector3 startPosition;
        private float startTime;
        private DuckHunterManager manager;

        public void Initialize(TargetType targetType, MovementPattern pattern, float moveSpeed, DuckHunterManager gameManager)
        {
            type = targetType;
            movementPattern = pattern;
            speed = moveSpeed;
            manager = gameManager;

            startPosition = transform.position;
            startTime = Time.time;
            
            // Asignar color según tipo para debug/prototipo (hasta tener prefabs)
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                switch (type)
                {
                    case TargetType.Real: renderer.material.color = Color.green; break; // Real (Jugador no sabe)
                    case TargetType.Decoy: renderer.material.color = Color.red; break;  // Trampa (UI dice este)
                    case TargetType.Neutral: renderer.material.color = Color.gray; break;
                }
            }
        }

        private void Update()
        {
            float distance = speed * (Time.time - startTime);
            Vector3 currentPos = startPosition;

            // Movimiento básico de Izquierda a Derecha (asumiendo X+)
            // Si startPosition.x es positivo, asumimos que va a la izquierda (X-)
            float direction = (startPosition.x > 0) ? -1f : 1f;

            float xOffset = distance * direction;

            if (movementPattern == MovementPattern.Linear)
            {
                currentPos.x += xOffset;
            }
            else if (movementPattern == MovementPattern.ZigZag)
            {
                currentPos.x += xOffset;
                currentPos.y += Mathf.Sin(Time.time * frequency) * amplitude;
            }

            transform.position = currentPos;

            // Destruir si sale de pantalla (valores aproximados, ajustar según cámara)
            if (Mathf.Abs(transform.position.x) > 15f)
            {
                Destroy(gameObject);
                // Opcional: Notificar al manager que se escapó un objetivo
            }
        }

        public void OnHit()
        {
            if (manager != null)
            {
                manager.RegisterHit(type);
            }
            Destroy(gameObject);
        }
    }
}
