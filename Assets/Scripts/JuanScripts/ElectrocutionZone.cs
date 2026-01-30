using UnityEngine;

public class ElectrocutionZone : MonoBehaviour
{
    public ElectricLamp lamp; // asigna en inspector (o busca en runtime)

    private void OnTriggerStay(Collider other)
    {
        if (lamp == null) return;
        if (!lamp.IsWaterElectrified()) return;

        var enemy = other.GetComponentInParent<EnemyStateController>();
        if (enemy != null && enemy.isInWater)
        {
            enemy.ElectrocuteAndDie();
        }
    }
}
