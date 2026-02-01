using System;
using System.Collections;
using GameJam.MiniGames;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    public enum EnemyState { Waiting, Angry, Dead, Attack }
    public event Action OnAttack;

    [Header("Refs")]
    [SerializeField] private Animator animator; // opcional

    [Header("State")]
    [SerializeField] private EnemyState state = EnemyState.Waiting;
    public EnemyState State => state;
    public MinigameController minigameController; // referencia al minijuego asociado
    public bool isInWater { get; private set; }

    [Header("Attack Timing")]
    [SerializeField] private float timeToAttack = 15f; // segundos para pasar de Angry a Attack
    private float angryStartTime;
    public TargetHitTrigger targetTrigger;

    void Start()
    {
        EnterWaiting();
        if(minigameController == null)
        {
            minigameController = FindFirstObjectByType<MinigameController>();
        }
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        animator.SetBool("IsInIdle", true);
        animator.SetBool("IsWalking", false);
        targetTrigger.OnHit += FallingTriggeredAnimtion;
    }

    private void FallingTriggeredAnimtion()
    {
        animator.SetTrigger("Falling");
        targetTrigger.OnHit -= FallingTriggeredAnimtion;
    }
    void OnDisable()
    {
        targetTrigger.OnHit -= FallingTriggeredAnimtion;
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
            animator.SetBool("IsInIdle", true);
        }
    }

    void EnterAngry()
    {
        if (state == EnemyState.Angry) return;
        state = EnemyState.Angry;
        angryStartTime = Time.time; // guardar el momento en que entró a Angry

        if (animator != null)
        {
            animator.SetTrigger("Angry");
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
        // if (animator != null)
        // {
        //     animator.Play("Death");
        // }
        StartCoroutine(WaitAndDie(3f,true)); // esperar 2 segundos antes de llamar a WinGame
        Debug.Log("Enemy murió electrocutado.");
    }

    void EnterAttack()
    {
        // Solo puede entrar a Attack si sigue en Angry
        if (state != EnemyState.Angry) return;

        state = EnemyState.Attack;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        OnAttack?.Invoke(); // Invocar el evento de ataque
        //GameManager.Instance?.GameOver();
        StartCoroutine(WaitAndDie(3f,false)); // esperar 3 segundos antes de llamar a LoseGame
        Debug.Log("Enemy cambió de Angry a Attack después de 15s.");
    }
    IEnumerator WaitAndDie(float delay, bool won = false)
    {
        yield return new WaitForSeconds(delay);
        if (won)
        {
            minigameController?.WinGame();
        }
        else
        {
            minigameController?.LoseGame();
        }
    }
}