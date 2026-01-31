using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ShuffleManager : MonoBehaviour
{
    [Header("References")]
    public List<Cup> cups = new List<Cup>();
    public List<Transform> positions = new List<Transform>();

    [Header("Shuffle Parameters")]
    [Tooltip("Cantidad de intercambios durante el shuffle.")]
    public int swapCount = 10;

    [Tooltip("Duración mínima de un swap.")]
    public float minDuration = 0.18f;

    [Tooltip("Duración máxima de un swap.")]
    public float maxDuration = 0.45f;

    [Tooltip("Multiplicador de velocidad global.")]
    public float speedMultiplier = 1f;

    [Range(0.1f, 1f)]
    [Tooltip("Qué tanto se superponen los swaps.")]
    public float overlapFactor = 0.95f;

    [Header("Trolling (Opcional)")]
    public bool allowGhostSwaps = true;

    [Range(0f, 1f)]
    public float ghostSwapChance = 0.12f;

    [Header("Debug")]
    public bool useDebugSeed = false;
    public int debugSeed = 12345;

    [Header("Events")]
    public UnityEvent OnShuffleComplete;

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
                cup.selectable = false;
        }

        int cupCount = cups.Count;
        int posCount = positions.Count;

        if (cupCount < 2 || posCount < cupCount)
        {
            Debug.LogError("[ShuffleManager] Cups / Positions mal configurados.");
            IsShuffling = false;
            yield break;
        }

        for (int i = 0; i < swapCount; i++)
        {
            int a = Random.Range(0, cupCount);
            int b = Random.Range(0, cupCount - 1);
            if (b >= a) b++;

            Cup cupA = cups[a];
            Cup cupB = cups[b];

            if (cupA == null || cupB == null)
                continue;

            int idxA = cupA.currentIndex;
            int idxB = cupB.currentIndex;

            Vector3 posA = positions[idxA].position;
            Vector3 posB = positions[idxB].position;

            float duration = Random.Range(minDuration, maxDuration)
                             / Mathf.Max(0.05f, speedMultiplier);

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
                cupA.currentIndex = idxB;
                cupB.currentIndex = idxA;

                cupA.StartMoveTo(posB, duration);
                cupB.StartMoveTo(posA, duration);

                yield return new WaitForSeconds(duration * overlapFactor);
            }
        }

        // ⏹️ Pequeña espera para asegurar fin de animaciones
        yield return new WaitForSeconds(0.05f);

        // ✅ Fijar posiciones finales + actualizar base local
        foreach (var cup in cups)
        {
            if (cup == null || cup.visual == null)
                continue;

            int idx = Mathf.Clamp(cup.currentIndex, 0, positions.Count - 1);
            cup.visual.position = positions[idx].position;

            // 🔥 CRÍTICO: base correcta para Lift / Lower
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
        foreach (var cup in cups)
        {
            if (cup == null || cup.visual == null)
                continue;

            int idx = Mathf.Clamp(cup.currentIndex, 0, positions.Count - 1);
            cup.visual.position = positions[idx].position;
            cup.UpdateBasePosition();
        }
    }
}
