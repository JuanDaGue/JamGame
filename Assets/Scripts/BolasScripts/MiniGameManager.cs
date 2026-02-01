using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameJam.MiniGames; // MinigameController (nuevo sistema)

[DisallowMultipleComponent]
public class MiniGameManager : MonoBehaviour
{
    [Header("References")]
    public ShuffleManager shuffleManager;
    public List<Cup> cups = new List<Cup>();

    [Header("Round Parameters")]
    [Tooltip("Tiempo que las bolas permanecen visibles al inicio.")]
    public float initialRevealTime = 1.2f;

    [Tooltip("Duración del levantar / bajar vasos.")]
    public float liftDuration = 0.35f;

    [Tooltip("Duración del pop visual al revelar bolas.")]
    public float revealVisualDuration = 0.18f;

    [Range(1, 2)]
    [Tooltip("Cantidad de bolas activas.")]
    public int ballsToPlace = 2;

    [Header("UI (Optional)")]
    public Text instructionText;
    public Button startButton;
    public Slider speedSlider;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip revealAllClip;

    [Header("Events")]
    public UnityEvent OnRoundStarted;
    public UnityEvent OnRoundFinishedWin;
    public UnityEvent OnRoundFinishedLose;

    [Header("Debug / Flow")]
    [Tooltip("Si true la ronda arrancará automáticamente al Start (útil para testing).")]
    public bool autoStart = true;

    // ======================================================
    // LOSE SEQUENCE (NPC + BLINK + VFX + RETURN HUB)
    // ======================================================

    [Header("Lose Sequence (Opcional)")]
    [Tooltip("Componente que dispara la animación de explosión del NPC.")]
    public NPCDeathAnimator npcDeathAnimator;

    [Tooltip("Componente que hace parpadear las bolas en rojo (idealmente 3 veces).")]
    public BallBlinker ballBlinker;

    [Tooltip("Prefab VFX de explosión (instanciar).")]
    public GameObject explosionVfxPrefab;

    [Tooltip("Punto alternativo donde aparece el VFX si no hay cup seleccionado.")]
    public Transform explosionVfxPoint;

    [Tooltip("Tiempo para que se note la animación 'Explotion' antes del parpadeo.")]
    public float npcExplotionLeadTime = 0.4f;

    [Tooltip("Delay tras el VFX antes de salir al hub.")]
    public float afterVfxDelay = 0.5f;

    [Tooltip("MinigameController del nuevo sistema. Si es null, fallback a GameManager.Instance.GameOver().")]
    public MinigameController minigameController;

    [Tooltip("Offset vertical del VFX para que no quede clavado en el piso.")]
    public float vfxUpOffset = 0.2f;

    // ======================================================
    // STATE
    // ======================================================

    private bool _roundActive;
    private bool _canSelect;
    private bool _isResolving;

    private Cup _selectedCup; // <-- vaso seleccionado (para VFX)

    // ======================================================
    // UNITY
    // ======================================================

    void Reset()
    {
        if (shuffleManager == null)
            shuffleManager = FindAnyObjectByType<ShuffleManager>();
    }

    void Start()
    {
        Debug.Log("[MiniGameManager] Start()");

        if (shuffleManager == null)
            Debug.LogError("[MiniGameManager] ShuffleManager no asignado.");

        if (cups == null || cups.Count == 0)
            Debug.LogError("[MiniGameManager] Cups no asignados.");

        // Reset fuerte inicial
        foreach (var cup in cups)
        {
            if (cup == null) continue;

            cup.SetHasBall(false);
            cup.ResetLift();
            cup.Selectable = false;
        }

        if (shuffleManager != null)
            shuffleManager.OnShuffleComplete.AddListener(OnShuffleFinished);

        // Suscribirse a selección (captura segura)
        foreach (var cup in cups)
        {
            if (cup == null) continue;

            cup.OnSelected.RemoveAllListeners();
            Cup capturedCup = cup;
            cup.OnSelected.AddListener(() => OnPlayerSelect(capturedCup));
        }

        if (startButton != null)
            startButton.onClick.AddListener(StartRound);

        if (speedSlider != null && shuffleManager != null)
            speedSlider.onValueChanged.AddListener(
                v => shuffleManager.SpeedMultiplier = Mathf.Max(0.05f, v)
            );

        UpdateInstruction("Pulsa iniciar para jugar");

        if (autoStart)
        {
            Debug.Log("[MiniGameManager] autoStart=true -> arrancando StartRound()");
            StartRound();
        }
    }

