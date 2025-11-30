using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public AudioMixer audioMixer;
    Resolution[] resolutions;

    public TMP_Dropdown resolutionDropDown;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string ResolutionIndexKey = "ResolutionIndex"; // Key for storing the selected resolution index
    private const string FullscreenKey = "Fullscreen"; // Key for storing the fullscreen state

    // Reference to sliders for music and SFX volume
    public Slider musicSlider; // Assign in the inspector
    public Slider sfxSlider; // Assign in the inspector

    // Reference to the fullscreen toggle
    public Toggle fullscreenToggle; // Assign in the inspector

    void Start()
    {
        // Get available resolutions
        resolutions = Screen.resolutions;

        resolutionDropDown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropDown.AddOptions(options);

        // Load and set the saved resolution index, or default to current
        if (PlayerPrefs.HasKey(ResolutionIndexKey))
        {
            currentResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey);
        }

        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();

        // Load and set the fullscreen state
        if (PlayerPrefs.HasKey(FullscreenKey))
        {
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey) == 1; // Convert to bool (1 for true, 0 for false)
            Screen.fullScreen = isFullscreen; // Set fullscreen
            fullscreenToggle.isOn = isFullscreen; // Set toggle state
        }

        LoadAudioSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex); // Save the selected resolution index
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0); // Save the fullscreen state (1 for true, 0 for false)
    }

    public void OnFullscreenToggleChanged(bool isOn)
    {
        SetFullscreen(isOn); // Update fullscreen state based on the toggle
    }

    public void LowQ()
    {
        QualitySettings.SetQualityLevel(0, true);
    }

    public void MediumQ()
    {
        QualitySettings.SetQualityLevel(1, true);
    }

    public void HighQ()
    {
        QualitySettings.SetQualityLevel(2, true);
    }

    private void LoadAudioSettings()
    {
        // Load and set the music volume
        if (PlayerPrefs.HasKey(MusicVolumeKey))
        {
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(savedMusicVolume) * 20); // Apply saved volume
            musicSlider.value = savedMusicVolume; // Set slider value
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(1) * 20); // Default to 0 dB if no saved value
            musicSlider.value = 1; // Set slider to default (assuming the slider value represents 100%)
        }

        // Load and set the SFX volume
        if (PlayerPrefs.HasKey(SFXVolumeKey))
        {
            float savedSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(savedSFXVolume) * 20); // Apply saved volume
            sfxSlider.value = savedSFXVolume; // Set slider value
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(1) * 20); // Default to 0 dB if no saved value
            sfxSlider.value = 1; // Set slider to default (assuming the slider value represents 100%)
        }
    }
}
