using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviourPun
{
    public GameObject[] playerPrefabs;
    public List<Transform> spawnPoints;

    private List<int> usedSpawnPoints = new List<int>();

    private void Start()
    {
        StartCoroutine(SpawnPlayer());
    }

    private IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        int randomIndex = GetRandomSpawnPointIndex();

        if (randomIndex != -1)
        {
            Transform selectedSpawnPoint = spawnPoints[randomIndex];
            GameObject playerPrefab = playerPrefabs[PlayerObjects.currentSelectionIndex];
            GameObject player = PhotonNetwork.Instantiate("Players/" + playerPrefab.name, selectedSpawnPoint.position, Quaternion.identity);
            photonView.RPC(nameof(AddUsedSpawnPoint), RpcTarget.AllBufferedViaServer, randomIndex);
        }
        else
        {
            Debug.LogError("No available spawn points.");
        }
    }

    [PunRPC]
    public void AddUsedSpawnPoint(int index)
    {
        usedSpawnPoints.Add(index);
    }

    private int GetRandomSpawnPointIndex()
    {
        if (usedSpawnPoints.Count >= spawnPoints.Count)
        {
            return -1; // No available spawn points
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, spawnPoints.Count);
        } while (usedSpawnPoints.Contains(randomIndex));

        return randomIndex;
    }
}