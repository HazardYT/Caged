using UnityEngine;
using Photon.Pun;
public class StartGracePeriod : MonoBehaviourPun
{
    public MonsterSpawner monsterSpawner;
    public bool spawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!spawned)
            {
                monsterSpawner.GracePeriod();
                photonView.RPC(nameof(spawnedtoggle), RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void spawnedtoggle()
    {
        spawned = true;
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.FindObjectOfType<GameManager>().timerOn = true;
        }
    }
}