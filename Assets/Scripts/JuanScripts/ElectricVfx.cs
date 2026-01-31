using UnityEngine;

public class ElectricVfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem electricParticles;
   [SerializeField] private ElectricLamp electricLamp;
    private void Start()
    {
        // Subscribe to the electric event
        if (electricLamp != null)
        {
            electricLamp.OnElectrified += ActivateElectricEffect;
        }
        else
        {
            Debug.LogError("ElectricVfx necesita referencia a ElectricLamp.");
        }
    }

    // private void OnDisable()
    // {
    //     // Unsubscribe to avoid memory leaks
    //     if (electricLamp != null)
    //     {
    //         electricLamp.OnElectrified -= ActivateElectricEffect;
    //     }
    // }

    private void ActivateElectricEffect()
    {
        if (electricParticles != null)
        {
            Debug.Log("Activating electric particle effect.");
            
            electricParticles.Play();
        }
    }
}