using UnityEngine;

public class PrizeSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Claw controller that fires OnReachedBottom.")]
    public ClawController_RailY clawController;

    [Tooltip("Table that decides which prize is spawned.")]
    public PrizeTable prizeTable;

    [Tooltip("Point where the prize will be instantiated (usually child of the claw).")]
    public Transform spawnPoint;

    [Header("Spawn Options")]
    [Tooltip("If true, the spawned prize will be parented to the spawnPoint.")]
    public bool parentToSpawnPoint = true;

    [Tooltip("Optional random offset (local) to avoid perfect alignment.")]
    public Vector3 randomLocalOffset;

    [Header("Debug")]
    public GameObject currentPrize;

    void Awake()
    {
        if (!clawController)
            clawController = FindObjectOfType<ClawController_RailY>();

        if (clawController)
            clawController.OnReachedBottom += HandleReachedBottom;
        else
            Debug.LogWarning("[PrizeSpawner] No ClawController_RailY found.");
    }

    void OnDestroy()
    {
        if (clawController)
            clawController.OnReachedBottom -= HandleReachedBottom;
    }

    void HandleReachedBottom()
    {
        SpawnPrize();
    }

    public void SpawnPrize()
    {
        if (!prizeTable || !spawnPoint)
        {
            Debug.LogWarning("[PrizeSpawner] Missing prizeTable or spawnPoint.");
            return;
        }

        // Clean previous prize if any
        if (currentPrize)
            Destroy(currentPrize);

        GameObject prefab = prizeTable.GetRandomPrize();
        if (!prefab)
            return;

        Vector3 position = spawnPoint.position;
        Quaternion rotation = spawnPoint.rotation;

        currentPrize = Instantiate(prefab, position, rotation);

        if (parentToSpawnPoint)
        {
            currentPrize.transform.SetParent(spawnPoint);
            currentPrize.transform.localPosition = Vector3.zero + randomLocalOffset;
            currentPrize.transform.localRotation = Quaternion.identity;
        }
    }

    // Optional helper if later you want to force-drop the prize
    public void ReleasePrize()
    {
        if (!currentPrize)
            return;

        currentPrize.transform.SetParent(null);

        Rigidbody rb = currentPrize.GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = false;

        currentPrize = null;
    }
}
