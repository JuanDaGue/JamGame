using UnityEngine;
using System.Collections;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckSpawner : MonoBehaviour
    {
        [Header("Referencias")]
        public GameObject targetPrefab; // Usar una primitiva base por ahora
        public DuckHunterManager manager;

        [Header("Configuración de Spawn")]
        public float minSpawnY = -2f;
        public float maxSpawnY = 4f;
        public float spawnX = 12f; // Qué tan a la derecha/izquierda spawnean

        public void SpawnWave(int count, TargetType realType, TargetType decoyType, TargetType neutralType, float spawnRate)
        {
            StartCoroutine(SpawnRoutine(count, realType, decoyType, neutralType, spawnRate));
        }

        private IEnumerator SpawnRoutine(int count, TargetType real, TargetType decoy, TargetType neutral, float rate)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnSingleTarget(real, decoy, neutral);
                yield return new WaitForSeconds(rate);
            }
        }

        private void SpawnSingleTarget(TargetType real, TargetType decoy, TargetType neutral)
        {
            // Decidir aleatoriamente qué tipo spawnea en este instante
            // Peso: 33% cada uno aproximadamente
            TargetType typeToSpawn;
            float rand = Random.value;
            if (rand < 0.33f) typeToSpawn = real;
            else if (rand < 0.66f) typeToSpawn = decoy;
            else typeToSpawn = neutral;

            // Decidir lado (Izquierda o Derecha)
            bool startLeft = Random.value > 0.5f;
            float startX = startLeft ? -spawnX : spawnX;

            Vector3 spawnPos = new Vector3(startX, Random.Range(minSpawnY, maxSpawnY), 0);
            // Ajustar profundidad aleatoria
            spawnPos.z = Random.Range(5f, 15f);

            GameObject newObj = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
            
            // Configurar script
            DuckTarget targetScript = newObj.GetComponent<DuckTarget>();
            if (targetScript == null) targetScript = newObj.AddComponent<DuckTarget>();

            MovementPattern pattern = Random.value > 0.5f ? MovementPattern.Linear : MovementPattern.ZigZag;
            // Velocidad variable según profundidad (más lejos = más lento visualmente, o ajustar para parajeje)
            float speed = Random.Range(3f, 6f); 

            targetScript.Initialize(typeToSpawn, pattern, speed, manager);
        }
    }
}
