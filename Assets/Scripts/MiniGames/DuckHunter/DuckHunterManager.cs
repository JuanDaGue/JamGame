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
        [SerializeField] private DuckHunterVFX vfxController;

        [Header("Configuración de Oleadas")]
        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int targetsPerColor = 5; // Cantidad de CADA tipo (Real, Decoy, Neutral)
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
            ui.UpdateWave(currentWave, totalWaves);

            // 3. Calcular umbrales 
            // - Derrota: Si matas a la MAYORÍA de los Decoys/Neutrals.
            //   Como spawnearán 'targetsPerColor' de cada uno, usaremos ese valor base.
            wrongHitsThreshold = (targetsPerColor / 2) + 1;

            wrongTargetsEliminated = 0;
            realTargetsEliminated = 0;

            // 4. Iniciar Spawner con los colores asignados Y cantidades explícitas
            // User requested: "cantidad de enemigos de cada color sea el mismo numero de la condicion de victoria"
            // Pasamos targetsPerColor para CADA uno de los 3 tipos.
            spawner.SpawnWave(targetsPerColor, targetsPerColor, targetsPerColor,
                              currentRealColor, currentDecoyColor, currentNeutralColor, spawnRate);

            Debug.Log($"[DuckHunter] Oleada {currentWave}: Roles asignados. Real={currentRealColor}, Decoy={currentDecoyColor}, Neutral={currentNeutralColor}. Spawning {targetsPerColor} of each.");
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

        public void RegisterHit(TargetColor hitColor, Vector3 hitPosition)
        {
            if (!isGameActive) return;

            // Determinar tipo para VFX
            TargetType typeHit = TargetType.Neutral;
            if (hitColor == currentRealColor) typeHit = TargetType.Real;
            else if (hitColor == currentDecoyColor) typeHit = TargetType.Decoy;

            if (vfxController != null)
            {
                vfxController.PlayHitVFX(hitPosition, typeHit);
            }

            if (hitColor == currentRealColor)
            {
                realTargetsEliminated++;
                ui.UpdateScore(realTargetsEliminated);

                // Condición de victoria de oleada
                if (realTargetsEliminated >= targetsPerColor)
                {
                    Debug.Log($"[DuckHunter] Wave {currentWave} Complete! Spawning next wave...");

                    // Esperar un momento antes de la siguiente oleada para dar feedback
                    StartCoroutine(WaitAndNextWave());
                }
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

        private System.Collections.IEnumerator WaitAndNextWave()
        {
            // Pausa breve para celebrar
            yield return new WaitForSeconds(1.5f);
            StartNextWave();
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
