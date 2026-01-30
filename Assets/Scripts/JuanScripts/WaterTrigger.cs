using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponentInParent<EnemyStateController>();
        if (enemy != null) enemy.SetInWaterByTrigger(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var enemy = other.GetComponentInParent<EnemyStateController>();
        if (enemy != null) enemy.SetInWaterByTrigger(false);
    }
}
