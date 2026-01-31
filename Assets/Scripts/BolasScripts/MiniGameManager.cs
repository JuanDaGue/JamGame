using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameJam.MiniGames;

[DisallowMultipleComponent]
public class MiniGameManager : MonoBehaviour
{
    [Header("References")]
    public ShuffleManager shuffleManager;
    public List<Cup> cups = new List<Cup>();

    [Header("Progression / Scene Flow (NEW)")]
    [Tooltip("Referencia al MinigameController de esta escena (recomendado asignarlo en Inspector).")]
    public MinigameController minigameController;

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
    // STATE
    // ======================================================

    private bool _roundActive;
    private bool _canSelect;

    void Reset()
    {
        if (shuffleManager == null)
            shuffleManager = FindAnyObjectByType<ShuffleManager>();
    }

    void Start()
    {
        Debug.Log("[MiniGameManager] Start()");

        if (minigameController == null)
            minigameController = FindFirstObjectByType<MinigameController>();

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
            cup.selectable = false;
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
                v => shuffleManager.speedMultiplier = Mathf.Max(0.05f, v)
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
        if (_roundActive)
        {
            Debug.Log("[MiniGameManager] StartRound() ignorado: ya hay una ronda activa.");
            return;
        }

        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("[MiniGameManager] StartRound(): no hay cups asignados.");
            return;
        }

        if (shuffleManager == null)
        {
            Debug.LogWarning("[MiniGameManager] StartRound(): ShuffleManager no asignado. La ronda seguirá sin mezclar.");
        }

        StartCoroutine(RoundCoroutine());
    }

    private IEnumerator RoundCoroutine()
    {
        _roundActive = true;
        _canSelect = false;

        Debug.Log("[MiniGameManager] RoundCoroutine: empezando ronda.");
        UpdateInstruction("Observa dónde están las bolas…");
        OnRoundStarted?.Invoke();

        ClearAllBalls();
        foreach (var cup in cups)
        {
            cup.ResetLift();
            cup.selectable = false;
        }

        AssignBallsRandomly();

        // ⬆️ Levantar vasos
        foreach (var cup in cups)
            cup.Lift();

        yield return new WaitForSeconds(liftDuration);

        // 👁 Mostrar bolas
        ShowAllBalls();
        yield return new WaitForSeconds(initialRevealTime);

        // ⬇️ Bajar vasos
        foreach (var cup in cups)
            cup.Lower();

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
            Debug.Log("[MiniGameManager] No hay ShuffleManager -> OnShuffleFinished() directo.");
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

        for (int i = 0; i < indices.Count; i++)
        {
            int j = Random.Range(i, indices.Count);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        int maxBalls = Mathf.Min(ballsToPlace, cups.Count);
        for (int i = 0; i < maxBalls; i++)
            cups[indices[i]].SetHasBall(true);

        Debug.Log($"[MiniGameManager] AssignBallsRandomly: colocadas {maxBalls} bolas.");
    }

    private void ClearAllBalls()
    {
        foreach (var cup in cups)
            cup.SetHasBall(false);
    }

    private void ShowAllBalls()
    {
        foreach (var cup in cups)
            if (cup.hasBall)
                cup.SetBallVisible(true);
    }

    private void HideAllBalls()
    {
        foreach (var cup in cups)
            cup.SetBallVisible(false);
    }

    // ======================================================
    // SELECTION
    // ======================================================

    public void OnShuffleFinished()
    {
        Debug.Log("[MiniGameManager] OnShuffleFinished() recibida.");
        _canSelect = true;

        foreach (var cup in cups)
            cup.selectable = true;

        UpdateInstruction("Encuentra el vaso vacío");
    }

    public void OnPlayerSelect(Cup cup)
    {
        if (!_roundActive || !_canSelect || cup == null)
            return;

        _canSelect = false;

        foreach (var c in cups)
            c.selectable = false;

        cup.LiftAndLock();

        bool win = !cup.hasBall;

        Debug.Log(win ? "[MiniGameManager] WIN" : "[MiniGameManager] LOSE");

        StartCoroutine(ResolveSelection(win));
    }

    private IEnumerator ResolveSelection(bool playerWon)
    {
        UpdateInstruction(playerWon ? "¡Correcto!" : "¡Incorrecto!");

        if (audioSource && revealAllClip)
            audioSource.PlayOneShot(revealAllClip);

        foreach (var cup in cups)
        {
            cup.SetBallVisible(cup.hasBall);
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

        // Pequeña pausa para feedback
        yield return new WaitForSeconds(1f);

        _roundActive = false;
        UpdateInstruction("Pulsa iniciar para jugar de nuevo");

        // NEW SYSTEM HOOK:
        // Al terminar la ronda, enviamos resultado al sistema global y volvemos al Hub.
        // Si quieres permitir "reintentar" sin salir, coméntalo y maneja el retorno desde UI.
        if (minigameController != null)
        {
            if (playerWon) minigameController.WinGame();
            else minigameController.LoseGame();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] No se encontró MinigameController. No se puede volver al Hub.");
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
