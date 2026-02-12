using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    [Header("Configuracion")]
    public Animator animatorBarra;

    public void SetTrigger()
    {
        if (animatorBarra != null)
        {
            animatorBarra.SetTrigger("TriggerGolpeDebil");
        }
    }
}