    // ======================================================
    // ROUND FLOW
    // ======================================================

    public void StartRound()
    {
        if (_roundActive || _isResolving)
        {
            Debug.Log("[MiniGameManager] StartRound() ignorado: ronda activa o resolviendo.");
            return;
        }

        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("[MiniGameManager] StartRound(): no hay cups asignados.");
            return;
        }

        StartCoroutine(RoundCoroutine());
    }

    private IEnumerator RoundCoroutine()
    {
        _roundActive = true;
        _canSelect = false;
        _isResolving = false;
        _selectedCup = null;

        Debug.Log("[MiniGameManager] RoundCoroutine: empezando ronda.");
        UpdateInstruction("Observa dónde están las bolas…");
        OnRoundStarted?.Invoke();

        // Reset total
        ClearAllBalls();
        foreach (var cup in cups)
        {
            if (cup == null) continue;
            cup.ResetLift();
            cup.Selectable = false;
        }

        AssignBallsRandomly();

        // ⬆️ Levantar vasos
        foreach (var cup in cups)
            if (cup != null) cup.Lift();

        yield return new WaitForSeconds(liftDuration);

        // 👁 Mostrar bolas
        ShowAllBalls();
        yield return new WaitForSeconds(initialRevealTime);

        // ⬇️ Bajar vasos
        foreach (var cup in cups)
            if (cup != null) cup.Lower();

        yield return new WaitForSeconds(liftDuration * 0.9f);

        // ❌ Ocultar bolas
        HideAllBalls();

        // 🔀 Shuffle
        if (shuffleManager != null)
        {
            shuffleManager.SnapCupsToPositions();
            Debug.Log("[MiniGameManager] Lanzando shuffleManager.StartShuffle()");
            UpdateInstruction("Mezclando…");
            shuffleManager.StartShuffle();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] No hay ShuffleManager -> OnShuffleFinished() directo.");
            OnShuffleFinished();
        }
    }

    // ======================================================
    // BALL LOGIC
    // ======================================================

    private void AssignBallsRandomly()
    {
        if (cups == null || cups.Count == 0) return;

        List<int> indices = new List<int>();
        for (int i = 0; i < cups.Count; i++)
            indices.Add(i);

        // Fisher–Yates
        for (int i = 0; i < indices.Count; i++)
        {
            int j = Random.Range(i, indices.Count);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        int maxBalls = Mathf.Min(ballsToPlace, cups.Count);
        for (int i = 0; i < maxBalls; i++)
        {
            if (cups[indices[i]] != null)
                cups[indices[i]].SetHasBall(true);
        }

        Debug.Log($"[MiniGameManager] AssignBallsRandomly: colocadas {maxBalls} bolas.");
    }

    private void ClearAllBalls()
    {
        foreach (var cup in cups)
            if (cup != null) cup.SetHasBall(false);
    }

    private void ShowAllBalls()
    {
        foreach (var cup in cups)
        {
            if (cup == null) continue;
            if (cup.HasBall)
                cup.SetBallVisible(true);
        }
    }

    private void HideAllBalls()
    {
        foreach (var cup in cups)
            if (cup != null) cup.SetBallVisible(false);
    }

    // ======================================================
    // SELECTION
    // ======================================================

    public void OnShuffleFinished()
    {
        if (!_roundActive) return;

        Debug.Log("[MiniGameManager] OnShuffleFinished() recibida.");
        _canSelect = true;

        foreach (var cup in cups)
            if (cup != null) cup.Selectable = true;

        UpdateInstruction("Encuentra el vaso vacío");
    }

    public void OnPlayerSelect(Cup cup)
    {
        if (!_roundActive || !_canSelect || _isResolving || cup == null)
            return;

        _selectedCup = cup; // <-- guardamos el vaso elegido (para el VFX)

        _isResolving = true;
        _canSelect = false;

        // Bloquear todos inmediatamente
        foreach (var c in cups)
            if (c != null) c.Selectable = false;

        // Levantar SOLO el vaso clickeado
        cup.LiftAndLock();

        bool win = !cup.HasBall;
        StartCoroutine(ResolveSelection(win));
    }

    private IEnumerator ResolveSelection(bool playerWon)
    {
        UpdateInstruction(playerWon ? "¡Correcto!" : "¡Incorrecto!");

        if (audioSource && revealAllClip)
            audioSource.PlayOneShot(revealAllClip);

        // Reveal final
        foreach (var cup in cups)
        {
            if (cup == null) continue;
            cup.SetBallVisible(cup.HasBall);
            cup.StartRevealSequence(this, revealVisualDuration);
            yield return new WaitForSeconds(0.05f);
        }

        if (audioSource)
        {
            if (playerWon && winClip)
                audioSource.PlayOneShot(winClip);
            else if (!playerWon && loseClip)
                audioSource.PlayOneShot(loseClip);
        }

        if (playerWon) OnRoundFinishedWin?.Invoke();
        else OnRoundFinishedLose?.Invoke();

        yield return new WaitForSeconds(0.35f);

        if (playerWon)
        {
            minigameController.WinGame(); // Nuevo sistema preferido
            Debug.Log("[MiniGameManager] WIN");
            _roundActive = false;
            _isResolving = false;
            UpdateInstruction("Pulsa iniciar para jugar de nuevo");
        }
        else
        {
            Debug.Log("[MiniGameManager] LOSE -> LoseSequence()");
            yield return LoseSequence();
        }
    }

    private IEnumerator LoseSequence()
    {
        // Asegurar bloqueo total
        _canSelect = false;
        foreach (var c in cups)
            if (c != null) c.Selectable = false;

        // 1) NPC entra a Explotion (si existe)
        if (npcDeathAnimator != null)
            npcDeathAnimator.PlayExplotion();

        if (npcExplotionLeadTime > 0f)
            yield return new WaitForSeconds(npcExplotionLeadTime);

        // 2) Bolas parpadean rojo (si existe)
        if (ballBlinker != null)
            yield return ballBlinker.BlinkRoutine();
        else
            yield return new WaitForSeconds(0.4f);

        // 3) VFX Explosion SOLO si el vaso elegido tenía bola
        if (explosionVfxPrefab != null && _selectedCup != null && _selectedCup.HasBall)
        {
            Transform p = (_selectedCup.Visual != null) ? _selectedCup.Visual : _selectedCup.transform;
            Vector3 pos = p.position + Vector3.up * vfxUpOffset;
            Instantiate(explosionVfxPrefab, pos, Quaternion.identity);
        }
        else if (explosionVfxPrefab != null && explosionVfxPoint != null)
        {
            // Fallback opcional si quieres ver algo aun sin cup/bola (puedes borrar este else si no lo deseas)
            Vector3 pos = explosionVfxPoint.position + Vector3.up * vfxUpOffset;
            Instantiate(explosionVfxPrefab, pos, explosionVfxPoint.rotation);
        }

        if (afterVfxDelay > 0f)
            yield return new WaitForSeconds(afterVfxDelay);

        // 4) Salir al Hub (nuevo sistema preferido)
        if (minigameController != null)
        {
            minigameController.LoseGame();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] MinigameController no asignado. Fallback a GameManager.Instance.GameOver().");
            GameManager.Instance.GameOver();
        }
    }

    // ======================================================
    // UI
    // ======================================================

    private void UpdateInstruction(string text)
    {
        if (instructionText != null)
            instructionText.text = text;
    }
}
