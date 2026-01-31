using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Configuration")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasScaler canvasScaler;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Game UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text levelText;
    
    [Header("Game Over/Victory UI")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text victoryScoreText;
    
    private GameManager _gameManager;
    private GameModel _model;
    
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _model = new GameModel(); // En una implementación real, esto vendría del GameManager
        
        ConfigureCanvas();
        SetupEventListeners();
        
        // Inicializar UI según estado actual
        UpdateUIState(_gameManager.CurrentState);
    }
    
    private void ConfigureCanvas()
    {
        if (canvasScaler == null) return;
        
        // Configuración responsive
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // Configurar render mode según necesidad
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
    
    private void SetupEventListeners()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += UpdateUIState;
        }
    }
    
    private void UpdateUIState(GameState state)
    {
        // Ocultar todos los paneles primero
        menuPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        
        // Activar panel correspondiente
        switch (state)
        {
            case GameState.InMenu:
                menuPanel.SetActive(true);
                break;
                
            case GameState.InGame:
                gamePanel.SetActive(true);
                UpdateGameUI();
                break;
                
            case GameState.InPause:
                pausePanel.SetActive(true);
                break;
                
            case GameState.InGameover:
                gameOverPanel.SetActive(true);
                UpdateGameOverUI();
                break;
                
            case GameState.InVictory:
                victoryPanel.SetActive(true);
                UpdateVictoryUI();
                break;
        }
    }
    
    private void UpdateGameUI()
    {
        if (_gameManager != null)
        {
            scoreText.text = $"Score: {_gameManager.GetScore()}";
            // Actualizar otros elementos UI
        }
    }
    
    private void UpdateGameOverUI()
    {
        if (_gameManager != null)
        {
            finalScoreText.text = $"Final Score: {_gameManager.GetScore()}";
        }
    }
    
    private void UpdateVictoryUI()
    {
        if (_gameManager != null)
        {
            victoryScoreText.text = $"Victory Score: {_gameManager.GetScore()}";
        }
    }
    
    // Métodos para botones UI (llamados desde OnClick en Unity)
    public void OnStartButtonClick()
    {
        _gameManager?.StartGame();
    }
    
    public void OnPauseButtonClick()
    {
        _gameManager?.PauseGame();
    }
    
    public void OnResumeButtonClick()
    {
        _gameManager?.ResumeGame();
    }
    
    public void OnRestartButtonClick()
    {
        _gameManager?.StartGame();
    }
    
    public void OnMenuButtonClick()
    {
        _gameManager?.ReturnToMenu();
    }
    
    public void OnQuitButtonClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Métodos para ajustes de sonido
    public void OnMasterVolumeChanged(Slider slider)
    {
        _gameManager?.SetMasterVolume(slider.value);
    }
    
    public void OnMusicVolumeChanged(Slider slider)
    {
        _gameManager?.SetMusicVolume(slider.value);
    }
    
    public void OnVFXVolumeChanged(Slider slider)
    {
        _gameManager?.SetVFXVolume(slider.value);
    }
    
    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= UpdateUIState;
        }
    }
}