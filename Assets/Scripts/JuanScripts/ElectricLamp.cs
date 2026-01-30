using UnityEngine;

public class ElectricLamp : MonoBehaviour
{
    [Header("Electrocution")]
    public float electrifyDuration = 2.5f;

    private bool waterElectrified;
    private float electrifyEndTime;

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
            ElectrifyWater();
        }

        // Si la lámpara entra al “electrocution zone”, también vale:
        // if (other.GetComponent<WaterTrigger>() != null) ElectrifyWater();
    }

    void ElectrifyWater()
    {
        waterElectrified = true;
        electrifyEndTime = Time.time + electrifyDuration;
    }

    // Llama esto desde un “ElectrocutionZone” dentro del agua (ver abajo)
    public bool IsWaterElectrified() => waterElectrified;
}
