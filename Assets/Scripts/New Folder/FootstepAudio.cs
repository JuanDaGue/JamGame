using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepVolume = 0.7f;

    [Header("Jump / Land")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    private AudioSource audioSource;

private void Awake()
{
    audioSource = GetComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = null; // ignora el grupo Sfx
}



    // === ANIMATION EVENTS ===

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip, stepVolume);
        Debug.Log("Footstep sound played: " + clip.name);
    }

    public void PlayJump()
    {
        if (jumpClip == null) return;
        audioSource.PlayOneShot(jumpClip, stepVolume);
        Debug.Log("Jump sound played: " + jumpClip.name);
    }

    public void PlayLand()
    {
        if (landClip == null) return;
        audioSource.PlayOneShot(landClip, stepVolume);
    }
}
