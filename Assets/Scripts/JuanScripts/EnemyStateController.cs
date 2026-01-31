using System;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    public enum EnemyState { Waiting, Angry, Dead, Attack }
    public event  System.Action  OnAttack;

    [Header("Refs")]
    public Animator animator; // opcional

    [Header("State")]
    public EnemyState state = EnemyState.Waiting;
    public bool isInWater { get; private set; }

    [Header("Attack Timing")]
    public float timeToAttack = 15f; // segundos para pasar de Angry a Attack
    private float angryStartTime;

    void Start()
    {
        EnterWaiting();
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
        if (animator != null)
        {
            animator.Play("Idle");
        }
    }

    void EnterAngry()
    {
        if (state == EnemyState.Angry) return;
        state = EnemyState.Angry;
        angryStartTime = Time.time; // guardar el momento en que entró a Angry

        if (animator != null)
        {
            animator.Play("Angry");
        }
    }

    void Update()
    {
        // Solo si está Angry y no Dead, comprobar si ya pasaron los segundos
        if (state == EnemyState.Angry)
        {
            if (Time.time - angryStartTime >= timeToAttack)
            {
                EnterAttack();
            }
        }
    }

    void EnterDead()
    {
        state = EnemyState.Dead;
        if (animator != null)
        {
            animator.Play("Death");
        }
    }

    void EnterAttack()
    {
        // Solo puede entrar a Attack si sigue en Angry
        if (state != EnemyState.Angry) return;

        state = EnemyState.Attack;
        if (animator != null)
        {
            animator.Play("Attack");
        }

        OnAttack?.Invoke(); // Invocar el evento de ataque
        GameManager.Instance?.GameOver();
        Debug.Log("Enemy cambió de Angry a Attack después de 15s.");
    }
}