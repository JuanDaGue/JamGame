using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bomb : MonoBehaviour
{
    [Header("Bomb")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Aim")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float aimAssistUp = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private float audioVolume = 0.8f;

    [Header("References")]
    [SerializeField] private Animator animator;

    [SerializeField] private AudioSource audioSource;

    private GameObject currentBomb;
    private Rigidbody bombRb;
    private Collider bombCollider;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    // ================= ANIMATION EVENTS =================

    // Event 1 – bomb appears and follows the hand
    public void AttachBombToHand()
    {
        if (currentBomb != null) return;

        currentBomb = Instantiate(bombPrefab, throwPoint.position, throwPoint.rotation);
        currentBomb.transform.SetParent(throwPoint);

        bombRb = currentBomb.GetComponent<Rigidbody>();
        bombCollider = currentBomb.GetComponent<Collider>();

        if (bombRb != null)
        {
            bombRb.isKinematic = true;
            bombRb.linearVelocity = Vector3.zero;
        }

        if (bombCollider != null)
            bombCollider.enabled = false;

        // 🔊 Grab sound
        PlaySound(grabClip);
    }

    // Event 2 – release frame
    public void ReleaseBomb()
    {
        if (currentBomb == null) return;

        currentBomb.transform.SetParent(null);

        if (bombRb != null)
        {
            bombRb.isKinematic = false;

            Vector3 direction =
                (aimPoint.position - throwPoint.position).normalized;

            Vector3 force =
                direction * throwForce +
                Vector3.up * upwardForce * aimAssistUp;

            bombRb.AddForce(force, ForceMode.Impulse);
        }

        if (bombCollider != null)
            bombCollider.enabled = true;

        // 🔊 Throw sound
        PlaySound(throwClip);

        currentBomb = null;
        bombRb = null;
        bombCollider = null;
    }

    // ================= AUDIO =================

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, audioVolume);
    }
}
