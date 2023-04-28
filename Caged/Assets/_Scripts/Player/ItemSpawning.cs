using UnityEngine;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;

public class ItemSpawning : MonoBehaviourPun
{
    [Header("Items")]
    public string[] ItemNames;
    [Header("Spawn Points")]
    public Transform[] frontdoorKeySpawn;
    public Transform[] meatSpawnPoints;
    public Transform[] junkSpawnPoints;
    public Transform[] RoomKeySpawnPoints;
    public Transform[] JailKeySpawnPoints;
    public Transform[] CellarKeySpawnPoints;
    public Transform[] CageKeySpawnPoints;
    [Header("Valueables Spawn Points")]
    public Transform[] valueablesSpawnPoints;

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnGameItems();
        }
    }

    public void SpawnGameItems()
    {
        StartCoroutine(SpawnItem("Room Key", 1, frontdoorKeySpawn));
        StartCoroutine(SpawnItem("Room Key", 1, RoomKeySpawnPoints));
        StartCoroutine(SpawnItem("Jail Key", 1, JailKeySpawnPoints));
        StartCoroutine(SpawnItem("Meat", 2, meatSpawnPoints));
        StartCoroutine(SpawnItem("Coke Can", 5, junkSpawnPoints));
        StartCoroutine(SpawnItem("Battery", 5, junkSpawnPoints));
        // Add more items to spawn here
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(SpawnItem(ItemNames[0], 1, junkSpawnPoints));
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                StartCoroutine(SpawnItem(ItemNames[1], 1, meatSpawnPoints));
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                StartCoroutine(SpawnItem(ItemNames[2], 1, RoomKeySpawnPoints));
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                StartCoroutine(SpawnItem(ItemNames[3], 1, JailKeySpawnPoints));
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                StartCoroutine(SpawnItem(ItemNames[4], 1, CellarKeySpawnPoints));
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StartCoroutine(SpawnItem(ItemNames[5], 1, CageKeySpawnPoints));
            }
        }
    }
    public IEnumerator SpawnItem(string itemName, int amount, Transform[] spawnPoints)
    {
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < amount; i++)
        {
            if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning("No more available spawn points for " + itemName);
                break;
            }

            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform randomSpawnPoint = availableSpawnPoints[randomIndex];

            GameObject spawnObject = PhotonNetwork.Instantiate("Items/" + itemName, randomSpawnPoint.position, randomSpawnPoint.rotation);
            photonView.RPC(nameof(SetNameRPC), RpcTarget.AllBuffered, spawnObject.GetComponent<PhotonView>().ViewID, itemName);

            availableSpawnPoints.RemoveAt(randomIndex);

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator SpawnValuable(string itemName, int amount, int spawnpoint)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject spawnObject = PhotonNetwork.Instantiate("Items/" + itemName, valueablesSpawnPoints[spawnpoint].position, valueablesSpawnPoints[spawnpoint].rotation);
            photonView.RPC(nameof(SetNameRPC), RpcTarget.AllBuffered, spawnObject.GetComponent<PhotonView>().ViewID, itemName);
            yield return new WaitForEndOfFrame();
        }
        
    }
    [PunRPC]
    void SetNameRPC(int viewID, string name)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            view.gameObject.name = name;
        }
    }




}
