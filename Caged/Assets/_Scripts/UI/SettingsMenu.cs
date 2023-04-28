using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenu : MonoBehaviour
{

    //TABS
    public GameObject Graphics;
    public GameObject Display;
    public GameObject Audio;
    public GameObject Keybinds;
    public GameObject Input;
    public GameObject ControllerKeybinds;
    public GameObject settingsmenu;
    //========

    public GameObject PlayMenu;
    public GameObject credits;
    public AudioMixer master;

    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    public void Settingsback()
    {
        PlayMenu.SetActive(true);
        Graphics.SetActive(true);
        Display.SetActive(false);
        Input.SetActive(false);
        Audio.SetActive(false);
        Keybinds.SetActive(false);
        settingsmenu.SetActive(false);
        ControllerKeybinds.SetActive(false);
    }
    public void GraphicsTab()
    {
        Graphics.SetActive(true);
        Display.SetActive(false);
        Audio.SetActive(false);
        Input.SetActive(false);
        Keybinds.SetActive(false);
        ControllerKeybinds.SetActive(false);
    }
    public void DisplayTab()
    {
        Graphics.SetActive(false);
        Display.SetActive(true);
        Audio.SetActive(false);
        Input.SetActive(false);
        Keybinds.SetActive(false);
        ControllerKeybinds.SetActive(false);
    }
    public void AudioTab()
    {
        Graphics.SetActive(false);
        Input.SetActive(false);
        Display.SetActive(false);
        Audio.SetActive(true);
        Keybinds.SetActive(false);
        ControllerKeybinds.SetActive(false);
    }
    public void InputTab()
    {
        Input.SetActive(true);
        Graphics.SetActive(false);
        Display.SetActive(false);
        Audio.SetActive(false);
        Keybinds.SetActive(false);
        ControllerKeybinds.SetActive(false);
    }
    public void KeyboardKeybindsTab()
    {
        Graphics.SetActive(false);
        Display.SetActive(false);
        Audio.SetActive(false); 
        Keybinds.SetActive(true);
        ControllerKeybinds.SetActive(false);
    }
    public void ControllerKeybindsTab()
    {
        ControllerKeybinds.SetActive(true);
        Graphics.SetActive(false);
        Display.SetActive(false);
        Audio.SetActive(false);
        Keybinds.SetActive(false);
    }
    public void enableSettings()
    {
        settingsmenu.SetActive(true);
        PlayMenu.SetActive(false);
    }
    public void backtoplay()
    {
        PlayMenu.SetActive(true);
    }
    public void Play()
    {
        PlayMenu.SetActive(false);
    }
    public void creditsback()
    {
        credits.SetActive(false);
    }
    public void Credits()
    {
        credits.SetActive(true);
    }

    private void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetMasterVolume(float volume)
    {
        master.SetFloat("MasterVolume", volume);
    }
    public void SetGameVolume(float volume)
    {
        master.SetFloat("GameVolume", volume);
    }
    public void SetMusicVolume(float volume)
    {
        master.SetFloat("MusicVolume", volume);
    }
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
