using UnityEngine;
using UnityEngine.UI;
using Core.UI;
using Core.Managers;

public class SettingsPopup : PopupBase
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetButton;

    protected override void Start()
    {
        base.Start();
        SetupUI();
    }

    /// <summary>
    /// Setup UI controls and their listeners
    /// </summary>
    private void SetupUI()
    {
        // Setup volume sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Setup toggles
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.AddListener(OnMusicToggled);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        }

        // Setup buttons
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnMusicToggled(bool enabled)
    {
        AudioManager.Instance.ToggleMusic(enabled);

        //AudioManager.Instance.PlaySFX("click");
    }

    private void OnSFXToggled(bool enabled)
    {
        AudioManager.Instance.ToggleSFX(enabled);

        //if (enabled)
        //AudioManager.Instance.PlaySFX("click");
    }

    private void OnCloseClicked()
    {
        //AudioManager.Instance.PlaySFX("click");
        Hide();
        UIManager.Instance.HidePopup<SettingsPopup>();
    }

    private void OnResetClicked()
    {
        //AudioManager.Instance.PlaySFX("click");
        AudioManager.Instance.SetMasterVolume(1f);
        AudioManager.Instance.SetMusicVolume(0.7f);
        AudioManager.Instance.SetSFXVolume(1f);

        // Update sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 0.7f;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 1f;

        Debug.Log("[SettingsPopup] Settings reset to default");
    }

    protected override void OnPopupShown()
    {
        Debug.Log("[SettingsPopup] Settings menu shown");

        // Sync UI with current values
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
    }

    protected override void OnPopupHidden()
    {
        Debug.Log("[SettingsPopup] Settings menu hidden");
    }

    public override void OnDespawned()
    {
        // Clean up listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (musicToggle != null)
            musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggled);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetClicked);

        base.OnDespawned();
    }
}
