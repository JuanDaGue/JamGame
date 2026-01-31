using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneManager : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;
    public LayerMask clickableLayers = ~0; // default: everything
    public float maxDistance = 200f;

    [Header("Loading")]
    public bool preventDoubleLoad = true;

    [Tooltip("Optional: load additively if you want to overlay minigame later.")]
    public bool loadAdditive = false;

    ScenePortal _hovered;
    bool _isLoading;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        UpdateHover();

        if (Input.GetMouseButtonDown(0))
            TryClick();
    }

    void UpdateHover()
    {
        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, clickableLayers, QueryTriggerInteraction.Ignore))
        {
            ScenePortal portal = hit.collider.GetComponentInParent<ScenePortal>();

            if (portal != _hovered)
            {
                if (_hovered) _hovered.SetHighlight(false);
                _hovered = portal;
                if (_hovered) _hovered.SetHighlight(true);
            }
        }
        else
        {
            if (_hovered) _hovered.SetHighlight(false);
            _hovered = null;
        }
    }

    void TryClick()
    {
        if (_isLoading && preventDoubleLoad) return;
        if (!_hovered) return;
        if (!_hovered.clickable) return;
        if (!_hovered.HasValidTarget())
        {
            Debug.LogWarning($"[LobbySceneManager] Portal '{_hovered.name}' has no target scene assigned.");
            return;
        }

        LoadPortal(_hovered);
    }

    public void LoadPortal(ScenePortal portal)
    {
        if (!portal) return;

        _isLoading = true;

        if (portal.buildIndexOverride >= 0)
        {
            if (loadAdditive) SceneManager.LoadScene(portal.buildIndexOverride, LoadSceneMode.Additive);
            else SceneManager.LoadScene(portal.buildIndexOverride, LoadSceneMode.Single);
            return;
        }

        if (loadAdditive) SceneManager.LoadScene(portal.sceneName, LoadSceneMode.Additive);
        else SceneManager.LoadScene(portal.sceneName, LoadSceneMode.Single);
    }
}
