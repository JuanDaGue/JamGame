using UnityEngine;
using GameJam.MiniGames;

public class ClawMinigameOutcome : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Claw controller que emite OnReachedTop.")]
    public ClawController_RailY clawController;

    [Tooltip("Spawner que mantiene referencia al premio actual (currentPrize).")]
    public PrizeSpawner prizeSpawner;

    [Tooltip("Conector al sistema global (recompensa + volver al Hub).")]
    public MinigameController minigameController;

    [Header("Outcome Rules")]
    [Tooltip("Prefab que cuenta como victoria (ej. Bueno). Arrastra el prefab exacto.")]
    public GameObject goodPrizePrefab;

    [Tooltip("Prefab que cuenta como derrota (ej. Malo). Arrastra el prefab exacto.")]
    public GameObject badPrizePrefab;

    [Tooltip("Delay opcional para dejar que el jugador vea el resultado al llegar arriba.")]
    public float resolveDelaySeconds = 0.35f;

    [Header("Debug")]
    public bool logOutcome = true;

    bool _resolvedThisDrop = false;

    void Awake()
    {
        if (!clawController) clawController = FindFirstObjectByType<ClawController_RailY>();
        if (!prizeSpawner) prizeSpawner = FindFirstObjectByType<PrizeSpawner>();
        if (!minigameController) minigameController = FindFirstObjectByType<MinigameController>();

        if (clawController != null)
        {
            clawController.OnReachedBottom += HandleReachedBottom;
            clawController.OnReachedTop += HandleReachedTop;
        }
        else
        {
            Debug.LogWarning("[ClawMinigameOutcome] No se encontr� ClawController_RailY.");
        }
    }

    void OnDestroy()
    {
        if (clawController != null)
        {
            clawController.OnReachedBottom -= HandleReachedBottom;
            clawController.OnReachedTop -= HandleReachedTop;
        }
    }

    void HandleReachedBottom()
    {
        // Se inicia un nuevo ciclo (spawn ocurre al llegar al fondo)
        _resolvedThisDrop = false;
    }

    void HandleReachedTop()
    {
        if (_resolvedThisDrop) return;
        _resolvedThisDrop = true;

        if (resolveDelaySeconds > 0f)
            Invoke(nameof(ResolveNow), resolveDelaySeconds);
        else
            ResolveNow();
    }

    void ResolveNow()
    {
        if (minigameController == null)
        {
            Debug.LogWarning("[ClawMinigameOutcome] Falta MinigameController en la escena.");
            return;
        }

        if (prizeSpawner == null || prizeSpawner.CurrentPrize == null)
        {
            // Si no hay premio por alg�n motivo, tr�talo como derrota (o cambia a retry si prefieres).
            if (logOutcome) Debug.Log("[ClawMinigameOutcome] No hay premio instanciado -> LOSE.");
            minigameController.LoseGame();
            return;
        }

        GameObject spawned = prizeSpawner.CurrentPrize;

        // Comparaci�n robusta: puede ser instancia (Clone), entonces comparamos por nombre base o por prefab reference si coincide.
        // Primero intentamos por prefab exacto: si parentas el premio, seguir� siendo instancia, no el prefab.
        // As� que la forma m�s fiable aqu� es comparar por nombre del prefab.
        bool isGood = IsSamePrefabByName(spawned, goodPrizePrefab);
        bool isBad = IsSamePrefabByName(spawned, badPrizePrefab);

        if (!isGood && !isBad)
        {
            // Fallback: intenta por nombre literal si el prefab no est� asignado
            string n = spawned.name;
            if (n.Contains("Bueno")) isGood = true;
            else if (n.Contains("Malo")) isBad = true;
        }

        if (isGood)
        {
            if (logOutcome) Debug.Log("[ClawMinigameOutcome] Premio BUENO -> WIN.");
            minigameController.WinGame();
            return;
        }

        if (isBad)
        {
            if (logOutcome) Debug.Log("[ClawMinigameOutcome] Premio MALO -> LOSE.");
            minigameController.LoseGame();
            return;
        }

        // Si no pudimos clasificar, mejor no bloquear: trata como derrota (o c�mbialo a retry).
        if (logOutcome) Debug.LogWarning($"[ClawMinigameOutcome] Premio no reconocido ({spawned.name}) -> LOSE por defecto.");
        minigameController.LoseGame();
    }

    static bool IsSamePrefabByName(GameObject instance, GameObject prefab)
    {
        if (instance == null || prefab == null) return false;

        // instance.name suele venir como "Bueno(Clone)".
        string inst = instance.name.Replace("(Clone)", "").Trim();
        string pref = prefab.name.Trim();

        return inst == pref;
    }
}
