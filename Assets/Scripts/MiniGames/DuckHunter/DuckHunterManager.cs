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
        [SerializeField] private GameOverGunSequence gunSequence;
        [SerializeField] private PlayerGunAim playerGunAim;

        [Header("Configuración de Oleadas")]
        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int targetsPerColor = 5; // Cantidad de CADA tipo (Real, Decoy, Neutral)
        [SerializeField] private float spawnRate = 1.5f;

        // Estado del Juego
        private int currentWave = 0;
        private int realTargetsEliminated = 0;
        private int wrongTargetsEliminated = 0;
        private readonly WaitForSeconds waveEndDelay = new(1.5f); // Cached delay

        // Mapeo dinámico de Roles
        private EnemyType currentRealType;      // Suma puntos
        private EnemyType currentDecoyType;     // Trampa/Engaño
        private EnemyType currentNeutralType;   // Error/Neutral

        private int wrongHitsThreshold = 3;
        private readonly EnemyType[] allTypes = { EnemyType.Duck, EnemyType.Balloon, EnemyType.Bird };
        private readonly EnemyType[] remainingTypes = new EnemyType[2];

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
            ui.SetInstruction(currentDecoyType);
            ui.UpdateWave(currentWave, totalWaves);

            // 3. Calcular umbrales 
            wrongHitsThreshold = (targetsPerColor / 2) + 1;

            wrongTargetsEliminated = 0;
            realTargetsEliminated = 0;

            // 4. Iniciar Spawner
            spawner.SpawnWave(targetsPerColor, targetsPerColor, targetsPerColor,
                              currentRealType, currentDecoyType, currentNeutralType, spawnRate);

            // 5. Habilitar el apuntado del arma
            if (playerGunAim != null) playerGunAim.SetAimEnabled(true);

#if UNITY_EDITOR
            Debug.Log($"[DuckHunter] Wave {currentWave}: Real={currentRealType}, Decoy={currentDecoyType}, Neutral={currentNeutralType}");
#endif
        }

        private void SetupWaveRules()
        {
            // Sistema de asignación aleatoria:
            // 1. Elegir un tipo al azar para ser el Real
            // 2. De los 2 restantes, elegir uno al azar para ser el Decoy (Trampa)
            // 3. El último es Neutral

            // Paso 1: Elegir Real
            int realIndex = Random.Range(0, 3);
            currentRealType = allTypes[realIndex];

            // Paso 2: Obtener los 2 restantes
            int idx = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i != realIndex)
                {
                    remainingTypes[idx] = allTypes[i];
                    idx++;
                }
            }

            // Paso 3: Elegir Decoy
            int decoyIndex = Random.Range(0, 2);
            currentDecoyType = remainingTypes[decoyIndex];
            currentNeutralType = remainingTypes[1 - decoyIndex];
        }

        public void RegisterHit(EnemyType hitType, Vector3 hitPosition)
        {
            if (!isGameActive) return;

            // Determinar tipo para VFX
            TargetType typeHit = TargetType.Neutral;
            if (hitType == currentRealType) typeHit = TargetType.Real;
            else if (hitType == currentDecoyType) typeHit = TargetType.Decoy;

            if (vfxController != null)
            {
                vfxController.PlayHitVFX(hitPosition, typeHit);
            }

            if (hitType == currentRealType)
            {
                realTargetsEliminated++;
                ui.UpdateScore();

                // Condición de victoria de oleada
                if (realTargetsEliminated >= targetsPerColor)
                {
#if UNITY_EDITOR
                    Debug.Log($"[DuckHunter] Wave {currentWave} Complete!");
#endif
                    StartCoroutine(WaitAndNextWave());
                }
            }
            else
            {
                // Decoy/Neutral hit
                wrongTargetsEliminated++;
#if UNITY_EDITOR
                Debug.LogWarning($"[DuckHunter] Error hit on {hitType}. {wrongTargetsEliminated}/{wrongHitsThreshold}");
#endif

                if (wrongTargetsEliminated >= wrongHitsThreshold)
                {
                    GameOver();
                }
            }
        }

        private System.Collections.IEnumerator WaitAndNextWave()
        {
            // Deshabilitar apuntado durante la transición
            if (playerGunAim != null) playerGunAim.SetAimEnabled(false);

            // Limpiar objetivos restantes antes de la pausa
            ClearActiveTargets();

            // Pausa breve para celebrar
            yield return waveEndDelay;
            StartNextWave();
        }

        private void ClearActiveTargets()
        {
            // Usamos FindObjectsByType si estamos >= Unity 2023, o FindObjectsOfType si <2023.
            // Dado que en el código ya se usó FindFirstObjectByType, asumimos API nueva.
#if UNITY_2023_1_OR_NEWER
            DuckTarget[] activeTargets = FindObjectsByType<DuckTarget>(FindObjectsSortMode.None);
#else
            DuckTarget[] activeTargets = FindObjectsOfType<DuckTarget>();
#endif
            foreach (DuckTarget target in activeTargets)
            {
                if (target != null)
                {
                    Destroy(target.gameObject);
                }
            }
        }

        public void GameOver()
        {
            if (!isGameActive) return;
            isGameActive = false;

#if UNITY_EDITOR
            Debug.Log("[DuckHunter] GAME OVER - NPC TRAP ACTIVATED!");
#endif
            StopAllCoroutines(); // Detener spawner

            // Deshabilitar apuntado
            if (playerGunAim != null) playerGunAim.SetAimEnabled(false);

            // Limpiar enemigos activos
            ClearActiveTargets();

            // Disparar secuencia dramática de Game Over
            if (gunSequence != null)
            {
                gunSequence.TriggerGameOverSequence();
            }
            else
            {
                // Fallback: perder directamente si no hay secuencia configurada
                HandleGameOverComplete();
            }
        }

        public void HandleGameOverComplete()
        {
            // Este método será llamado por el evento de GameOverGunSequence
            if (controller != null)
            {
                controller.LoseGame();
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("[DuckHunter] MinigameController missing!");
            }
#endif
        }
    }
}
