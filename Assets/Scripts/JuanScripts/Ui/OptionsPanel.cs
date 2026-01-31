using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider vfxSlider;
    
    [Header("Mute Toggles")]
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle vfxMuteToggle;
    
    [Header("UI Elements")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button backButton;
    
    private GameManager _gameManager;
    private GameManager.SoundSettings _originalSettings;
    
    private void Start()
    {
        _gameManager = GameManager.Instance;
        LoadCurrentSettings();
        
        // Save original settings
        _originalSettings = _gameManager.GetSoundSettings();
        
        // Setup button listeners
        applyButton.onClick.AddListener(OnApply);
        cancelButton.onClick.AddListener(OnCancel);
        backButton.onClick.AddListener(OnBack);
    }
    
    private void LoadCurrentSettings()
    {
        var settings = _gameManager.GetSoundSettings();
        
        masterSlider.value = settings.masterVolume;
        musicSlider.value = settings.musicVolume;
        vfxSlider.value = settings.vfxVolume;
        musicMuteToggle.isOn = !settings.musicMuted;
        vfxMuteToggle.isOn = !settings.vfxMuted;
    }
    
    private void OnApply()
    {
        _gameManager.PlayButtonClick();
        SaveSettings();
    }
    
    private void OnCancel()
    {
        _gameManager.PlayButtonClick();
        RestoreOriginalSettings();
        gameObject.SetActive(false);
    }
    
    private void OnBack()
    {
        _gameManager.PlayButtonClick();
        SaveSettings();
        gameObject.SetActive(false);
    }
    
    private void SaveSettings()
    {
        _gameManager.SetMasterVolume(masterSlider.value);
        _gameManager.SetMusicVolume(musicSlider.value);
        _gameManager.SetVFXVolume(vfxSlider.value);
        
        if (musicMuteToggle.isOn != !_gameManager.GetSoundSettings().musicMuted)
        {
            _gameManager.ToggleMusicMute();
        }
        
        if (vfxMuteToggle.isOn != !_gameManager.GetSoundSettings().vfxMuted)
        {
            _gameManager.ToggleVFXMute();
        }
    }
    
    private void RestoreOriginalSettings()
    {
        _gameManager.SetMasterVolume(_originalSettings.masterVolume);
        _gameManager.SetMusicVolume(_originalSettings.musicVolume);
        _gameManager.SetVFXVolume(_originalSettings.vfxVolume);
        
        if (_gameManager.GetSoundSettings().musicMuted != _originalSettings.musicMuted)
        {
            _gameManager.ToggleMusicMute();
        }
        
        if (_gameManager.GetSoundSettings().vfxMuted != _originalSettings.vfxMuted)
        {
            _gameManager.ToggleVFXMute();
        }
    }
}