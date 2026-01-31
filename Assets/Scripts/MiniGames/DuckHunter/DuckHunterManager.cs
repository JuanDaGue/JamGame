using UnityEngine;
using GameJam.Core;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHunterManager : MonoBehaviour
    {
        [Header("Referencias")]
        public DuckSpawner spawner;
        public DuckHunterUI ui;
        public MinigameController controller;

        [Header("Configuración de Oleadas")]
        public int totalWaves = 3;
        public int targetsPerWave = 10;
        public float spawnRate = 1.5f;

        // Estado del Juego
        private int currentWave = 0;
        private int realTargetsEliminated = 0;
        private int wrongTargetsEliminated = 0;

        // "Verdad" vs "Mentira" de la oleada actual
        private TargetType currentRealTarget;
        private TargetType currentDecoyTarget;
        
        // Umbral de derrota (Mitad + 1 de los objetivos no-reales generados... 
        // simplificado: si matas X cantidad de inocentes/trampas, pierdes).
        // Calcularemos esto basado en lo que spawnea o un valor fijo por balance.
        private int wrongHitsThreshold = 3; 

        private void Start()
        {
            StartNextWave();
        }

        private void StartNextWave()
        {
            currentWave++;
            if (currentWave > totalWaves)
            {
                // Victoria Final
                controller.WinGame();
                return;
            }

            // 1. Definir la Regla de la Oleada (Real vs Trampa)
            SetupWaveRules();

            // 2. Configurar UI para engañar al jugador
            // Le decimos "Dispara al Decoy", pero el Real es otro
            ui.SetInstruction(currentDecoyTarget);

            // 3. Calcular umbral de derrota para esta oleada
            wrongHitsThreshold = (targetsPerWave / 2) + 1;
            wrongTargetsEliminated = 0;
            realTargetsEliminated = 0;

            // 4. Iniciar Spawner
            spawner.SpawnWave(targetsPerWave, currentRealTarget, currentDecoyTarget, TargetType.Neutral, spawnRate);
            
            Debug.Log($"[DuckHunter] Oleada {currentWave}: UI dice {currentDecoyTarget}, Real es {currentRealTarget}");
        }

        private void SetupWaveRules()
        {
            // Elegir aleatoriamente roles para Real y Decoy entre (0, 1, 2)
            // Asumimos que TargetType tiene 3 valores significativos
            int pivot = UnityEngine.Random.Range(0, 3);
            
            // Lógica simple: 
            // Si sale 0 -> Real=0, Decoy=1
            // Si sale 1 -> Real=1, Decoy=2
            // Si sale 2 -> Real=2, Decoy=0
            
            currentRealTarget = (TargetType)pivot;
            currentDecoyTarget = (TargetType)((pivot + 1) % 3);
        }

        public void RegisterHit(TargetType hitType)
        {
            if (hitType == currentRealTarget)
            {
                realTargetsEliminated++;
                ui.UpdateScore(realTargetsEliminated); // Feedback positivo (sonido?)
                
                // Chequear si pasamos de oleada (ej. si matamos el 80% de los reales generados?
                // O esperamos a que termine el tiempo? Por ahora simple: al terminar spawn)
                // MEJORA: El spawner debería avisar cuando termina.
                // Por simplicidad del jam: si matas suficientes reales, pasas.
                if (realTargetsEliminated >= targetsPerWave / 3) // Balancear esto
                {
                    // Forzar siguiente oleada cancelando la actual? 
                    // Mejor dejar que termine el spawn routine.
                }
            }
            else
            {
                // Disparó a Trampa o Neutral
                wrongTargetsEliminated++;
                Debug.LogWarning($"[DuckHunter] Error! Llevas {wrongTargetsEliminated}/{wrongHitsThreshold}");

                if (wrongTargetsEliminated >= wrongHitsThreshold)
                {
                    GameOver();
                }
            }
        }

        public void GameOver()
        {
            Debug.LogError("[DuckHunter] GAME OVER - NPC TRAP ACTIVATED on you!");
            StopAllCoroutines(); // Detener spawner
            // Aquí iría la animación del NPC disparando
            controller.LoseGame();
        }
    }
}
