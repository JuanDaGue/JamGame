using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    
    private GameManager _gameManager;
    private GameModel _model;
    private CanvasGroup _currentActivePanel;
    
    private void Start()
    {
        _gameManager = GameManager.Instance;
        
        ConfigureCanvas();
        SetupEventListeners();
        InitializeButtons();
        
        // Ocultar todos los paneles al inicio
        HideAllPanelsImmediate();
        
        // Mostrar panel inicial según estado
        UpdateUIState(_gameManager.CurrentState);
        DontDestroyOnLoad(gameObject);
    }
    
    private void ConfigureCanvas()
    {
        if (canvasScaler == null) return;
        
        // Configuración responsive
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // Configurar render mode
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
    
    private void SetupEventListeners()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += UpdateUIState;
        }
    }
    
    private void InitializeButtons()
    {
        // Menu Panel
        playButton.onClick.AddListener(OnPlayButtonClick);
        optionsButton.onClick.AddListener(OnOptionsButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);
        
        // Options Panel
        optionsBackButton.onClick.AddListener(OnOptionsBackButtonClick);
        
        // Pause Panel
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        restartButton.onClick.AddListener(OnRestartButtonClick);
        pauseMenuButton.onClick.AddListener(OnPauseMenuButtonClick);
        
        // Game Panel
        pauseButton.onClick.AddListener(OnPauseButtonClick);
        
        // Game Over Panel
        gameOverRestartButton.onClick.AddListener(OnGameOverRestartClick);
        gameOverMenuButton.onClick.AddListener(OnGameOverMenuClick);
        
        // Victory Panel
        victoryNextButton.onClick.AddListener(OnVictoryNextClick);
        victoryMenuButton.onClick.AddListener(OnVictoryMenuClick);
        
        // Options sliders and toggles
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        vfxVolumeSlider.onValueChanged.AddListener(OnVFXVolumeChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        vfxToggle.onValueChanged.AddListener(OnVFXToggleChanged);
    }
    
    private void HideAllPanelsImmediate()
    {
        SetPanelActive(menuPanel, false, false);
        SetPanelActive(gamePanel, false, false);
        SetPanelActive(pausePanel, false, false);
        SetPanelActive(gameOverPanel, false, false);
        SetPanelActive(victoryPanel, false, false);
        SetPanelActive(optionsPanel, false, false);
    }
    
    private void UpdateUIState(GameState state)
    {
        // Fade out current panel
        if (_currentActivePanel != null)
        {
            StartCoroutine(FadePanel(_currentActivePanel, false));
        }
        
        // Activate new panel based on state
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
        
        // Fade in new panel
        if (newPanel != null)
        {
            _currentActivePanel = newPanel;
            StartCoroutine(FadePanel(newPanel, true));
        }
    }
    
    private IEnumerator FadePanel(CanvasGroup panel, bool fadeIn)
    {
        float startAlpha = panel.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        
        // Enable/disable interactability
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
        if (fade)
        {
            StartCoroutine(FadePanel(panel, active));
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
        _gameManager?.StartGame();
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
        
        masterVolumeSlider.value = settings.masterVolume;
        musicVolumeSlider.value = settings.musicVolume;
        vfxVolumeSlider.value = settings.vfxVolume;
        musicToggle.isOn = !settings.musicMuted;
        vfxToggle.isOn = !settings.vfxMuted;
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
        _gameManager?.StartGame();
    }
    
    public void OnPauseMenuButtonClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ReturnToMenu();
    }
    
    // Game Over Panel Methods
    private void UpdateGameOverUI()
    {
        if (_gameManager != null)
        {
            finalScoreText.text = $"Score: {_gameManager.GetScore()}";
        }
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
    }
    
    // Victory Panel Methods
    private void UpdateVictoryUI()
    {
        if (_gameManager != null)
        {
            victoryScoreText.text = $"Score: {_gameManager.GetScore()}";
        }
    }
    
    public void OnVictoryNextClick()
    {
        _gameManager?.PlayButtonClick();
        // Aquí puedes implementar la lógica para el siguiente nivel
        _gameManager?.StartGame();
    }
    
    public void OnVictoryMenuClick()
    {
        _gameManager?.PlayButtonClick();
        _gameManager?.ReturnToMenu();
    }
    
    // Update Game UI during gameplay
    private void UpdateGameUI()
    {
        if (_gameManager != null)
        {
            scoreText.text = $"Score: {_gameManager.GetScore()}";
            livesText.text = $"Lives: {_gameManager.GetLives()}";
        }
    }
    
    // Sound Settings Methods
    private void OnMasterVolumeChanged(float value)
    {
        _gameManager?.SetMasterVolume(value);
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        _gameManager?.SetMusicVolume(value);
    }
    
    private void OnVFXVolumeChanged(float value)
    {
        _gameManager?.SetVFXVolume(value);
    }
    
    private void OnMusicToggleChanged(bool value)
    {
        _gameManager?.ToggleMusicMute();
    }
    
    private void OnVFXToggleChanged(bool value)
    {
        _gameManager?.ToggleVFXMute();
    }
    
    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= UpdateUIState;
        }
    }
}