using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneManager : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableLayers = ~0; // default: everything
    [SerializeField] private float maxDistance = 200f;

    [Header("Loading")]
    [SerializeField] private bool preventDoubleLoad = true;

    [Tooltip("Optional: load additively if you want to overlay minigame later.")]
    [SerializeField] private bool loadAdditive = false;

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
        if (!_hovered.Clickable) return;
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

        if (portal.BuildIndexOverride >= 0)
        {
            if (loadAdditive) SceneManager.LoadScene(portal.BuildIndexOverride, LoadSceneMode.Additive);
            else SceneManager.LoadScene(portal.BuildIndexOverride, LoadSceneMode.Single);
            return;
        }

        if (loadAdditive) SceneManager.LoadScene(portal.SceneName, LoadSceneMode.Additive);
        else SceneManager.LoadScene(portal.SceneName, LoadSceneMode.Single);
    }
}
