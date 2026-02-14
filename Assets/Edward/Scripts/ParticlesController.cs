using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private ParticleSystem particleWarp;
    [SerializeField] private ParticleSystem particleConfeti;

    public void ActivarParticulasWarp()
    {
        if (particleWarp != null)
        {
            particleWarp.Play();
        }
    }

    public void DetenerParticulasWarp()
    {
        if (particleWarp != null)
        {
            particleWarp.Stop();
        }
    }

    public void ActivarParticulasConfeti()
    {
        if (particleConfeti != null)
        {
            particleConfeti.Play();
        }
    }

    public void DetenerParticulasConfeti()
    {
        if (particleConfeti != null)
        {
            particleConfeti.Stop();
        }
    }
}
