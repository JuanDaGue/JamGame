using UnityEngine;

public class Route : MonoBehaviour
{
    public Transform[] waypoints;

    void Reset()
    {
        CacheFromChildren();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Auto-cache if empty
        if (waypoints == null || waypoints.Length == 0)
            CacheFromChildren();
    }
#endif

    public void CacheFromChildren()
    {
        int n = transform.childCount;
        waypoints = new Transform[n];
        for (int i = 0; i < n; i++)
            waypoints[i] = transform.GetChild(i);
    }

    public int Count => waypoints != null ? waypoints.Length : 0;

    public Transform Get(int index)
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        return waypoints[index];
    }
}
