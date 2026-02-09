using System.Collections;
using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckSpawner : MonoBehaviour
    {
        [Header("Referencias Prefabs")]
        [Tooltip("Prefab para Pato (Horizontal)")]
        [SerializeField] private GameObject duckPrefab;
        [Tooltip("Prefab para Globo (Vertical)")]
        [SerializeField] private GameObject balloonPrefab;
        [Tooltip("Prefab para Ave (ZigZag)")]
        [SerializeField] private GameObject birdPrefab;

        [SerializeField] private DuckHunterManager manager;

        [Header("Configuración Pato (Duck) - Area Unificada")]
        [Tooltip("Area donde aparecen y se mueven los patos (Spawn en extremos X, rebote en extremos X)")]
        [SerializeField] private Vector2 duckAreaXRange = new(-23f, 23f);
        [SerializeField] private Vector2 duckAreaYRange = new(-12f, -2f);
        [SerializeField] private Vector2 duckAreaZRange = new(37f, 60f);

        [Header("Configuración Ave (Bird) - Area Unificada")]
        [Tooltip("Area donde aparecen y se mueven las aves (Spawn en extremos X, rebote en extremos X)")]
        [SerializeField] private Vector2 birdAreaXRange = new(-23f, 23f);
        [SerializeField] private Vector2 birdAreaYRange = new(2f, 13f);
        [SerializeField] private Vector2 birdAreaZRange = new(37f, 60f);

        [Header("Configuración Globo (Balloon) - Area Unificada")]
        [Tooltip("Area donde aparecen y se mueven los globos (Spawn dentro, rebote en extremos Y)")]
        [SerializeField] private Vector2 balloonAreaXRange = new(-43f, 43f);
        [SerializeField] private Vector2 balloonAreaYRange = new(-30f, 2f); // Rango completo de movimiento vertical
        [SerializeField] private Vector2 balloonAreaZRange = new(45f, 65f);

        [Header("Velocidad Global")]
        [SerializeField] private Vector2 speedRange = new(3f, 6f);

        public void SpawnWave(int duckCount, int balloonCount, int birdCount,
                              EnemyType realType, EnemyType decoyType, EnemyType neutralType, float rate)
        {
            StartCoroutine(SpawnRoutine(duckCount, balloonCount, birdCount, realType, decoyType, neutralType, rate));
        }

        private IEnumerator SpawnRoutine(int duckCount, int balloonCount, int birdCount,
                                         EnemyType realType, EnemyType decoyType, EnemyType neutralType, float rate)
        {
            // 1. Crear lista de objetivos
            System.Collections.Generic.List<EnemyType> waveComposition = new();

            for (int i = 0; i < duckCount; i++) waveComposition.Add(EnemyType.Duck);
            for (int i = 0; i < balloonCount; i++) waveComposition.Add(EnemyType.Balloon);
            for (int i = 0; i < birdCount; i++) waveComposition.Add(EnemyType.Bird);

            // 2. Shuffle (Fisher-Yates)
            for (int i = 0; i < waveComposition.Count; i++)
            {
                EnemyType temp = waveComposition[i];
                int randomIndex = Random.Range(i, waveComposition.Count);
                waveComposition[i] = waveComposition[randomIndex];
                waveComposition[randomIndex] = temp;
            }

#if UNITY_EDITOR
            Debug.Log($"[DuckSpawner] Starting Wave. Total: {waveComposition.Count}");
#endif

            // 3. Spawn Loop
            foreach (EnemyType typeToSpawn in waveComposition)
            {
                GameObject prefabToSpawn = GetPrefabForType(typeToSpawn);
                if (prefabToSpawn == null) continue;

                if (!prefabToSpawn.TryGetComponent(out DuckTarget targetScript)) continue;

                // Spawn según patrón del Prefab con CONFIGURACIÓN UNIFICADA
                switch (targetScript.Pattern)
                {
                    case MovementPattern.Vertical: // GLOBOS
                        // Usamos balloonAreaYRange como límites de movimiento Y
                        SpawnVerticalTarget(prefabToSpawn, typeToSpawn, balloonAreaXRange, balloonAreaYRange, balloonAreaZRange);
                        break;
                    case MovementPattern.ZigZag: // AVES (BIRD)
                        // Usamos birdAreaXRange como límites de movimiento X
                        SpawnLinearTarget(prefabToSpawn, typeToSpawn, birdAreaXRange, birdAreaYRange, birdAreaZRange);
                        break;
                    default: // LINEAR (DUCKS)
                        // Usamos duckAreaXRange como límites de movimiento X
                        SpawnLinearTarget(prefabToSpawn, typeToSpawn, duckAreaXRange, duckAreaYRange, duckAreaZRange);
                        break;
                }

                // yield return new WaitForSeconds(rate); // DESACTIVADO: Todos aparecen al mismo tiempo
            }

#if UNITY_EDITOR
            Debug.Log("[DuckSpawner] Wave Spawning Complete.");
#endif
            yield return null; // Necesario para que la corrutina termine correctamente
        }

        private void SpawnLinearTarget(GameObject prefab, EnemyType type, Vector2 xRange, Vector2 yRange, Vector2 zRange)
        {
            // Spawn en los extremos del área X (izquierda o derecha)
            // Lado izquierdo = xRange.x, Lado derecho = xRange.y
            float xPos = (Random.value > 0.5f) ? xRange.y : xRange.x;

            float yPos = Random.Range(yRange.x, yRange.y);
            float zPos = Random.Range(zRange.x, zRange.y);

            Vector3 spawnPos = new(xPos, yPos, zPos);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            if (obj.TryGetComponent(out DuckTarget target))
            {
                float speed = Random.Range(speedRange.x, speedRange.y);
                // Pasar límites X: Min = xRange.x, Max = xRange.y
                target.Initialize(type, speed, manager, xRange.x, xRange.y);
            }
        }

        private void SpawnVerticalTarget(GameObject prefab, EnemyType type, Vector2 xRange, Vector2 yRange, Vector2 zRange)
        {
            // Spawn dentro del área X (aleatorio)
            float xPos = Random.Range(xRange.x, xRange.y);
            // Spawn dentro del área Y (aleatorio), pero suele ser mejor abajo si suben
            // Aqui usamos random en todo el rango vertical de movimiento
            float yPos = Random.Range(yRange.x, yRange.y);
            float zPos = Random.Range(zRange.x, zRange.y);

            Vector3 spawnPos = new(xPos, yPos, zPos);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            if (obj.TryGetComponent(out DuckTarget target))
            {
                float speed = Random.Range(speedRange.x, speedRange.y) * 0.5f;
                // Pasar límites Y: Min = yRange.x, Max = yRange.y
                target.Initialize(type, speed, manager, yRange.x, yRange.y);
            }
        }

        // Gizmos para visualizar zonas de spawn en el editor
        private void OnDrawGizmos()
        {
            // --- 1. PATOS (GREEN) ---
            Gizmos.color = Color.green;
            DrawAreaBox(duckAreaXRange, duckAreaYRange, duckAreaZRange);

            // --- 2. AVES (BLUE) ---
            Gizmos.color = Color.blue;
            DrawAreaBox(birdAreaXRange, birdAreaYRange, birdAreaZRange);

            // --- 3. GLOBOS (RED) ---
            Gizmos.color = Color.red;
            DrawAreaBox(balloonAreaXRange, balloonAreaYRange, balloonAreaZRange);
        }

        private void DrawAreaBox(Vector2 xRange, Vector2 yRange, Vector2 zRange)
        {
            float centerX = (xRange.x + xRange.y) / 2f;
            float widthX = xRange.y - xRange.x;

            float centerY = (yRange.x + yRange.y) / 2f;
            float heightY = yRange.y - yRange.x;

            float centerZ = (zRange.x + zRange.y) / 2f;
            float depthZ = zRange.y - zRange.x;

            Gizmos.DrawWireCube(new Vector3(centerX, centerY, centerZ), new Vector3(widthX, heightY, depthZ));
        }

        private GameObject GetPrefabForType(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Duck: return duckPrefab;
                case EnemyType.Balloon: return balloonPrefab;
                case EnemyType.Bird: return birdPrefab;
                default:
#if UNITY_EDITOR
                    Debug.LogWarning($"[DuckSpawner] Unknown Type: {type}");
#endif
                    return null;
            }
        }
    }
}
