using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SlingshotLauncher : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Tuning")]
    public float maxPower = 50f;       // Potencia máxima
    public float chargeTime = 3f;      // Tiempo máximo de carga en segundos

    private bool charging;
    private float currentCharge;
    private float chargeStartTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            chargeStartTime = Time.time;
            currentCharge = 0f;
            //Debug.Log("Comenzó la carga de disparo.");
        }

        if (charging)
        {
            float elapsed = Time.time - chargeStartTime;
            // Escalar la potencia según el tiempo transcurrido
            currentCharge = Mathf.Clamp((elapsed / chargeTime) * maxPower, 0f, maxPower);

            //Debug.Log($"Cargando... Tiempo: {elapsed:F2}s, Potencia: {currentCharge:F2}");

            // Si llega al tiempo máximo, dispara automáticamente
            if (elapsed >= chargeTime)
            {
                charging = false;
                Vector3 dir = (Camera.main.transform.forward + Camera.main.transform.up * 0.3f).normalized;
                Fire(dir, currentCharge);
                //Debug.Log("Disparo automático por tiempo máximo.");
            }
        }

        if (Input.GetMouseButtonUp(0) && charging)
        {
            charging = false;
            Fire(Camera.main.transform.forward, currentCharge);
            //Debug.Log("Disparo al soltar el mouse.");
        }
    }

    void Fire(Vector3 dir, float power)
    {
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        go.tag = "projectile";

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            //Debug.LogError("Projectile prefab necesita un Rigidbody.");
            Destroy(go);
            return;
        }

        rb.linearVelocity = dir.normalized * power;
        //Debug.Log($"Disparo con potencia {power:F2}, dirección {dir}, velocidad aplicada {rb.linearVelocity}");
    }
}