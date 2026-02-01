using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using GameJam.Core;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Configuration")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasScaler canvasScaler;

    [Header("UI Panels with Canvas Group")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private CanvasGroup gamePanel;
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private CanvasGroup gameOverPanel;
    [SerializeField] private CanvasGroup victoryPanel;
    [SerializeField] private CanvasGroup optionsPanel;

    [Header("Menu Panel Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    [Header("Options Panel Elements")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider vfxVolumeSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle vfxToggle;
    [SerializeField] private Button optionsBackButton;

    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Button pauseButton;

    [Header("Pause Panel Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseMenuButton;

    [Header("Game Over Panel")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMenuButton;

    [Header("Victory Panel")]
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private Button victoryNextButton;
    [SerializeField] private Button victoryMenuButton;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Scene Flow (GameJam)")]
    [Tooltip("Si está activo, el botón Play carga el Hub (Main). Si no, solo cambia estado.")]
    [SerializeField] private bool loadHubSceneOnPlay = true;

    [Tooltip("Si está activo, los botones 'Menu' cargan Menu1 además de cambiar estado.")]
    [SerializeField] private bool loadMenuSceneOnReturnToMenu = true;

    private GameManager _gameManager;
    private CanvasGroup _currentActivePanel;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        // Evitar duplicados si por error se dejó un UIManager en varias escenas
        var existing = FindObjectsByType<UIManager>(FindObjectsSortMode.None);
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        ConfigureCanvas();
        SetupEventListeners();
        InitializeButtons();

        HideAllPanelsImmediate();

        if (_gameManager != null)
            UpdateUIState(_gameManager.CurrentState);
        else
            UpdateUIState(GameState.InMenu);
    }

    private void ConfigureCanvas()
    {
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (mainCanvas != null)
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    private void SetupEventListeners()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= UpdateUIState;
            _gameManager.OnGameStateChanged += UpdateUIState;
        }
    }

    private void InitializeButtons()
    {
        // Menu Panel
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClick);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsButtonClick);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClick);

        // Options Panel
        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(OnOptionsBackButtonClick);

        // Pause Panel
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeButtonClick);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClick);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(OnPauseMenuButtonClick);

        // Game Panel
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClick);

        // Game Over Panel
        if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnGameOverRestartClick);
        if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnGameOverMenuClick);

        // Victory Panel
        if (victoryNextButton != null) victoryNextButton.onClick.AddListener(OnVictoryNextClick);
        if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(OnVictoryMenuClick);

        // Options sliders and toggles
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (vfxVolumeSlider != null) vfxVolumeSlider.onValueChanged.AddListener(OnVFXVolumeChanged);
        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        if (vfxToggle != null) vfxToggle.onValueChanged.AddListener(OnVFXToggleChanged);
    }

    private void HideAllPanelsImmediate()
    {
        SetPanelActive(menuPanel, false, false);
        SetPanelActive(gamePanel, false, false);
        SetPanelActive(pausePanel, false, false);
        SetPanelActive(gameOverPanel, false, false);
        SetPanelActive(victoryPanel, false, false);
        SetPanelActive(optionsPanel, false, false);

        _currentActivePanel = null;
    }

    private void UpdateUIState(GameState state)
    {
        // Cancelar fades en curso
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        // Apagar panel actual (sin corutinas múltiples)
        if (_currentActivePanel != null)
            SetPanelActive(_currentActivePanel, false, true);

        CanvasGroup newPanel = null;

        switch (state)
        {
            case GameState.InMenu:
                newPanel = menuPanel;
                break;

            case GameState.InGame:
                newPanel = gamePanel;
                UpdateGameUI();
                break;

            case GameState.InPause:
                newPanel = pausePanel;
                break;

            case GameState.InGameover:
                newPanel = gameOverPanel;
                UpdateGameOverUI();
                break;

            case GameState.InVictory:
                newPanel = victoryPanel;
                UpdateVictoryUI();
                break;
        }

        if (newPanel != null)
        {
            _currentActivePanel = newPanel;
            SetPanelActive(newPanel, true, true);
        }
    }

    private IEnumerator FadePanel(CanvasGroup panel, bool fadeIn)
    {
        if (panel == null) yield break;

        float startAlpha = panel.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        panel.interactable = fadeIn;
        panel.blocksRaycasts = fadeIn;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        panel.alpha = targetAlpha;
    }

    private void SetPanelActive(CanvasGroup panel, bool active, bool fade = true)
    {
        if (panel == null) return;

        if (fade)
        {
            _fadeRoutine = StartCoroutine(FadePanel(panel, active));
        }
        else
        {
            panel.alpha = active ? 1f : 0f;
            panel.interactable = active;
            panel.blocksRaycasts = active;
        }
    }

    // Menu Panel Methods
    public void OnPlayButtonClick()
    {
        _gameManager?.PlayButtonClick();

        // IMPORTANTE: StartGame() resetea el GameModel. Debe llamarse 1 vez al salir del menú.
        _gameManager?.StartGame();

        if (loadHubSceneOnPlay)
        {
            if (SceneManager.GetActiveScene().name != GameConstants.SCENE_HUB)
                SceneManager.LoadScene(GameConstants.SCENE_HUB);
        }
    }

    public void OnOptionsButtonClick()
    {
        _gameManager?.PlayButtonClick();
        LoadOptionsSettings();
        SetPanelActive(optionsPanel, true);
    }

    public void OnExitButtonClick()
    {
        _gameManager?.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Options Panel Methods
    private void LoadOptionsSettings()
    {
        if (_gameManager == null) return;

        var settings = _gameManager.GetSoundSettings();

        if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = settings.musicVolume;
        if (vfxVolumeSlider != null) vfxVolumeSlider.value = settings.vfxVolume;

        // OJO: Toggle indica "activo", pero el setting es "Muted"
        if (musicToggle != null) musicToggle.isOn = !settings.musicMuted;
        if (vfxToggle != null) vfxToggle.isOn = !settings.vfxMuted;
    }

    public void OnOptionsBackButtonClick()
    {
        _gameManager?.PlayButtonClick();
        SetPanelActive(optionsPanel, false);
    }

    // Pause Panel Methods
    public void OnPauseButtonClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.PauseGame();
    }

    public void OnResumeButtonClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ResumeGame();
    }

    public void OnRestartButtonClick()
    {
        _gameManager?.PlayButtonClick();

        // En este proyecto, "Restart" desde pausa es ambiguo.
        // Por defecto: vuelve a estado InGame sin recargar escenas.
        // Si quieres reiniciar una escena específica, hazlo en el minijuego (MinigameController) o crea un SceneReloader.
        _gameManager?.StartGame();
    }

    public void OnPauseMenuButtonClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ReturnToMenu();

        if (loadMenuSceneOnReturnToMenu)
        {
            if (SceneManager.GetActiveScene().name != GameConstants.SCENE_MENU)
                SceneManager.LoadScene(GameConstants.SCENE_MENU);
        }
    }

    // Game Over Panel Methods
    private void UpdateGameOverUI()
    {
        if (_gameManager != null && finalScoreText != null)
            finalScoreText.text = $"Score: {_gameManager.GetScore()}";
    }

    public void OnGameOverRestartClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.StartGame();
    }

    public void OnGameOverMenuClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ReturnToMenu();

        if (loadMenuSceneOnReturnToMenu)
        {
            if (SceneManager.GetActiveScene().name != GameConstants.SCENE_MENU)
                SceneManager.LoadScene(GameConstants.SCENE_MENU);
        }
    }

    // Victory Panel Methods
    private void UpdateVictoryUI()
    {
        if (_gameManager != null && victoryScoreText != null)
            victoryScoreText.text = $"Score: {_gameManager.GetScore()}";
    }

    public void OnVictoryNextClick()
    {
        _gameManager?.PlayButtonClick();

        // De momento, "Next" solo reinicia la sesión.
        // La progresión real entre minijuegos va por InventorySystem (máscaras).
        _gameManager?.StartGame();
    }

    public void OnVictoryMenuClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ReturnToMenu();

        if (loadMenuSceneOnReturnToMenu)
        {
            if (SceneManager.GetActiveScene().name != GameConstants.SCENE_MENU)
                SceneManager.LoadScene(GameConstants.SCENE_MENU);
        }
    }

    // Update Game UI during gameplay
    private void UpdateGameUI()
    {
        if (_gameManager == null) return;

        if (scoreText != null) scoreText.text = $"Score: {_gameManager.GetScore()}";
        if (livesText != null) livesText.text = $"Lives: {_gameManager.GetLives()}";
    }

    // Sound Settings Methods
    private void OnMasterVolumeChanged(float value) => _gameManager?.SetMasterVolume(value);
    private void OnMusicVolumeChanged(float value) => _gameManager?.SetMusicVolume(value);
    private void OnVFXVolumeChanged(float value) => _gameManager?.SetVFXVolume(value);

    private void OnMusicToggleChanged(bool value)
    {
        // Toggle = "activo". Setting = "Muted". Si el usuario apaga, debemos mutear.
        // Aquí mantenemos el comportamiento del compañero (Toggle invierte estado).
        _gameManager?.ToggleMusicMute();
    }

    private void OnVFXToggleChanged(bool value)
    {
        _gameManager?.ToggleVFXMute();
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
            _gameManager.OnGameStateChanged -= UpdateUIState;
    }
}
