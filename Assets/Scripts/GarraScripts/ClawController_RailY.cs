using System;
using UnityEngine;

public class ClawController_RailY : MonoBehaviour
{
    public enum ClawState { Move, Dropping, Raising, Cooldown }

    [Header("Rig References")]
    [SerializeField] private Transform railRig;     // Moves in local Y
    [SerializeField] private Transform carriage;    // Moves in local X (child of railRig)

    [Header("Horizontal Limits (children of railRig)")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("Vertical Setup (explicit local Y)")]
    [SerializeField] private float topLocalY = 0f;
    [SerializeField] private float bottomLocalY = -2f;

    [Header("Speeds")]
    [SerializeField] private float horizontalSpeed = 4f;
    [SerializeField] private float dropSpeed = 6f;
    [SerializeField] private float raiseSpeed = 2f;

    [Header("Input")]
    [SerializeField] private KeyCode dropKey = KeyCode.Mouse0;
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private bool allowKeyboardAxis = true;

    [Header("Tuning")]
    [SerializeField] private float arriveEpsilon = 0.001f;
    [SerializeField] private float cooldownSeconds = 0.2f;

    [Header("Physics / Conflicts")]
    [Tooltip("If railRig has Rigidbody, force it to kinematic to avoid physics fighting the script.")]
    [SerializeField] private bool forceKinematicIfRigidbody = true;

    [Tooltip("Debug: warns if Y stops changing while in Raising/Dropping.")]
    [SerializeField] private bool watchdogLog = false;

    [Header("Debug")]
    [SerializeField] private ClawState state = ClawState.Move;
    public ClawState State => state;

    public event Action OnReachedBottom;
    public event Action OnReachedTop;
    public event Action<ClawState> OnStateChanged;

    float _cooldownTimer;
    float _topY, _bottomY;

    Rigidbody _rb;
    float _lastY;
    int _stuckFrames;

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
        if (_bottomY > _topY)
        {
            float t = _topY;
            _topY = _bottomY;
            _bottomY = t;
        }
    }

    void Update()
    {
        // Horizontal in Update is fine (no physics usually on carriage)
        if (state == ClawState.Move)
        {
            TickHorizontalMove();
            if (Input.GetKeyDown(dropKey))
                SetState(ClawState.Dropping);
        }
    }

    void FixedUpdate()
    {
        // Vertical in FixedUpdate to be physics-safe (even if no Rigidbody, it still works)
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

    void TickHorizontalMove()
    {
        if (!carriage || !leftLimit || !rightLimit) return;

        float input = 0f;
        if (allowKeyboardAxis && !string.IsNullOrEmpty(horizontalAxis))
            input = Input.GetAxisRaw(horizontalAxis);

        Vector3 lp = carriage.localPosition;
        float targetX = lp.x + input * horizontalSpeed * Time.deltaTime;

        float minX = Mathf.Min(leftLimit.localPosition.x, rightLimit.localPosition.x);
        float maxX = Mathf.Max(leftLimit.localPosition.x, rightLimit.localPosition.x);
        lp.x = Mathf.Clamp(targetX, minX, maxX);

        carriage.localPosition = lp;
    }

    // Returns true if arrived.
    bool TickRailTo(float targetLocalY, float speed)
    {
        Vector3 lp = railRig.localPosition;
        float newY = Mathf.MoveTowards(lp.y, targetLocalY, speed * Time.fixedDeltaTime);
        lp.y = newY;

        // If you ever switch to non-kinematic rigidbody movement, do it here with MovePosition (world space).
        railRig.localPosition = lp;

        return Mathf.Abs(lp.y - targetLocalY) <= arriveEpsilon;
    }

    void SetState(ClawState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(state);
        _stuckFrames = 0;
        _lastY = railRig.localPosition.y;
    }

    void WatchdogCheck()
    {
        if (!watchdogLog) return;
        if (state != ClawState.Raising && state != ClawState.Dropping) return;

        float y = railRig.localPosition.y;
        if (Mathf.Abs(y - _lastY) < 0.00001f)
        {
            _stuckFrames++;
            if (_stuckFrames == 20) // ~0.4s at 50Hz
            {
                Debug.LogWarning($"[ClawController_RailY] Y is not changing while {state}. Something else is overriding railRig. y={y} top={_topY} bottom={_bottomY} rb={(_rb ? "YES" : "NO")}");
            }
        }
        else
        {
            _stuckFrames = 0;
            _lastY = y;
        }
    }
}
