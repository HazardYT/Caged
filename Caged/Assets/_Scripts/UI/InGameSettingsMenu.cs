using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;

public class InGameSettingsMenu : MonoBehaviourPun
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
    public AudioMixer master;

    public GameObject menu;
    public GameObject menubuttons;
    public Toggle sprinttoggle;
    public Toggle crouchtoggle;
    public Toggle pronetoggle;
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (UserInput.instance.MenuPressed)
        {
            if(!PhotonNetwork.OfflineMode){
                if (!menu.activeSelf && !transform.root.GetComponent<PlayerMovement>().isInGUI){
                    menuon();
                } else menuoff();
            } else if(PhotonNetwork.OfflineMode){
                if (!menu.activeSelf && !transform.root.GetComponent<PlayerMoveOffline>().isInGUI){
                    menuon();
                } else menuoff();
            }
        }
        if (UserInput.instance.ExitPressed && UserInput.instance.currentLookInput is Gamepad)
        {
            if (settingsmenu.activeSelf) { CloseSettings(); return; }
            if (menu.activeSelf) { menuoff(); return; }
        }
    }
    public void menuon()
    {
        menu.SetActive(true);
        if(!PhotonNetwork.OfflineMode){this.transform.root.GetComponent<PlayerMovement>().enabled = false;}
        else{this.transform.root.GetComponent<PlayerMoveOffline>().enabled = false;}
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return;
    }
    public void menuoff()
    {
        menu.SetActive(false);
        if(!PhotonNetwork.OfflineMode){this.transform.root.GetComponent<PlayerMovement>().enabled = true;}
        else{this.transform.root.GetComponent<PlayerMoveOffline>().enabled = true;}
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        return;
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
    public void ToggleSprint(bool i)
    {
        if(!PhotonNetwork.OfflineMode){transform.root.GetComponent<PlayerMovement>().ToggleSprint = i;}
        else{transform.root.GetComponent<PlayerMoveOffline>().ToggleSprint = i;}
    }
    public void ToggleCrouch(bool i)
    {
        if(!PhotonNetwork.OfflineMode){transform.root.GetComponent<PlayerMovement>().ToggleCrouch = i;}
        else{transform.root.GetComponent<PlayerMoveOffline>().ToggleCrouch = i;}
    }
    public void ToggleProne(bool i)
    {
        if(!PhotonNetwork.OfflineMode){transform.root.GetComponent<PlayerMovement>().ToggleProne = i;}
        else{transform.root.GetComponent<PlayerMoveOffline>().ToggleProne = i;}
    }
    public void CloseSettings()
    {
        settingsmenu.SetActive(false);
        menubuttons.SetActive(true);
    }
    public void EnableSettings()
    {
        settingsmenu.SetActive(true);
        menubuttons.SetActive(false);
    }
    public void ExitMenus()
    {
        menu.SetActive(false);
        settingsmenu.SetActive(false);
    }
    public void Leave()
    {
        PhotonNetwork.Destroy(transform.root.gameObject);
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("ConnectToServer");
    }

    private void Start()
    {
        if (!photonView.IsMine) this.enabled = false;
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            foreach (string res in options)
            {
                if (res == option) return;
                else options.Add(option);
            }
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
