using UnityEngine;

public class ElectrocutionZone : MonoBehaviour
{
    private bool lampInZone = false;

    public ElectricLamp lamp; // asigna en inspector
    private EnemyStateController enemy; // referencia al enemigo dentro de la zona

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemy = other.GetComponentInParent<EnemyStateController>();
            if (enemy != null)
            {
                enemy.SetInWaterByTrigger(true);
                Debug.Log("Enemy cayó al agua -> estado Angry, isInWater = true");

                // Si ya hay una lámpara en la zona, electrocuta inmediatamente
                if (lampInZone || (lamp != null && lamp.IsWaterElectrified()))
                {
                    enemy.ElectrocuteAndDie();
                    Debug.Log("Enemy electrocutado -> estado Dead");
                }
            }
        }
        else if (other.CompareTag("Lamp"))
        {
            lampInZone = true;
            Debug.Log("Lamp cayó al agua -> agua electrificada");

            // Desactivar la lámpara que cayó
            other.gameObject.SetActive(false);

            // Si hay enemigo dentro, electrocútalo
            if (enemy != null && enemy.isInWater)
            {
                enemy.ElectrocuteAndDie();
                Debug.Log("Enemy electrocutado por lamp -> estado Dead");
            }
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Enemy"))
    //     {
    //         var exitingEnemy = other.GetComponentInParent<EnemyStateController>();
    //         if (exitingEnemy != null)
    //         {
    //             exitingEnemy.SetInWaterByTrigger(false);
    //             Debug.Log("Enemy salió del agua -> isInWater = false");
    //         }

    //         // limpiar referencia si es el mismo enemigo
    //         if (exitingEnemy == enemy)
    //         {
    //             enemy = null;
    //         }
    //     }
    //     else if (other.CompareTag("Lamp"))
    //     {
    //         lampInZone = false;
    //         Debug.Log("Lamp salió del agua -> agua ya no electrificada");
    //     }
    // }
}