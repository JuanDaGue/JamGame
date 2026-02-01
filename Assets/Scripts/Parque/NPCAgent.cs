using UnityEngine;

public class NPCAgent : MonoBehaviour
{
    public enum PathMode { Loop, PingPong }

    [Header("Route")]
    [SerializeField] private Route route;
    public Route Route { get => route; set => route = value; }
    [SerializeField] private PathMode pathMode = PathMode.Loop;
    [SerializeField] private int startIndex = 0;
    [SerializeField] private float arriveDistance = 0.2f;

    [Header("Movement (Transform)")]
    [SerializeField] private float moveSpeed = 1.6f;     // units/sec
    [SerializeField] private float turnSpeed = 360f;     // degrees/sec

    [Header("Stops (each waypoint)")]
    [SerializeField] private Vector2 waitSecondsRange = new Vector2(0.2f, 1.2f);

    [Header("PingPong End Pause (only at ends)")]
    [Tooltip("Extra random pause before turning around at the ends (PingPong only).")]
    [SerializeField] private Vector2 endWaitSecondsRange = new Vector2(0.5f, 2.0f);

    [Header("Start Placement")]
    [SerializeField] private bool snapToStartWaypoint = true;
    [SerializeField] private bool keepCurrentYOnSnap = true;
    [SerializeField] private bool faceNextOnStart = true;

    int _index;
    int _dir = 1;
    float _waitTimer;

    void Start()
    {
        _index = Mathf.Max(0, startIndex);
        _waitTimer = 0f;

        if (!route || route.Count == 0) return;

        _index = Mathf.Clamp(_index, 0, route.Count - 1);

        if (snapToStartWaypoint)
            SnapToWaypoint(_index);

        if (faceNextOnStart)
        {
            int lookIndex = _index;
            if (route.Count > 1)
                lookIndex = GetNextIndexPreview(_index, _dir);

            Transform wp = route.Get(lookIndex);
            if (wp) FaceTowards(wp.position);
        }
    }

    void Update()
    {
        if (!route || route.Count == 0) return;

        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        Transform wp = route.Get(_index);
        if (!wp) return;

        Vector3 target = wp.position;
        Vector3 pos = transform.position;

        // Keep NPC on same Y for flat lobbies
        target.y = pos.y;

        float dist = Vector3.Distance(pos, target);
        if (dist <= arriveDistance)
        {
            bool reachedEndAndTurned = AdvanceIndexAndDetectTurnaround();

            // Normal waypoint pause
            _waitTimer = Random.Range(waitSecondsRange.x, waitSecondsRange.y);

            // Extra pause ONLY when we hit an end and are about to go back (PingPong)
            if (reachedEndAndTurned)
                _waitTimer += Random.Range(endWaitSecondsRange.x, endWaitSecondsRange.y);

            return;
        }

        // Rotate towards target
        Vector3 dir = (target - pos);
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
        }

        // Move directly (no physics)
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(pos, target, step);
    }

    // Returns true if we hit an end in PingPong and reversed direction this frame.
    bool AdvanceIndexAndDetectTurnaround()
    {
        if (route.Count <= 1) return false;

        if (pathMode == PathMode.Loop)
        {
            _index = (_index + 1) % route.Count;
            return false;
        }

        int next = _index + _dir;

        // End reached -> reverse direction and "wait before returning"
        if (next >= route.Count)
        {
            _dir = -1;
            _index = Mathf.Max(0, route.Count - 2);
            return true;
        }

        if (next < 0)
        {
            _dir = 1;
            _index = Mathf.Min(route.Count - 1, 1);
            return true;
        }

        _index = next;
        return false;
    }

    int GetNextIndexPreview(int index, int dir)
    {
        if (route.Count <= 1) return index;

        if (pathMode == PathMode.Loop)
            return (index + 1) % route.Count;

        int next = index + dir;
        if (next >= route.Count) return Mathf.Max(0, route.Count - 2);
        if (next < 0) return Mathf.Min(route.Count - 1, 1);
        return Mathf.Clamp(next, 0, route.Count - 1);
    }

    void SnapToWaypoint(int waypointIndex)
    {
        Transform wp = route.Get(waypointIndex);
        if (!wp) return;

        Vector3 p = wp.position;
        if (keepCurrentYOnSnap)
            p.y = transform.position.y;

        transform.position = p;
    }

    void FaceTowards(Vector3 worldPos)
    {
        Vector3 pos = transform.position;
        Vector3 to = worldPos - pos;
        to.y = 0f;

        if (to.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
    }
}
