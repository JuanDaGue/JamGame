using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MiniGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShuffleManager shuffleManager;
    [SerializeField] private List<Cup> cups = new();

    [Header("Round Parameters")]
    [Tooltip("Tiempo que las bolas permanecen visibles al inicio.")]
    [SerializeField] private float initialRevealTime = 1.2f;

    [Tooltip("Duración del levantar / bajar vasos.")]
    [SerializeField] private float liftDuration = 0.35f;

    [Tooltip("Duración del pop visual al revelar bolas.")]
    [SerializeField] private float revealVisualDuration = 0.18f;

    [Range(1, 2)]
    [Tooltip("Cantidad de bolas activas.")]
    [SerializeField] private int ballsToPlace = 2;

    [Header("UI (Optional)")]
    [SerializeField] private Text instructionText;
    [SerializeField] private Button startButton;
    [SerializeField] private Slider speedSlider;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip revealAllClip;

    [Header("Events")]
    [SerializeField] private UnityEvent onRoundStarted;
    public UnityEvent OnRoundStarted => onRoundStarted;
    [SerializeField] private UnityEvent onRoundFinishedWin;
    public UnityEvent OnRoundFinishedWin => onRoundFinishedWin;
    [SerializeField] private UnityEvent onRoundFinishedLose;
    public UnityEvent OnRoundFinishedLose => onRoundFinishedLose;

    [Header("Debug / Flow")]
    [Tooltip("Si true la ronda arrancará automáticamente al Start (útil para testing).")]
    [SerializeField] private bool autoStart = true;

    // ======================================================
    // STATE
    // ======================================================

    private bool _roundActive;
    private bool _canSelect;
    private WaitForSeconds _waitSmall = new(0.05f);
    private WaitForSeconds _waitOneSecond = new(1f);

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
        Debug.Log("[GameManager] Start()");

        if (shuffleManager == null)
            Debug.LogError("[GameManager] ShuffleManager no asignado.");

        if (cups == null || cups.Count == 0)
            Debug.LogError("[GameManager] Cups no asignados.");

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

        // Auto start si lo quieres para testing
        if (autoStart)
        {
            Debug.Log("[GameManager] autoStart=true -> arrancando StartRound()");
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
            Debug.Log("[GameManager] StartRound() ignorado: ya hay una ronda activa.");
            return;
        }

        // sanity checks
        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("[GameManager] StartRound(): no hay cups asignados.");
            return;
        }

        if (shuffleManager == null)
        {
            Debug.LogWarning("[GameManager] StartRound(): ShuffleManager no asignado. La ronda seguirá sin mezclar.");
        }

        StartCoroutine(RoundCoroutine());
    }

    private IEnumerator RoundCoroutine()
    {
        _roundActive = true;
        _canSelect = false;

        Debug.Log("[GameManager] RoundCoroutine: empezando ronda.");
        UpdateInstruction("Observa dónde están las bolas…");
        OnRoundStarted?.Invoke();

        // Reset total
        ClearAllBalls();
        foreach (var cup in cups)
        {
            cup.ResetLift();
            cup.Selectable = false;
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
            // asegurar snap inicial y que positions estén bien
            shuffleManager.SnapCupsToPositions();
            Debug.Log("[GameManager] Lanzando shuffleManager.StartShuffle()");
            UpdateInstruction("Mezclando…");
            shuffleManager.StartShuffle();
        }
        else
        {
            Debug.Log("[GameManager] No hay ShuffleManager -> OnShuffleFinished() directo.");
            OnShuffleFinished();
        }
    }

    // ======================================================
    // BALL LOGIC
    // ======================================================

    private void AssignBallsRandomly()
    {
        if (cups == null || cups.Count == 0) return;

        List<int> indices = new();
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
            cups[indices[i]].SetHasBall(true);

        Debug.Log($"[GameManager] AssignBallsRandomly: colocadas {maxBalls} bolas.");
    }

    private void ClearAllBalls()
    {
        foreach (var cup in cups)
            cup.SetHasBall(false);
    }

    private void ShowAllBalls()
    {
        foreach (var cup in cups)
            if (cup.HasBall)
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
        Debug.Log("[GameManager] OnShuffleFinished() recibida.");
        _canSelect = true;

        foreach (var cup in cups)
            cup.Selectable = true;

        UpdateInstruction("Encuentra el vaso vacío");
    }

    public void OnPlayerSelect(Cup cup)
    {
        if (!_roundActive || !_canSelect || cup == null)
            return;

        _canSelect = false;

        // Bloquear todos inmediatamente
        foreach (var c in cups)
            c.Selectable = false;

        // Levantar SOLO el vaso clickeado
        cup.LiftAndLock();

        bool win = !cup.HasBall;

        // 🔥 OUTPUT DE ESTADO (por ahora solo consola)
        if (win)
            Debug.Log("WIN");
        else
        {
            //GameManager.Instance.GameOver();
            StartCoroutine(WaitForSeconds(2f));
            Debug.Log("GAME OVER");
        }

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
            cup.SetBallVisible(cup.HasBall);
            cup.StartRevealSequence(this, revealVisualDuration);
            yield return _waitSmall;
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

        yield return _waitOneSecond;

        _roundActive = false;
        UpdateInstruction("Pulsa iniciar para jugar de nuevo");
    }

    // ======================================================
    // UI
    // ======================================================

    private void UpdateInstruction(string text)
    {
        if (instructionText != null)
            instructionText.text = text;
    }
    IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameManager.Instance.GameOver();
        yield break;
    }
}
