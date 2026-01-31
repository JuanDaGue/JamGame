using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    InMenu,
    InGame,
    InPause,
    InGameover,
    InVictory
}

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    // Current game state
    [SerializeField] private GameState _currentState;
    public GameState CurrentState => _currentState;

    // Events for state changes
    public event Action<GameState> OnGameStateChanged;

    // Sound settings
    [System.Serializable]
    public class SoundSettings
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float vfxVolume = 0.8f;
        public bool musicMuted = false;
        public bool vfxMuted = false;
    }

    [SerializeField] private SoundSettings soundSettings = new SoundSettings();

    // Audio sources
    private AudioSource _musicSource;
    private AudioSource _vfxSource;

    // Audio clips (asignar en el inspector)
    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;

    // Model reference
    private GameModel _model;

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        Initialize();
    }

    private void Initialize()
    {
        // Initialize model
        _model = new GameModel();
        
        // Setup audio sources
        SetupAudioSources();
        
        // Set initial state
        ChangeState(GameState.InMenu);
    }

    private void SetupAudioSources()
    {
        // Create music audio source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.parent = transform;
        _musicSource = musicObj.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        
        // Create VFX audio source
        GameObject vfxObj = new GameObject("VFXSource");
        vfxObj.transform.parent = transform;
        _vfxSource = vfxObj.AddComponent<AudioSource>();
        _vfxSource.playOnAwake = false;
        
        UpdateAudioVolumes();
    }

    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        OnGameStateChanged?.Invoke(newState);

        HandleStateChange(newState);
    }

    private void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.InMenu:
                Time.timeScale = 1f;
                PlayMenuMusic();
                break;
                
            case GameState.InGame:
                Time.timeScale = 1f;
                _model.ResetGame();
                PlayGameMusic();
                break;
                
            case GameState.InPause:
                Time.timeScale = 0f;
                break;
                
            case GameState.InGameover:
                Time.timeScale = 1f;
                PlayVFX(gameOverSound);
                break;
                
            case GameState.InVictory:
                Time.timeScale = 1f;
                PlayVFX(victorySound);
                break;
        }
    }

    // Music control
    private void PlayMenuMusic()
    {
        if (menuMusic != null)
        {
            PlayMusic(menuMusic);
        }
    }

    private void PlayGameMusic()
    {
        if (gameMusic != null)
        {
            PlayMusic(gameMusic);
        }
    }

    // Sound management
    public void PlayMusic(AudioClip clip)
    {
        if (soundSettings.musicMuted || soundSettings.masterVolume <= 0) return;
        
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void PlayVFX(AudioClip clip)
    {
        if (soundSettings.vfxMuted || soundSettings.masterVolume <= 0 || clip == null) return;
        
        _vfxSource.PlayOneShot(clip, soundSettings.vfxVolume * soundSettings.masterVolume);
    }

    public void PlayButtonClick()
    {
        PlayVFX(buttonClickSound);
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void SetMasterVolume(float volume)
    {
        soundSettings.masterVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        soundSettings.musicVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
    }

    public void SetVFXVolume(float volume)
    {
        soundSettings.vfxVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
    }

    public void ToggleMusicMute()
    {
        soundSettings.musicMuted = !soundSettings.musicMuted;
        _musicSource.mute = soundSettings.musicMuted;
    }

    public void ToggleVFXMute()
    {
        soundSettings.vfxMuted = !soundSettings.vfxMuted;
        _vfxSource.mute = soundSettings.vfxMuted;
    }

    private void UpdateAudioVolumes()
    {
        _musicSource.volume = soundSettings.musicVolume * soundSettings.masterVolume;
        _musicSource.mute = soundSettings.musicMuted;
        
        _vfxSource.volume = soundSettings.vfxVolume * soundSettings.masterVolume;
        _vfxSource.mute = soundSettings.vfxMuted;
    }

    // Game control methods
    public void StartGame()
    {
        ChangeState(GameState.InGame);
    }

    public void PauseGame()
    {
        ChangeState(GameState.InPause);
    }

    public void ResumeGame()
    {
        ChangeState(GameState.InGame);
    }

    public void GameOver()
    {
        ChangeState(GameState.InGameover);
    }

    public void Victory()
    {
        ChangeState(GameState.InVictory);
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.InMenu);
    }

    // Model access methods
    public void AddScore(int points)
    {
        _model.AddScore(points);
    }

    public int GetScore() => _model.Score;
    public int GetLives() => _model.Lives;

    // Get sound settings for UI
    public SoundSettings GetSoundSettings() => soundSettings;
}