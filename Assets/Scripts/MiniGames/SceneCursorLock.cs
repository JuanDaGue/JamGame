using UnityEngine;

[DisallowMultipleComponent]
public class SceneCursorLock : MonoBehaviour
{
    [Header("Cursor Settings")]
    [Tooltip("Bloquear el cursor al activar este objeto.")]
    [SerializeField] private bool lockCursor = true;

    [Tooltip("Ocultar el cursor cuando está bloqueado.")]
    [SerializeField] private bool hideCursor = true;

    private void OnEnable()
    {
        Apply();
    }

    private void OnDisable()
    {
        Release();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Apply()
    {
        if (!lockCursor) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !hideCursor;
    }

    private void Release()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
