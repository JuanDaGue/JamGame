using UnityEngine;

public class SemiCircularMover : MonoBehaviour
{
    [Header("Arc")]
    [SerializeField] private Transform center;
    [SerializeField] private float radius = 2f;
    [Tooltip("Semicírculo: 180 grados. Ej: -90 a +90")]
    [SerializeField] private float startAngleDeg = -90f;
    [SerializeField] private float endAngleDeg = 90f;

    [Header("Speed Variation")]
    [SerializeField] private float baseSpeed = 1f;         // velocidad angular base (ciclos por segundo aprox)
    [SerializeField] private float speedVariance = 0.5f;    // cuánto varía
    [SerializeField] private float varianceFrequency = 0.7f;// qué tan rápido cambia la velocidad

    private float t; // 0..1
    private int dir = 1;

    void Update()
    {
        if (center == null) return;

        float speedFactor = baseSpeed + Mathf.PerlinNoise(Time.time * varianceFrequency, 0.123f) * speedVariance;
        t += Time.deltaTime * speedFactor * dir;

        if (t >= 1f) { t = 1f; dir = -1; }
        if (t <= 0f) { t = 0f; dir = 1; }

        float angle = Mathf.Lerp(startAngleDeg, endAngleDeg, t) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        transform.position = center.position + offset;
    }
}
