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

    [Tooltip("Duración mínima de un swap.")]
    [SerializeField] private float minDuration = 0.18f;

    [Tooltip("Duración máxima de un swap.")]
    [SerializeField] private float maxDuration = 0.45f;

    [Tooltip("Multiplicador de velocidad global.")]
    [SerializeField] private float speedMultiplier = 1f;
    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }

    [Range(0.1f, 1f)]
    [Tooltip("Qué tanto se superponen los swaps.")]
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

            int idxA = cupA.CurrentIndex;
            int idxB = cupB.CurrentIndex;

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
                cupA.CurrentIndex = idxB;
                cupB.CurrentIndex = idxA;

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
            if (cup == null || cup.Visual == null)
                continue;

            int idx = Mathf.Clamp(cup.CurrentIndex, 0, positions.Count - 1);
            cup.Visual.position = positions[idx].position;

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
            if (cup == null || cup.Visual == null)
                continue;

            int idx = Mathf.Clamp(cup.CurrentIndex, 0, positions.Count - 1);
            cup.Visual.position = positions[idx].position;
            cup.UpdateBasePosition();
        }
    }
}
