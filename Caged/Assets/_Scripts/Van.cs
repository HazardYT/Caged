using UnityEngine;
using Photon.Pun;
public class Van : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EndGame();
        }
    }
    public void EndGame()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.IsMasterClient)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhotonNetwork.LoadLevel("Lobby");
        }
    }
}
