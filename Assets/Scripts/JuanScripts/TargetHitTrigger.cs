using UnityEngine;
//using UnityEngine.Events;

public class TargetHitTrigger : MonoBehaviour
{
    public event System.Action OnHit;
    [SerializeField] private bool singleUse = true;
    

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used && singleUse) return;

        if (other.CompareTag("projectile"))
        {
            Debug.Log("Target hit by projectile!");
            used = true;
            OnHit?.Invoke();
        }
    }

    // Si prefieres colisión normal:
    private void OnCollisionEnter(Collision collision)
    {
        if (used && singleUse) return;
        if (collision.collider.CompareTag("projectile"))
        {
            Debug.Log("Target hit by projectile!");
            used = true;
            OnHit?.Invoke();
        }
    }
}
