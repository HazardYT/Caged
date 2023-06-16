using UnityEngine;
using Photon.Pun;
public class Van : MonoBehaviourPun
{
    GameManager manager;
    void Start(){
        manager = GameObject.FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager._savedNPCS > 0)
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
