using UnityEngine;

public class NPCDeathAnimator : MonoBehaviour
{
    [Header("Animator Lookup")]
    [Tooltip("Si el Animator está en un hijo, actívalo.")]
    [SerializeField] private bool searchInChildren = true;

    [Header("Mode")]
    [Tooltip("Si true usa Trigger, si false usa Bool.")]
    [SerializeField] private bool useTrigger = false;

    [Tooltip("Nombre del bool para Explotion (si useTrigger = false).")]
    [SerializeField] private string explotionBool = "IsExplotion";

    [Tooltip("Nombre del trigger para Explotion (si useTrigger = true).")]
    [SerializeField] private string explotionTrigger = "Explotion";

    [Header("Optional Hard Force")]
    [Tooltip("Si lo pones, fuerza el estado por nombre (CrossFade) además del parámetro.")]
    [SerializeField] private bool forceStateByName = false;

    [Tooltip("Nombre exacto del estado en el Animator (ej: 'Explotion' o 'Base Layer.Explotion').")]
    [SerializeField] private string explotionStateName = "Explotion";

    private Animator _anim;

    private void Awake()
    {
        _anim = searchInChildren ? GetComponentInChildren<Animator>() : GetComponent<Animator>();
        if (_anim == null)
            Debug.LogWarning("[NPCDeathAnimator] No Animator found.", this);
    }

    public void PlayExplotion()
    {
        if (_anim == null) return;

        if (useTrigger)
        {
            _anim.ResetTrigger(explotionTrigger);
            _anim.SetTrigger(explotionTrigger);
        }
        else
        {
            _anim.SetBool(explotionBool, true);
        }

        if (forceStateByName && !string.IsNullOrEmpty(explotionStateName))
        {
            // 0.05f para que no sea “snap” duro
            _anim.CrossFade(explotionStateName, 0.05f, 0);
        }

        _anim.Update(0f);
    }
}
