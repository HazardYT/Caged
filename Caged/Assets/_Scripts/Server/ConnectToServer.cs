using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        InvokeRepeating(nameof(RetryConnect), 5f, 5f);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        var ping = PhotonNetwork.GetPing();
        Debug.Log("Connected to Region: " + PhotonNetwork.CloudRegion + " with Ping: " + ping);
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Menu");
    }
    private void RetryConnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}
