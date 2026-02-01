using UnityEngine;

public class ElectricLamp : MonoBehaviour
{
    [Header("Electrocution")]
    [SerializeField] private float electrifyDuration = 2.5f;

    private bool waterElectrified;
    private float electrifyEndTime;
    private Rigidbody rb;
    public event System.Action OnElectrified;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

    }
    private void Update()
    {
        if (waterElectrified && Time.time >= electrifyEndTime)
        {
            waterElectrified = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) // ponle tag "Water" al trigger/volumen del agua
        {
            OnElectrified?.Invoke();
            ElectrifyWater();
            Debug.Log("La lámpara ha sido electrificada al entrar en el agua.");
        }

        // Si la lámpara entra al “electrocution zone”, también vale:
        // if (other.GetComponent<WaterTrigger>() != null) ElectrifyWater();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("projectile")) // ponle tag "Water" al trigger/volumen del agu
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            Debug.Log("La lámpara ha sido derribada por un proyectil.");
        }
    }

    void ElectrifyWater()
    {
        waterElectrified = true;
        electrifyEndTime = Time.time + electrifyDuration;

    }

    // Llama esto desde un “ElectrocutionZone” dentro del agua (ver abajo)
    public bool IsWaterElectrified() => waterElectrified;

}
