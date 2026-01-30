using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    public enum EnemyState { Waiting, Angry, Dead }

    [Header("Refs")]
    public Rigidbody rb;
    public Animator animator; // opcional
    public Transform poolEdgePoint;

    [Header("Water Detection")]
    public float waterLevelY = 0f; // ajusta al nivel del agua si usas detección por altura
    public bool useHeightToDetectWater = true;

    [Header("State")]
    public EnemyState state = EnemyState.Waiting;
    public bool isInWater { get; private set; }

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        EnterWaiting();
    }

    void Update()
    {
        if (state == EnemyState.Dead) return;

        if (useHeightToDetectWater)
        {
            bool nowInWater = transform.position.y <= waterLevelY;
            if (nowInWater && !isInWater)
            {
                isInWater = true;
                if (state != EnemyState.Dead) EnterAngry();
            }
            else if (!nowInWater && isInWater)
            {
                isInWater = false;
            }
        }
    }

    public void TriggerFallIntoPool()
    {
        if (state == EnemyState.Dead) return;

        // Aquí puedes soltar constraints, activar física, o empujar.
        // Ejemplo: permitir que caiga si estaba kinematic.
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // un empujón opcional hacia la piscina
            rb.AddForce(transform.forward * 2f, ForceMode.VelocityChange);
        }
    }

    public void SetInWaterByTrigger(bool inWater)
    {
        isInWater = inWater;
        if (inWater && state != EnemyState.Dead)
        {
            EnterAngry();
        }
    }

    public void ElectrocuteAndDie()
    {
        if (state == EnemyState.Dead) return;
        EnterDead();
    }

    void EnterWaiting()
    {
        state = EnemyState.Waiting;

        if (poolEdgePoint != null)
        {
            transform.position = poolEdgePoint.position;
            transform.rotation = poolEdgePoint.rotation;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // esperando “quieto” al borde
            rb.useGravity = false;
        }

        if (animator != null)
        {
            animator.Play("Idle"); // nómbralo como quieras
        }
    }

    void EnterAngry()
    {
        if (state == EnemyState.Angry) return;
        state = EnemyState.Angry;

        if (animator != null)
        {
            animator.Play("Angry"); // o reacción en agua
        }

        // Aquí puedes iniciar un timer, sonido, partículas, etc.
    }

    void EnterDead()
    {
        state = EnemyState.Dead;

        if (animator != null)
        {
            animator.Play("Death");
        }

        // Puedes congelar o dejar ragdoll, depende del estilo.
        // Ejemplo: dejarlo caer pero sin control
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
