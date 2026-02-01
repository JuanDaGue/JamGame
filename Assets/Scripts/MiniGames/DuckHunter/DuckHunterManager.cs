using GameJam.Core;
using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHunterManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private DuckSpawner spawner;
        [SerializeField] private DuckHunterUI ui;
        [SerializeField] private MinigameController controller;

        [Header("Configuración de Oleadas")]
        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int targetsPerWave = 10;
        [SerializeField] private float spawnRate = 1.5f;

        // Estado del Juego
        private int currentWave = 0;
        private int realTargetsEliminated = 0;
        private int wrongTargetsEliminated = 0;

        // Mapeo dinámico de Color -> Rol (cambia cada oleada)
        private TargetColor currentRealColor;      // El color que REALMENTE suma puntos
        private TargetColor currentDecoyColor;     // El color que la UI MIENTE que debes disparar
        private TargetColor currentNeutralColor;   // El color neutral (error si disparas)

        private int wrongHitsThreshold = 3;

        private bool isGameActive = false;

        private void Start()
        {
            if (controller == null)
            {
                // Intentar buscarlo si no está asignado
                controller = FindFirstObjectByType<MinigameController>();
            }
            isGameActive = true;
            StartNextWave();
        }

        private void StartNextWave()
        {
            if (!isGameActive) return;

            currentWave++;
            if (currentWave > totalWaves)
            {
                // Victoria Final
                if (controller != null) controller.WinGame();
                isGameActive = false;
                return;
            }

            // 1. Asignar roles aleatorios a los colores
            SetupWaveRules();

            // 2. Configurar UI para engañar al jugador
            // La UI muestra el color Decoy (que NO debe disparar)
            ui.SetInstruction(currentDecoyColor);

            // 3. Calcular umbral de derrota
            wrongHitsThreshold = (targetsPerWave / 2) + 1;
            wrongTargetsEliminated = 0;
            realTargetsEliminated = 0;

            // 4. Iniciar Spawner con los colores asignados
            spawner.SpawnWave(targetsPerWave, currentRealColor, currentDecoyColor, currentNeutralColor, spawnRate);

            Debug.Log($"[DuckHunter] Oleada {currentWave}: UI dice {currentDecoyColor}, Real es {currentRealColor}, Neutral es {currentNeutralColor}");
        }

        private void SetupWaveRules()
        {
            // Sistema de asignación aleatoria:
            // 1. Elegir un color al azar para ser el Real
            // 2. De los 2 restantes, elegir uno al azar para ser el Decoy
            // 3. El último es automáticamente Neutral

            TargetColor[] allColors = { TargetColor.Green, TargetColor.Red, TargetColor.Blue };

            // Paso 1: Elegir color Real (índice 0, 1 o 2)
            int realIndex = Random.Range(0, 3);
            currentRealColor = allColors[realIndex];

            // Paso 2: Obtener los 2 colores restantes
            TargetColor[] remainingColors = new TargetColor[2];
            int idx = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i != realIndex)
                {
                    remainingColors[idx] = allColors[i];
                    idx++;
                }
            }

            // Paso 3: De los restantes, elegir uno para Decoy
            int decoyIndex = Random.Range(0, 2);
            currentDecoyColor = remainingColors[decoyIndex];
            currentNeutralColor = remainingColors[1 - decoyIndex]; // El otro
        }

        public void RegisterHit(TargetColor hitColor)
        {
            if (!isGameActive) return;

            if (hitColor == currentRealColor)
            {
                realTargetsEliminated++;
                ui.UpdateScore(realTargetsEliminated);

                // Condición de victoria de oleada (ejemplo: 50% de los targets spawnearon?)
                // Ojo: targetsPerWave es cuantos spawnean. Si elimina todos los reals...
                // Por ahora no hay condición explícita de "Fin de oleada" salvo que se acabe el tiempo o spawns.
                // El spawner termina su corrutina, pero el manager no sabe cuándo termina la oleada.
                // Podríamos asumir que si se matan suficientes, se pasa?
                // Dejaremos esto simple por ahora, la oleada "termina" por tiempo o flujo externo, 
                // pero DuckSpawner lanza "Oleada completada". Falta conectar eso.
            }
            else
            {
                // Disparó a Decoy o Neutral
                wrongTargetsEliminated++;
                Debug.LogWarning($"[DuckHunter] Error! Disparaste a {hitColor}. Llevas {wrongTargetsEliminated}/{wrongHitsThreshold}");

                if (wrongTargetsEliminated >= wrongHitsThreshold)
                {
                    GameOver();
                }
            }
        }

        public void GameOver()
        {
            if (!isGameActive) return;
            isGameActive = false;

            Debug.LogError("[DuckHunter] GAME OVER - NPC TRAP ACTIVATED on you!");
            StopAllCoroutines(); // Detener spawner

            if (controller != null)
            {
                controller.LoseGame();
            }
            else
            {
                Debug.LogError("[DuckHunter] MinigameController no asignado! No se puede ejecutar LoseGame().");
            }
        }
    }
}
