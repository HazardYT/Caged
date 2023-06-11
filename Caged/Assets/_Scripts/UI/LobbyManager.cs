using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPun
{
    public static int AInumber = 0;
    public static int Difficulty = 1;
    public Transform loadingImage;
    public GameObject mapselect;
    public Image MapImageSlot;
    [SerializeField] private Sprite[] mapImages;
    [HideInInspector] public string levelname;
    public GameObject StartButton;
    public GameObject MapButton;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI Aiselection;
    public TextMeshProUGUI mapnamerpc;
    public TextMeshProUGUI region;

    public void Start()
    {
        levelname = "BeaverCreek";
        MapImageSlot.sprite = mapImages[0];
        PhotonNetwork.AutomaticallySyncScene = true;
        region.text = RegionSelect.region;
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
            MapButton.SetActive(true);
        }
    }
    public void ChangeAI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (AInumber <= 1)
            {
                AInumber++;
            }
            else
            {
                AInumber = 0;
            }
            photonView.RPC(nameof(AIChangeRPC), RpcTarget.AllBuffered, AInumber);
        }
    }
    [PunRPC]
    public void AIChangeRPC(int i)
    {
        AInumber = i;
        switch (i)
        {
            case 0:
                Aiselection.text = "Donzel";
                break;
            case 1:
                Aiselection.text = "Khaosa";
                break;
            case 2:
                Aiselection.text = "Zlata";
                break;
        }
    }

    public void SetDifficulty()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (Difficulty <= 2)
            {
                Difficulty++;
            }
            else
            {
                Difficulty = 0;
            }
            photonView.RPC(nameof(DifficultyRPC), RpcTarget.AllBuffered, Difficulty);
        }
    }
    [PunRPC]
    public void DifficultyRPC(int i)
    {
        Difficulty = i;
        switch (i)
        {
            case 0:
                difficultyText.text = "Easy";
                break;
            case 1:
                difficultyText.text = "Normal";
                break;
            case 2:
                difficultyText.text = "Hard";
                break;
            case 3:
                difficultyText.text = "Nightmare";
                break;
        }
    }
    public void mapback()
    {
        mapselect.SetActive(false);
    }

    public void selectmap()
    {
        mapselect.SetActive(true);
    }

    public void setmap1()
    {
        levelname = "BeaverCreek";
        photonView.RPC(nameof(rpcMapName), RpcTarget.AllBuffered, levelname);
        mapselect.SetActive(false);
        MapImageSlot.sprite = mapImages[0];
    }

    public void setmap2()
    {
        levelname = "Cottondale";
        photonView.RPC(nameof(rpcMapName), RpcTarget.AllBuffered, levelname);
        mapselect.SetActive(false);
        MapImageSlot.sprite = mapImages[1];
    }

    public void setmap3()
    {
        levelname = "Woodsburgh";
        photonView.RPC(nameof(rpcMapName), RpcTarget.AllBuffered, levelname);
        mapselect.SetActive(false);
        MapImageSlot.sprite = mapImages[2];
    }

    public void leavelobby()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }

    public void startgame()
    {
        photonView.RPC(nameof(Loading), RpcTarget.All);
        Invoke(nameof(startTimer), 1f);
    }
    void startTimer()
    {
        PhotonNetwork.LoadLevel(levelname);
    }
    [PunRPC]
    public void Loading()
    {
        loadingImage.gameObject.SetActive(true);
    }

    [PunRPC]
    public void rpcMapName(string name)
    {
        mapnamerpc.text = name;
    }

}
