
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPun
{
    public GameObject[] playerPrefabs;
    public List<Transform> spawnPoints;

    private void Start()
    {
        photonView.RPC(nameof(SpawnPlayer), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    private void SpawnPlayer()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        if (spawnPoints[randomIndex].GetComponent<PhotonView>() != null)
        {
            Transform selectedSpawnPoint = spawnPoints[randomIndex];
            PhotonNetwork.Instantiate("Players/" + playerPrefabs[PlayerObjects.currentSelectionIndex].name, selectedSpawnPoint.position, Quaternion.identity);

            PhotonNetwork.Destroy(selectedSpawnPoint.GetComponent<PhotonView>());
        }
        else
        {
            Debug.LogError("No available spawn points.");
        }
    }
}