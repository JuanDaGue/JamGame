using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Exact scene name as in Build Settings.")]
    [SerializeField] private string sceneName;
    public string SceneName => sceneName;

    [Header("Click")]
    [SerializeField] private bool clickable = true;
    public bool Clickable => clickable;

    [Tooltip("Optional: if assigned, this overrides sceneName.")]
    [SerializeField] private int buildIndexOverride = -1;
    public int BuildIndexOverride => buildIndexOverride;

    [Header("Feedback (optional)")]
    [SerializeField] private GameObject highlightObject; // e.g. outline mesh, icon, etc.

    public void SetHighlight(bool on)
    {
        if (highlightObject) highlightObject.SetActive(on);
    }

    public bool HasValidTarget()
    {
        return (buildIndexOverride >= 0) || !string.IsNullOrEmpty(sceneName);
    }
}
