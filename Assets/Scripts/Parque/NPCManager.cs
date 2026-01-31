using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    [Header("NPCs to manage")]
    public List<NPCAgent> npcs = new List<NPCAgent>();

    [Header("Available routes")]
    public List<Route> routes = new List<Route>();

    public enum AssignMode { ByIndex, RoundRobin, Random }
    public AssignMode assignMode = AssignMode.RoundRobin;

    [Tooltip("If true, assigns routes on Start.")]
    public bool assignOnStart = true;

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

            npcs[i].route = chosen;
        }
    }

    [ContextMenu("Auto-Find NPCs & Routes (Scene)")]
    public void AutoFind()
    {
        npcs.Clear();
        routes.Clear();

        npcs.AddRange(FindObjectsOfType<NPCAgent>(true));
        routes.AddRange(FindObjectsOfType<Route>(true));
    }
}
