using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("NPCs to manage")]
    [SerializeField] private List<NPCAgent> npcs = new List<NPCAgent>();

    [Header("Available routes")]
    [SerializeField] private List<Route> routes = new List<Route>();

    public enum AssignMode { ByIndex, RoundRobin, Random }
    [SerializeField] private AssignMode assignMode = AssignMode.RoundRobin;

    [Tooltip("If true, assigns routes on Start.")]
    [SerializeField] private bool assignOnStart = true;

    void Start()
    {
        if (assignOnStart)
            AssignRoutes();
    }

    [ContextMenu("Assign Routes")]
    public void AssignRoutes()
    {
        if (npcs == null || npcs.Count == 0)
        {
            Debug.LogWarning("[NPCManager] No NPCs assigned.");
            return;
        }
        if (routes == null || routes.Count == 0)
        {
            Debug.LogWarning("[NPCManager] No routes assigned.");
            return;
        }

        int rr = 0;
        for (int i = 0; i < npcs.Count; i++)
        {
            if (!npcs[i]) continue;

            Route chosen = null;

            switch (assignMode)
            {
                case AssignMode.ByIndex:
                    chosen = routes[Mathf.Clamp(i, 0, routes.Count - 1)];
                    break;

                case AssignMode.RoundRobin:
                    chosen = routes[rr % routes.Count];
                    rr++;
                    break;

                case AssignMode.Random:
                    chosen = routes[Random.Range(0, routes.Count)];
                    break;
            }

            npcs[i].Route = chosen;
        }
    }

    [ContextMenu("Auto-Find NPCs & Routes (Scene)")]
    public void AutoFind()
    {
        npcs.Clear();
        routes.Clear();

        npcs.AddRange(FindObjectsByType<NPCAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        routes.AddRange(FindObjectsByType<Route>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }
}
