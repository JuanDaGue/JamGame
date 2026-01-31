using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlingshotLauncher : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Tuning")]
    public float maxPower = 50f;       // Potencia máxima
    public float chargeTime = 3f;      // Tiempo máximo de carga en segundos
    public int trajectoryPoints = 30;  // cantidad de puntos para dibujar la trayectoria
    public float timeStep = 0.1f;      // intervalo de simulación

    private bool charging;
    private float currentCharge;
    private float chargeStartTime;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            chargeStartTime = Time.time;
            currentCharge = 0f;
        }

        if (charging)
        {
            float elapsed = Time.time - chargeStartTime;
            currentCharge = Mathf.Clamp((elapsed / chargeTime) * maxPower, 0f, maxPower);

            // Dibujar trayectoria mientras se carga
            Vector3 dir = (Camera.main.transform.forward + Camera.main.transform.up * 0.3f).normalized;
            ShowTrajectory(firePoint.position, dir * currentCharge);

            if (elapsed >= chargeTime)
            {
                charging = false;
                Fire(dir, currentCharge);
                ClearTrajectory();
            }
        }

        if (Input.GetMouseButtonUp(0) && charging)
        {
            charging = false;
            Vector3 dir = (Camera.main.transform.forward + Camera.main.transform.up * 0.3f).normalized;
            Fire(dir, currentCharge);
            ClearTrajectory();
        }
    }

    void Fire(Vector3 dir, float power)
    {
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        go.tag = "projectile";

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Destroy(go);
            return;
        }

        rb.linearVelocity = dir.normalized * power;
    }

    void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        lineRenderer.positionCount = trajectoryPoints;
        Vector3 currentPos = startPos;
        Vector3 currentVelocity = startVelocity;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            lineRenderer.SetPosition(i, currentPos);
            currentVelocity += Physics.gravity * timeStep;
            currentPos += currentVelocity * timeStep;
        }
    }

    void ClearTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}