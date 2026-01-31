using UnityEngine;

public class EnemyPlatform : MonoBehaviour
{
    [Header("Refs")]
    public TargetHitTrigger targetTrigger;

    private void Awake()
    {
        if (targetTrigger != null)
        {
            // Suscribirse al evento
            targetTrigger.OnHit += RotatePlatform;
        }
        else
        {
            Debug.LogError("EnemyPlatform necesita referencia a TargetHitTrigger.");
        }
    }

    private void OnDestroy()
    {
        if (targetTrigger != null)
        {
            // Desuscribirse para evitar memory leaks
            targetTrigger.OnHit -= RotatePlatform;
        }
    }

    private void RotatePlatform()
    {
        //Debug.Log("EnemyPlatform recibió evento OnHit, rotando...");
        transform.Rotate(90f, 0f, 0f);
    }
}