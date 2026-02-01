using System.Collections;
using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckSpawner : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private DuckHunterManager manager;

        [Header("Configuración Linear (Laterales)")]
        [Tooltip("Distancia lateral donde spawnean los lineales (±X)")]
        [SerializeField] private float linearSpawnX = 12f;
        [Tooltip("Rango Y para lineales (2/3 inferiores)")]
        [SerializeField] private Vector2 linearSpawnYRange = new Vector2(-2f, 2f);
        [Tooltip("Rango Z (profundidad) para lineales")]
        [SerializeField] private Vector2 linearSpawnZRange = new Vector2(5f, 15f);

        [Header("Configuración Zig-Zag (Caída desde arriba)")]
        [Tooltip("Rango X donde pueden aparecer los zig-zag")]
        [SerializeField] private Vector2 zigZagSpawnXRange = new Vector2(-8f, 8f);
        [Tooltip("Altura Y desde donde caen los zig-zag")]
        [SerializeField] private float zigZagDropHeight = 10f;
        [Tooltip("Altura Y objetivo (tercio superior) donde quedan los zig-zag")]
        [SerializeField] private Vector2 zigZagTargetYRange = new Vector2(2f, 4f);
        [Tooltip("Rango Z (profundidad) para zig-zag")]
        [SerializeField] private Vector2 zigZagSpawnZRange = new Vector2(5f, 15f);
        [Tooltip("Duración de la animación de caída")]
        [SerializeField] private float dropDuration = 1f;
        [Tooltip("Ángulo inicial de rotación para simular paleta colgante")]
        [SerializeField] private float initialPendulumAngle = 90f;

        [Header("Velocidad")]
        [SerializeField] private Vector2 speedRange = new Vector2(3f, 6f);

        public void SpawnWave(int count, TargetColor realColor, TargetColor decoyColor, TargetColor neutralColor, float rate)
        {
            StartCoroutine(SpawnRoutine(count, realColor, decoyColor, neutralColor, rate));
        }

        private IEnumerator SpawnRoutine(int count, TargetColor realColor, TargetColor decoyColor, TargetColor neutralColor, float rate)
        {
            TargetColor[] allColors = { realColor, decoyColor, neutralColor };

            for (int i = 0; i < count; i++)
            {
                if (targetPrefab == null)
                {
                    Debug.LogError("[DuckSpawner] No hay prefab de objetivo.");
                    yield break;
                }

                // Elegir color aleatorio
                TargetColor randomColor = allColors[Random.Range(0, allColors.Length)];

                // Elegir patrón aleatorio
                MovementPattern pattern = (Random.value > 0.5f) ? MovementPattern.Linear : MovementPattern.ZigZag;

                // Spawn según el patrón
                if (pattern == MovementPattern.Linear)
                {
                    SpawnLinearTarget(randomColor);
                }
                else
                {
                    SpawnZigZagTarget(randomColor);
                }

                yield return new WaitForSeconds(rate);
            }

            Debug.Log("[DuckSpawner] Oleada completada.");
        }

        private void SpawnLinearTarget(TargetColor color)
        {
            // Spawn desde los laterales (izquierda o derecha)
            float side = (Random.value > 0.5f) ? 1f : -1f;
            float xPos = linearSpawnX * side;
            float yPos = Random.Range(linearSpawnYRange.x, linearSpawnYRange.y);
            float zPos = Random.Range(linearSpawnZRange.x, linearSpawnZRange.y);

            Vector3 spawnPos = new Vector3(xPos, yPos, zPos);

            GameObject obj = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
            DuckTarget target = obj.GetComponent<DuckTarget>();

            if (target != null)
            {
                float speed = Random.Range(speedRange.x, speedRange.y);
                target.Initialize(color, MovementPattern.Linear, speed, manager);
            }
        }

        private void SpawnZigZagTarget(TargetColor color)
        {
            // Posición X aleatoria dentro del rango
            float xPos = Random.Range(zigZagSpawnXRange.x, zigZagSpawnXRange.y);
            float zPos = Random.Range(zigZagSpawnZRange.x, zigZagSpawnZRange.y);

            // Altura Y objetivo (donde quedará después de caer)
            float targetY = Random.Range(zigZagTargetYRange.x, zigZagTargetYRange.y);

            // Spawn desde arriba (fuera de vista)
            Vector3 dropStart = new Vector3(xPos, zigZagDropHeight, zPos);
            Vector3 dropEnd = new Vector3(xPos, targetY, zPos);

            GameObject obj = Instantiate(targetPrefab, dropStart, Quaternion.identity);
            DuckTarget target = obj.GetComponent<DuckTarget>();

            if (target != null)
            {
                float speed = Random.Range(speedRange.x, speedRange.y);
                // Iniciar con animación de caída
                StartCoroutine(DropAnimation(obj, target, dropEnd, color, speed));
            }
        }

        private IEnumerator DropAnimation(GameObject obj, DuckTarget target, Vector3 finalPosition, TargetColor color, float speed)
        {
            // Rotación inicial tipo paleta colgante (eje X)
            obj.transform.rotation = Quaternion.Euler(initialPendulumAngle, 0, 0);

            Vector3 startPos = obj.transform.position;
            float elapsed = 0f;

            // Animación de caída con rotación tipo péndulo
            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dropDuration;

                // Lerp de posición
                obj.transform.position = Vector3.Lerp(startPos, finalPosition, t);

                // Rotación de péndulo (de 90° a 0° en eje X)
                float currentAngle = Mathf.Lerp(initialPendulumAngle, 0f, t);
                obj.transform.rotation = Quaternion.Euler(currentAngle, 0, 0);

                yield return null;
            }

            // Enderezar de golpe al final
            obj.transform.position = finalPosition;
            obj.transform.rotation = Quaternion.identity;

            // Ahora inicializar el movimiento normal del target
            target.Initialize(color, MovementPattern.ZigZag, speed, manager);
        }

        // Gizmos para visualizar zonas de spawn en el editor
        private void OnDrawGizmosSelected()
        {
            // Zona Linear (Laterales) - Color Cyan
            Gizmos.color = Color.cyan;

            // Lateral izquierdo
            Vector3 leftCenter = new Vector3(-linearSpawnX, (linearSpawnYRange.x + linearSpawnYRange.y) / 2f, (linearSpawnZRange.x + linearSpawnZRange.y) / 2f);
            Vector3 leftSize = new Vector3(0.5f, linearSpawnYRange.y - linearSpawnYRange.x, linearSpawnZRange.y - linearSpawnZRange.x);
            Gizmos.DrawWireCube(leftCenter, leftSize);

            // Lateral derecho
            Vector3 rightCenter = new Vector3(linearSpawnX, (linearSpawnYRange.x + linearSpawnYRange.y) / 2f, (linearSpawnZRange.x + linearSpawnZRange.y) / 2f);
            Gizmos.DrawWireCube(rightCenter, leftSize);

            // Zona Zig-Zag (Superior) - Color Yellow
            Gizmos.color = Color.yellow;

            // Zona de caída (arriba)
            Vector3 dropCenter = new Vector3((zigZagSpawnXRange.x + zigZagSpawnXRange.y) / 2f, zigZagDropHeight, (zigZagSpawnZRange.x + zigZagSpawnZRange.y) / 2f);
            Vector3 dropSize = new Vector3(zigZagSpawnXRange.y - zigZagSpawnXRange.x, 0.5f, zigZagSpawnZRange.y - zigZagSpawnZRange.x);
            Gizmos.DrawWireCube(dropCenter, dropSize);

            // Zona objetivo (donde aterrizan)
            Gizmos.color = Color.green;
            Vector3 targetCenter = new Vector3((zigZagSpawnXRange.x + zigZagSpawnXRange.y) / 2f, (zigZagTargetYRange.x + zigZagTargetYRange.y) / 2f, (zigZagSpawnZRange.x + zigZagSpawnZRange.y) / 2f);
            Vector3 targetSize = new Vector3(zigZagSpawnXRange.y - zigZagSpawnXRange.x, zigZagTargetYRange.y - zigZagTargetYRange.x, zigZagSpawnZRange.y - zigZagSpawnZRange.x);
            Gizmos.DrawWireCube(targetCenter, targetSize);

            // Líneas conectando zona de caída con zona objetivo
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(zigZagSpawnXRange.x, zigZagDropHeight, zigZagSpawnZRange.x),
                            new Vector3(zigZagSpawnXRange.x, zigZagTargetYRange.y, zigZagSpawnZRange.x));
            Gizmos.DrawLine(new Vector3(zigZagSpawnXRange.y, zigZagDropHeight, zigZagSpawnZRange.y),
                            new Vector3(zigZagSpawnXRange.y, zigZagTargetYRange.y, zigZagSpawnZRange.y));
        }
    }
}
