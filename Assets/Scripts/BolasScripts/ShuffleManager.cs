using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ShuffleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Cup> cups = new List<Cup>();
    [SerializeField] private List<Transform> positions = new List<Transform>();

    [Header("Shuffle Parameters")]
    [Tooltip("Cantidad de intercambios durante el shuffle.")]
    [SerializeField] private int swapCount = 10;

    [Tooltip("Duración mínima de un swap (antes de aplicar SpeedMultiplier).")]
    [SerializeField] private float minDuration = 0.18f;

    [Tooltip("Duración máxima de un swap (antes de aplicar SpeedMultiplier).")]
    [SerializeField] private float maxDuration = 0.45f;

    [Tooltip("Multiplicador de velocidad global (más alto = más rápido).")]
    [SerializeField] private float speedMultiplier = 1f;

    /// <summary>
    /// Multiplicador de velocidad global. (más alto = más rápido)
    /// Lo exponemos para UI/Slider sin acceder al campo privado.
    /// </summary>
    public float SpeedMultiplier
    {
        get => speedMultiplier;
        set => speedMultiplier = Mathf.Max(0.05f, value);
    }

    [Range(0.1f, 1f)]
    [Tooltip("Qué tanto se superponen los swaps (1 = solapan casi completo).")]
    [SerializeField] private float overlapFactor = 0.95f;

    [Header("Trolling (Opcional)")]
    [SerializeField] private bool allowGhostSwaps = true;

    [Range(0f, 1f)]
    [SerializeField] private float ghostSwapChance = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool useDebugSeed = false;
    [SerializeField] private int debugSeed = 12345;

    [Header("Events")]
    [SerializeField] private UnityEvent onShuffleComplete;
    public UnityEvent OnShuffleComplete => onShuffleComplete;

    private Coroutine _shuffleRoutine;
    public bool IsShuffling { get; private set; }

    // ======================================================
    // UNITY
    // ======================================================

    void Awake()
    {
        SnapCupsToPositions();
    }

    // ======================================================
    // CONTROL
    // ======================================================

    public void StartShuffle()
    {
        if (IsShuffling)
            return;

        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("[ShuffleManager] No hay cups asignados.");
            return;
        }

        if (positions == null || positions.Count < cups.Count)
        {
            Debug.LogError("[ShuffleManager] Positions insuficientes (debe haber >= cups).");
            return;
        }

        _shuffleRoutine = StartCoroutine(ShuffleCoroutine());
    }

    public void StopShuffle()
    {
        if (_shuffleRoutine != null)
        {
            StopCoroutine(_shuffleRoutine);
            _shuffleRoutine = null;
        }

        IsShuffling = false;
    }

    // ======================================================
    // SHUFFLE LOGIC
    // ======================================================

    private IEnumerator ShuffleCoroutine()
    {
        if (useDebugSeed)
            Random.InitState(debugSeed);

        IsShuffling = true;

        // 🔒 Bloquear selección durante shuffle
        foreach (var cup in cups)
        {
            if (cup != null)
                cup.Selectable = false;
        }

        int cupCount = cups.Count;

        // swaps
        for (int i = 0; i < swapCount; i++)
        {
            int a = Random.Range(0, cupCount);
            int b = Random.Range(0, cupCount - 1);
            if (b >= a) b++;

            Cup cupA = cups[a];
            Cup cupB = cups[b];

            if (cupA == null || cupB == null)
                continue;

            int idxA = cupA.CurrentIndex;
            int idxB = cupB.CurrentIndex;

            idxA = Mathf.Clamp(idxA, 0, positions.Count - 1);
            idxB = Mathf.Clamp(idxB, 0, positions.Count - 1);

            Vector3 posA = positions[idxA].position;
            Vector3 posB = positions[idxB].position;

            float duration = Random.Range(minDuration, maxDuration) / Mathf.Max(0.05f, speedMultiplier);

            bool ghost = allowGhostSwaps && Random.value < ghostSwapChance;

            if (ghost)
            {
                // 👻 Swap falso (no cambia índices)
                float half = Mathf.Max(0.05f, duration * 0.5f);

                cupA.StartMoveTo(posB, half);
                cupB.StartMoveTo(posA, half);

                yield return new WaitForSeconds(half * overlapFactor);

                cupA.StartMoveTo(posA, half);
                cupB.StartMoveTo(posB, half);

                yield return new WaitForSeconds(half * overlapFactor);
            }
            else
            {
                // 🔁 Swap real (lógico + visual)
                cupA.CurrentIndex = idxB;
                cupB.CurrentIndex = idxA;

                cupA.StartMoveTo(posB, duration);
                cupB.StartMoveTo(posA, duration);

                yield return new WaitForSeconds(duration * overlapFactor);
            }
        }

        // ⏹️ Pequeña espera para asegurar fin de animaciones
        yield return new WaitForSeconds(0.05f);

        // ✅ Fijar posiciones finales + actualizar base local (para lift/lower)
        foreach (var cup in cups)
        {
            if (cup == null)
                continue;

            int idx = Mathf.Clamp(cup.CurrentIndex, 0, positions.Count - 1);

            // CRÍTICO: Cup mueve el ROOT en StartMoveTo(), así que el snap final debe mover el root
            cup.transform.position = positions[idx].position;

            // El lift usa visual.localPosition, esta base debe ser coherente
            cup.UpdateBasePosition();
        }

        IsShuffling = false;
        _shuffleRoutine = null;

        OnShuffleComplete?.Invoke();
    }

    // ======================================================
    // UTILIDAD
    // ======================================================

    [ContextMenu("Snap Cups To Positions")]
    public void SnapCupsToPositions()
    {
        if (cups == null || positions == null) return;

        for (int i = 0; i < cups.Count; i++)
        {
            var cup = cups[i];
            if (cup == null) continue;

            int idx = Mathf.Clamp(cup.CurrentIndex, 0, positions.Count - 1);
            cup.transform.position = positions[idx].position;
            cup.UpdateBasePosition();
        }
    }
}
