using Photon.Pun;
using TMPro;

public class JoinRoom : MonoBehaviourPun
{
    public TextMeshProUGUI joinInput;
    public void JoinRooms()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }
}
