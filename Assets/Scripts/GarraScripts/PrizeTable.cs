using UnityEngine;

[CreateAssetMenu(
    fileName = "PrizeTable",
    menuName = "GameJam/Claw Machine/Prize Table",
    order = 1)]
public class PrizeTable : ScriptableObject
{
    [System.Serializable]
    public class PrizeEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float probability = 0.5f;
    }

    [Header("Prizes")]
    [Tooltip("Usually 2 prizes: one 'good' and one 'bad'")]
    public PrizeEntry[] prizes;

    [Header("Debug")]
    [SerializeField] bool normalizeProbabilities = true;

    void OnValidate()
    {
        if (!normalizeProbabilities || prizes == null || prizes.Length == 0)
            return;

        float total = 0f;
        for (int i = 0; i < prizes.Length; i++)
            total += prizes[i].probability;

        if (total <= 0f)
            return;

        for (int i = 0; i < prizes.Length; i++)
            prizes[i].probability /= total;
    }

    public GameObject GetRandomPrize()
    {
        if (prizes == null || prizes.Length == 0)
        {
            Debug.LogWarning("[PrizeTable] No prizes configured.");
            return null;
        }

        float roll = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < prizes.Length; i++)
        {
            cumulative += prizes[i].probability;
            if (roll <= cumulative)
                return prizes[i].prefab;
        }

        // Fallback (por errores de redondeo)
        return prizes[prizes.Length - 1].prefab;
    }
}
