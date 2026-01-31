using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Exact scene name as in Build Settings.")]
    public string sceneName;

    [Header("Click")]
    public bool clickable = true;

    [Tooltip("Optional: if assigned, this overrides sceneName.")]
    public int buildIndexOverride = -1;

    [Header("Feedback (optional)")]
    public GameObject highlightObject; // e.g. outline mesh, icon, etc.

    public void SetHighlight(bool on)
    {
        if (highlightObject) highlightObject.SetActive(on);
    }

    public bool HasValidTarget()
    {
        return (buildIndexOverride >= 0) || !string.IsNullOrEmpty(sceneName);
    }
}
