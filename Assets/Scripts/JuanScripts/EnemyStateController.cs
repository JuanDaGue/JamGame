using System;
using System.Collections;
using GameJam.MiniGames;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyStateController : MonoBehaviour
{
    public enum EnemyState { Waiting, Angry, Dead, Attack }
    public event Action OnAttack;

    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Header("State")]
    [SerializeField] private EnemyState state = EnemyState.Waiting;
    public EnemyState State => state;

    public MinigameController minigameController;
    public bool isInWater { get; private set; }

    [Header("Attack Timing")]
    [SerializeField] private float timeToAttack = 15f;
    private float angryStartTime;

    [Header("Triggers")]
    public TargetHitTrigger targetTrigger;

    [Header("Audio")]
    [SerializeField] private AudioClip idleLoop;
    [SerializeField] private AudioClip angryClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip fallingClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private float audioVolume = 0.8f;

    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // sonido 3D

        EnterWaiting();

        if (minigameController == null)
            minigameController = FindFirstObjectByType<MinigameController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetBool("IsInIdle", true);
        animator.SetBool("IsWalking", false);

        if (targetTrigger != null)
            targetTrigger.OnHit += FallingTriggeredAnimtion;
    }

    private void OnDisable()
    {
        if (targetTrigger != null)
            targetTrigger.OnHit -= FallingTriggeredAnimtion;
    }

    // ================= STATES =================

    void EnterWaiting()
    {
        state = EnemyState.Waiting;
        animator?.SetBool("IsInIdle", true);
        PlayLoop(idleLoop);
    }

    void EnterAngry()
    {
        if (state == EnemyState.Angry) return;

        state = EnemyState.Angry;
        angryStartTime = Time.time;

        animator?.SetTrigger("Angry");
        PlayOneShot(angryClip);
    }

    void EnterAttack()
    {
        if (state != EnemyState.Angry) return;

        state = EnemyState.Attack;
        animator?.SetTrigger("Attack");

        PlayOneShot(attackClip);
        OnAttack?.Invoke();

        StartCoroutine(WaitAndDie(3f, false));
    }

    void EnterDead()
    {
        if (state == EnemyState.Dead) return;

        state = EnemyState.Dead;

        StopLoop();
        PlayOneShot(deathClip);

        StartCoroutine(WaitAndDie(3f, true));
        Debug.Log("Enemy murió electrocutado.");
    }

    // ================= EVENTS =================

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
        EnterDead();
    }

    private void FallingTriggeredAnimtion()
    {
        animator?.SetTrigger("Falling");
        PlayOneShot(fallingClip);

        if (targetTrigger != null)
            targetTrigger.OnHit -= FallingTriggeredAnimtion;
    }

    // ================= UPDATE =================

    void Update()
    {
        if (state == EnemyState.Angry)
        {
            if (Time.time - angryStartTime >= timeToAttack)
            {
                EnterAttack();
            }
        }
    }

    // ================= AUDIO =================

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, audioVolume);
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = audioVolume;
        audioSource.Play();
    }

    private void StopLoop()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    // ================= END =================

    IEnumerator WaitAndDie(float delay, bool won)
    {
        yield return new WaitForSeconds(delay);

        if (won)
            minigameController?.WinGame();
        else
            minigameController?.LoseGame();
    }
}
