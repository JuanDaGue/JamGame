using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Cup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform visual que se levanta (child).")]
    [SerializeField] private Transform visual;
    public Transform Visual => visual;

    [Tooltip("GameObject que representa la bola dentro del vaso.")]
    [SerializeField] private GameObject ballVisual;

    [Header("State")]
    [Tooltip("Índice lógico de posición (ShuffleManager).")]
    [SerializeField] private int currentIndex = 0;
    public int CurrentIndex { get => currentIndex; set => currentIndex = value; }

    [Tooltip("Indica si este vaso tiene bola.")]
    [SerializeField] private bool hasBall = false;
    public bool HasBall => hasBall;

    [Tooltip("¿Puede ser seleccionado por el jugador?")]
    [SerializeField] private bool selectable = false;
    public bool Selectable { get => selectable; set => selectable = value; }

    [Header("Movement")]
    [Tooltip("Altura vertical al levantar el vaso.")]
    [SerializeField] private float liftHeight = 0.35f;

    [Tooltip("Altura del arco durante swaps.")]
    [SerializeField] private float moveArcHeight = 0.25f;

    [Tooltip("Duración del lift / lower.")]
    [SerializeField] private float liftDuration = 0.3f;

    [Tooltip("Permite selección con mouse (debug).")]
    [SerializeField] private bool enableOnMouseDown = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip revealClip;
    [SerializeField] private AudioClip selectClip;

    [Header("Events")]
    [SerializeField] private UnityEvent onSelected;
    public UnityEvent OnSelected => onSelected;
    [SerializeField] private UnityEvent onMoveComplete;
    public UnityEvent OnMoveComplete => onMoveComplete;

    // ======================================================
    // INTERNAL
    // ======================================================

    private Coroutine _moveCoroutine;
    private Vector3 _baseLocalPos; // base local del visual (solo Y usado para lift)
    private bool _isMoving = false;
    private bool _lockedUp = false;

    public bool IsMoving => _isMoving;

    // ======================================================
    // UNITY
    // ======================================================

    void Reset()
    {
        if (visual == null && transform.childCount > 0)
            visual = transform.GetChild(0);
    }

    void Awake()
    {
        if (visual == null)
        {
            Debug.LogWarning($"[{name}] Visual no asignado.", this);
            return;
        }

        // Guardamos la posición local base del visual (para lift/lower)
        _baseLocalPos = visual.localPosition;

        // Seguridad: preferible tener collider en el root (GameObject con este script)
        // y NO colliders en hijos; si quieres, desactiva coliders hijos aquí:
        // foreach (var col in GetComponentsInChildren<Collider>()) if (col.gameObject != gameObject) col.enabled = false;

        SetBallVisible(false);
    }

    // ======================================================
    // 🔁 POSICIÓN BASE (para lift)
    // ======================================================

    /// <summary>
    /// Actualiza la posición base local del visual.
    /// Debe llamarse después de que el shuffle fije la posición final del root (transform.position).
    /// </summary>
    public void UpdateBasePosition()
    {
        if (visual == null) return;
        _baseLocalPos = visual.localPosition;
    }

    // ======================================================
    // SHUFFLE MOVEMENT (ROOT — MUEVE transform.position)
    // ======================================================

    /// <summary>
    /// Mueve el root (transform.position) hacia targetPos en world space.
    /// Esto mantiene el collider alineado con lo visible.
    /// </summary>
    public void StartMoveTo(Vector3 targetPos, float duration)
    {
        if (_lockedUp) return;

        CancelMove();
        _moveCoroutine = StartCoroutine(MoveRootCoroutine(targetPos, duration));
    }

    private IEnumerator MoveRootCoroutine(Vector3 targetPos, float duration)
    {
        _isMoving = true;

        Vector3 start = transform.position; // <-- mover el root, no el visual
        float elapsed = 0f;

        if (audioSource != null && moveClip != null)
            audioSource.PlayOneShot(moveClip);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(start, targetPos, ease);
            pos += Vector3.up * (Mathf.Sin(ease * Mathf.PI) * moveArcHeight);

            transform.position = pos; // <-- aquí
            yield return null;
        }

        transform.position = targetPos;

        _isMoving = false;
        _moveCoroutine = null;
        OnMoveComplete?.Invoke();
    }

    public void CancelMove()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        _isMoving = false;
    }

    // ======================================================
    // LIFT / LOWER (VISUAL — LOCAL Y)
    // ======================================================

    public void Lift()
    {
        if (_lockedUp) return;
        CancelMove();
        StartCoroutine(LiftLowerCoroutine(true));
    }

    public void Lower()
    {
        if (_lockedUp) return;
        CancelMove();
        StartCoroutine(LiftLowerCoroutine(false));
    }

    /// <summary>
    /// Levanta y bloquea el vaso (usado al seleccionar).
    /// </summary>
    public void LiftAndLock()
    {
        CancelMove();
        _lockedUp = true;
        selectable = false;
        StartCoroutine(LiftLowerCoroutine(true));
    }

    /// <summary>
    /// Restaura el visual a su posición base local.
    /// </summary>
    public void ResetLift()
    {
        CancelMove();
        _lockedUp = false;
        selectable = false;
        if (visual != null)
            visual.localPosition = _baseLocalPos;
    }

    private IEnumerator LiftLowerCoroutine(bool up)
    {
        if (visual == null) yield break;

        _isMoving = true;

        Vector3 start = visual.localPosition;
        Vector3 target = up ? _baseLocalPos + Vector3.up * liftHeight : _baseLocalPos;

        float t = 0f;
        while (t < liftDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / liftDuration);
            visual.localPosition = Vector3.Lerp(start, target, k);
            yield return null;
        }

        visual.localPosition = target;
        _isMoving = false;
    }

    // ======================================================
    // BALL VISUAL
    // ======================================================

    public void SetBallVisible(bool visible)
    {
        if (ballVisual != null)
            ballVisual.SetActive(visible);
    }

    public void SetHasBall(bool value)
    {
        hasBall = value;
        SetBallVisible(false); // nunca visible automáticamente
    }

    // ======================================================
    // SELECTION
    // ======================================================

    public void OnPlayerSelect()
    {
        if (!selectable || _isMoving || _lockedUp) return;

        if (audioSource != null && selectClip != null)
            audioSource.PlayOneShot(selectClip);

        OnSelected?.Invoke();
    }

    void OnMouseDown()
    {
        if (!enableOnMouseDown) return;
        OnPlayerSelect();
    }

    // ======================================================
    // REVEAL
    // ======================================================

    private IEnumerator RevealSequence(float duration)
    {
        if (audioSource != null && revealClip != null)
            audioSource.PlayOneShot(revealClip);

        if (visual == null) yield break;

        Vector3 baseScale = visual.localScale;
        Vector3 popScale = baseScale * 1.08f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            visual.localScale = Vector3.Lerp(baseScale, popScale, t / duration);
            yield return null;
        }

        visual.localScale = baseScale;
    }

    public void StartRevealSequence(MonoBehaviour owner, float duration)
    {
        if (owner != null)
            owner.StartCoroutine(RevealSequence(duration));
    }
}
