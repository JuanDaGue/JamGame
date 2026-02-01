using System;
using UnityEngine;

namespace GameJam.Core
{
    /// <summary>
    /// La "Biblia de Eventos".
    /// Punto central de comunicación para todo el juego.
    /// Cualquier sistema puede suscribirse o disparar estos eventos sin conocer a los demás.
    /// </summary>
    public static class GameEvents
    {
        // =================================================================================
        // SECCIÓN: COLECCIONABLES
        // =================================================================================
        
        /// <summary>
        /// Se dispara cuando un coleccionable cambia de estado (se adquiere o se pierde).
        /// Param 1 (CollectibleData): El dato del item.
        /// Param 2 (bool): True si se obtuvo, False si se perdió.
        /// </summary>
        public static Action<CollectibleData, bool> OnCollectibleStateChanged;

        /// <summary>
        /// Evento opcional por si queremos notificar algo más genérico o específico por ID.
        /// </summary>
        public static Action<string> OnCollectibleCollectedById;


        // =================================================================================
        // SECCIÓN: MINIJUEGOS (Futuro)
        // =================================================================================
        // public static Action OnMiniGameStart;
        // public static Action OnMiniGameEnd;
        public static Action<int, int> OnLivesChanged; // current, max
        public static Action OnPlayerDied;
    }
}
