using UnityEngine;
using UnityEngine.SceneManagement;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    [SerializeField] private Light directionalLight;
    [SerializeField] private Material mainSkybox;
    [SerializeField] private Material midDarkSkybox;
    [SerializeField] private Material darkSkybox;

    [SerializeField] private float darkenStep = 0.1f;

    private Material currentSkybox;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reapply the last skybox when a new scene loads
        if (currentSkybox != null)
            RenderSettings.skybox = currentSkybox;
    }

    public void DarkenDay()
    {
        if (directionalLight == null) return;

        directionalLight.intensity = Mathf.Max(0f, directionalLight.intensity - darkenStep);
        UpdateSkybox();
    }

    private void UpdateSkybox()
    {
        float intensity = directionalLight.intensity;

        if (intensity <= 0.2f)
            currentSkybox = darkSkybox;
        else if (intensity <= 0.5f)
            currentSkybox = midDarkSkybox;
        else
            currentSkybox = mainSkybox;

        RenderSettings.skybox = currentSkybox;
    }
}