
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createjoinInput;
    public TMP_InputField inputname;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 5;
        PhotonNetwork.JoinOrCreateRoom(createjoinInput.text, options, TypedLobby.Default);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(createjoinInput.text);
    }

    public override void OnJoinedRoom()
    {
        SetName();
        PhotonNetwork.LoadLevel("Lobby");
    }
    public void SetName()
    {
        if (inputname.text != "")
        {
            PhotonNetwork.NickName = inputname.text;
        }
        else
        {
            PhotonNetwork.NickName = "anonymous";
        }
    }
}
