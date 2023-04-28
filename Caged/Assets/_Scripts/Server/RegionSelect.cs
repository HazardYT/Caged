using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
public class RegionSelect : MonoBehaviourPunCallbacks
{
    public TMP_Dropdown regionDropdown;
    public static string region;

    public void Start()
    {
        region = PhotonNetwork.CloudRegion;
        switch (region)
        {
            case "us":
                regionDropdown.value = 0;
                break;
            case "usw":
                regionDropdown.value = 1;
                break;
            case "cae":
                regionDropdown.value = 2;
                break;
            case "tr":
                regionDropdown.value = 3;
                break;
            case "eu":
                regionDropdown.value = 4;
                break;
            case "asia":
                regionDropdown.value = 5;
                break;
            case "au":
                regionDropdown.value = 6;
                break;
            case "cn":
                regionDropdown.value = 7;
                break;
            case "in":
                regionDropdown.value = 8;
                break;
            case "jp":
                regionDropdown.value = 9;
                break;
            case "ru":
                regionDropdown.value = 10;
                break;
            case "rue":
                regionDropdown.value = 11;
                break;
            case "za":
                regionDropdown.value = 12;
                break;
            case "sa":
                regionDropdown.value = 13;
                break;
            case "kr":
                regionDropdown.value = 14;
                break;
        }

    }

    public void SetValue()
    { 
        switch(regionDropdown.value)
        {
            case 0:
                region = "us";
                break;
            case 1:
                region = "usw";
                break;
            case 2:
                region = "cae";
                break;
            case 3:
                region = "tr";
                break;
            case 4:
                region = "eu";
                break;
            case 5:
                region = "asia";
                break;
            case 6:
                region = "au";
                break;
            case 7:
                region = "cn";
                break;
            case 8:
                region = "in";
                break;
            case 9:
                region = "jp";
                break;
            case 10:
                region = "ru";
                break;
            case 11:
                region = "rue";
                break;
            case 12:
                region = "za";
                break;
            case 13:
                region = "sa";
                break;
            case 14:
                region = "kr";
                break;
        }
        StartCoroutine(ConnectToRegion());
    }

    public IEnumerator ConnectToRegion()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        }

        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("Connecting to Region: " + region);
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        var ping = PhotonNetwork.GetPing();
        Debug.Log("Connected to Region: " + region + " with Ping: " + ping);
    }


}
