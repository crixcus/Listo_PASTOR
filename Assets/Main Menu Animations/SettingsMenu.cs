using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    void Start()
    {
        LoadSettings();
        PopulateResolutions();
    }

    // ── AUDIO ──────────────────────────────────────────

    public void SetMusicVolume(float volume)
    {
        // Slider must be min 0.001, max 1
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);

        var tenkoku = FindObjectOfType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku != null)
            tenkoku.overallVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);

        var tenkoku = FindObjectOfType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku != null)
        {
            tenkoku.overallVolume = volume;        // master — controls everything
            tenkoku.volumeThunder = volume;        // thunder specifically
            tenkoku.volumeWind = volume;        // wind
            tenkoku.volumeRain = volume;        // rain
            tenkoku.volumeTurb1 = volume;        // turbulence 1
            tenkoku.volumeTurb2 = volume;        // turbulence 2
        }
    }

    // ── GRAPHICS ───────────────────────────────────────

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }

    // ── RESOLUTION ─────────────────────────────────────

    void PopulateResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height
                          + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    // ── SAVE / LOAD ────────────────────────────────────

    void LoadSettings()
    {
        // Audio — default to 75% if no saved value
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        // Graphics
        int savedQuality = PlayerPrefs.GetInt("Quality", 2);
        QualitySettings.SetQualityLevel(savedQuality);
    }

    public void ResetToDefaults()
    {
        if (musicSlider != null) musicSlider.value = 0.75f;
        if (sfxSlider != null) sfxSlider.value = 0.75f;

        SetMusicVolume(0.75f);
        SetSFXVolume(0.75f);
        SetQuality(2);

        PlayerPrefs.DeleteAll();
    }
}