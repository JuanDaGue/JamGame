using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StaticNPC : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Nombre exacto del parámetro bool que activa Idle")]
    [SerializeField] private string idleParameterName = "IsInIdle";

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ForceIdle();
    }

    private void Start()
    {
        ForceIdle();
    }

    private void ForceIdle()
    {
        if (_animator == null) return;

        // Seguridad: resetear todo antes
        foreach (var param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                _animator.SetBool(param.name, false);
            }
        }

        // Activar Idle
        _animator.SetBool(idleParameterName, true);

        // Asegurar evaluación inmediata
        _animator.Update(0f);
    }
}
