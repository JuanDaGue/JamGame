using UnityEngine;

public class PlayerPlatform : MonoBehaviour
{
    [Header("Refs")]
    public EnemyStateController targetTrigger;

    private void Awake()
    {
        if (targetTrigger != null)
        {
            // Suscribirse al evento
            targetTrigger.OnAttack += RotatePlatform;
        }
        else
        {
            Debug.LogError("PlayerPlatform necesita referencia a EnemyStateController.");
        }
    }

    private void OnDestroy()
    {
        if (targetTrigger != null)
        {
            // Desuscribirse para evitar memory leaks
            targetTrigger.OnAttack -= RotatePlatform;
        }
    }

    private void RotatePlatform()
    {
        //Debug.Log("EnemyPlatform recibió evento OnHit, rotando...");
        transform.Rotate(90f, 0f, 0f);
    }
}