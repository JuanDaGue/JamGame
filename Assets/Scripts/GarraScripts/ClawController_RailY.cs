using UnityEngine;
using System;

public class ClawController_RailY : MonoBehaviour
{
    public enum ClawState { Move, Dropping, Raising, Cooldown }

    [Header("Rig References")]
    [Tooltip("Transform que se moverá verticalmente (rail), típicamente el objeto Rail.")]
    public Transform railRig;

    [Tooltip("Transform que se moverá horizontalmente a lo largo del segmento LeftLimit -> RightLimit.")]
    public Transform carriage;

    [Header("Horizontal Limits (world segment)")]
    [Tooltip("Punto extremo izquierdo del recorrido (en mundo).")]
    public Transform leftLimit;

    [Tooltip("Punto extremo derecho del recorrido (en mundo).")]
    public Transform rightLimit;

    [Header("Vertical Setup (explicit local Y)")]
    [Tooltip("Posición Y local (del railRig) cuando está arriba.")]
    public float topLocalY = 0f;

    [Tooltip("Posición Y local (del railRig) cuando está abajo.")]
    public float bottomLocalY = -2f;

    [Header("Speeds")]
    [Tooltip("Velocidad horizontal (unidades del mundo por segundo a lo largo del segmento Left->Right).")]
    public float horizontalSpeed = 4f;

    [Tooltip("Velocidad de bajada en Y (unidades locales por segundo).")]
    public float dropSpeed = 6f;

    [Tooltip("Velocidad de subida en Y (unidades locales por segundo).")]
    public float raiseSpeed = 2f;

    [Header("Input")]
    public KeyCode dropKey = KeyCode.Mouse0;
    public string horizontalAxis = "Horizontal";
    public bool allowKeyboardAxis = true;

    [Header("Tuning")]
    public float arriveEpsilon = 0.001f;
    public float cooldownSeconds = 0.2f;

    [Header("Physics / Conflicts")]
    [Tooltip("Si railRig tiene Rigidbody, lo fuerza a kinematic para evitar pelea con física.")]
    public bool forceKinematicIfRigidbody = true;

    [Tooltip("Debug: advierte si el Y deja de cambiar estando en Raising/Dropping.")]
    public bool watchdogLog = false;

    [Header("Debug")]
    public ClawState state = ClawState.Move;

    public event Action OnReachedBottom;
    public event Action OnReachedTop;
    public event Action<ClawState> OnStateChanged;

    float _cooldownTimer;
    float _topY, _bottomY;

    Rigidbody _rb;
    float _lastY;
    int _stuckFrames;

    float _horizontalInput;

    void Awake()
    {
        if (!railRig) railRig = transform;

        _rb = railRig.GetComponent<Rigidbody>();
        if (_rb && forceKinematicIfRigidbody)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        _topY = topLocalY;
        _bottomY = bottomLocalY;
        SortVertical();

        _lastY = railRig.localPosition.y;
    }

    void SortVertical()
    {
        // Asegura que top >= bottom
        if (_bottomY > _topY)
        {
            float t = _topY;
            _topY = _bottomY;
            _bottomY = t;
        }
    }

    void Update()
    {
        // Leer input en Update (mejor práctica).
        if (state == ClawState.Move)
        {
            _horizontalInput = ReadHorizontalInput();

            if (Input.GetKeyDown(dropKey))
                SetState(ClawState.Dropping);
        }
        else
        {
            _horizontalInput = 0f;
        }
    }

    void FixedUpdate()
    {
        // Horizontal en FixedUpdate para evitar “pisadas” por física/constraints.
        if (state == ClawState.Move)
        {
            TickHorizontalAlongSegment(_horizontalInput);
        }

        // Vertical en FixedUpdate
        switch (state)
        {
            case ClawState.Dropping:
                if (TickRailTo(_bottomY, dropSpeed))
                {
                    OnReachedBottom?.Invoke();
                    SetState(ClawState.Raising);
                }
                break;

            case ClawState.Raising:
                if (TickRailTo(_topY, raiseSpeed))
                {
                    OnReachedTop?.Invoke();
                    if (cooldownSeconds > 0f)
                    {
                        _cooldownTimer = cooldownSeconds;
                        SetState(ClawState.Cooldown);
                    }
                    else
                    {
                        SetState(ClawState.Move);
                    }
                }
                break;

            case ClawState.Cooldown:
                _cooldownTimer -= Time.fixedDeltaTime;
                if (_cooldownTimer <= 0f)
                    SetState(ClawState.Move);
                break;
        }

        WatchdogCheck();
    }

    float ReadHorizontalInput()
    {
        float input = 0f;

        if (allowKeyboardAxis && !string.IsNullOrEmpty(horizontalAxis))
            input = Input.GetAxisRaw(horizontalAxis);

        // Fallback robusto (por si el axis no responde o el proyecto está con New Input System only)
        if (Mathf.Approximately(input, 0f))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input = 1f;
        }

        return input;
    }

    /// <summary>
    /// Movimiento horizontal robusto: desplaza el carriage a lo largo del segmento (en mundo)
    /// definido por LeftLimit -> RightLimit, sin asumir ejes locales X/Z.
    /// </summary>
    void TickHorizontalAlongSegment(float input)
    {
        if (!carriage || !leftLimit || !rightLimit) return;

        Vector3 a = leftLimit.position;
        Vector3 b = rightLimit.position;

        Vector3 axis = (b - a);
        float len = axis.magnitude;
        if (len < 0.0001f) return;

        Vector3 dir = axis / len;

        // Proyecta la posición actual del carriage sobre el eje del rail (a lo largo de dir)
        float t = Vector3.Dot((carriage.position - a), dir);

        // Avanza según input
        t += input * horizontalSpeed * Time.fixedDeltaTime;

        // Clamp al segmento
        t = Mathf.Clamp(t, 0f, len);

        Vector3 newPos = a + dir * t;

        // Si quieres conservar Y (altura) fija aunque los límites tengan distinta Y,
        // descomenta estas dos líneas:
        // newPos.y = carriage.position.y;

        carriage.position = newPos;
    }

    // Returns true if arrived.
    bool TickRailTo(float targetLocalY, float speed)
    {
        if (!railRig) return false;

        Vector3 lp = railRig.localPosition;
        float newY = Mathf.MoveTowards(lp.y, targetLocalY, speed * Time.fixedDeltaTime);
        lp.y = newY;

        // Si cambias a Rigidbody dinámico, aquí usarías MovePosition en mundo.
        railRig.localPosition = lp;

        return Mathf.Abs(lp.y - targetLocalY) <= arriveEpsilon;
    }

    void SetState(ClawState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(state);
        _stuckFrames = 0;
        if (railRig) _lastY = railRig.localPosition.y;
    }

    void WatchdogCheck()
    {
        if (!watchdogLog) return;
        if (state != ClawState.Raising && state != ClawState.Dropping) return;
        if (!railRig) return;

        float y = railRig.localPosition.y;
        if (Mathf.Abs(y - _lastY) < 0.00001f)
        {
            _stuckFrames++;
            if (_stuckFrames == 20) // ~0.4s a 50Hz
            {
                Debug.LogWarning(
                    $"[ClawController_RailY] Y no cambia durante {state}. Algo está sobrescribiendo railRig. " +
                    $"y={y} top={_topY} bottom={_bottomY} rb={(_rb ? "YES" : "NO")}"
                );
            }
        }
        else
        {
            _stuckFrames = 0;
            _lastY = y;
        }
    }
}
