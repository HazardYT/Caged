using UnityEngine;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;

public class ItemSpawning : MonoBehaviourPun
{
    [Header("Spawn Points")]
    public List<Transform> frontdoorKeySpawnPoints = new List<Transform>();
    public List<Transform> meatSpawnPoints = new List<Transform>();
    public List<Transform> junkSpawnPoints = new List<Transform>();
    public List<Transform> RoomKeySpawnPoints = new List<Transform>();
    public List<Transform> JailKeySpawnPoints = new List<Transform>();
    public List<Transform> CellarKeySpawnPoints = new List<Transform>();
    public List<Transform> CageKeySpawnPoints = new List<Transform>();
    [Header("Valueables")]
    public List<Transform> ValueablesSpawnPoints = new List<Transform>();

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnGameItems());
        }
    }

    public IEnumerator SpawnGameItems()
    {
        yield return new WaitForSeconds(0.1f);
        SpawnItem("Room Key", 1, frontdoorKeySpawnPoints);
        yield return new WaitForEndOfFrame();
        SpawnItem("Room Key", 1, RoomKeySpawnPoints);
        yield return new WaitForEndOfFrame();
        SpawnItem("Jail Key", 1, JailKeySpawnPoints);
        yield return new WaitForEndOfFrame();
        SpawnItem("Meat", 2, meatSpawnPoints);
        yield return new WaitForEndOfFrame();
        SpawnItem("Coke Can", 5, junkSpawnPoints);
        yield return new WaitForEndOfFrame();
        SpawnItem("Battery", 5, junkSpawnPoints);
        // Add more items to spawn here
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SpawnItem(ItemManager.instance.ItemNames[0], 1, junkSpawnPoints);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SpawnItem(ItemManager.instance.ItemNames[1], 1, meatSpawnPoints);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SpawnItem(ItemManager.instance.ItemNames[2], 1, RoomKeySpawnPoints);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SpawnItem(ItemManager.instance.ItemNames[3], 1, JailKeySpawnPoints);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SpawnItem(ItemManager.instance.ItemNames[4], 1, CellarKeySpawnPoints);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SpawnItem(ItemManager.instance.ItemNames[5], 1, CageKeySpawnPoints);
            }
        }
    }
    public void SpawnItem(string itemName, int amount, List<Transform> spawnPoints)
    {
        for (int i = 0; i < amount; i++)
        {
            int r = Random.Range(0, spawnPoints.Count);
            GameObject spawnObject = PhotonNetwork.Instantiate("Items/" + itemName, spawnPoints[r].position, spawnPoints[r].rotation);
            photonView.RPC(nameof(SetNameRPC), RpcTarget.AllBufferedViaServer, spawnObject.GetComponent<PhotonView>().ViewID, itemName);
        }
    }

    public void SpawnValuable(string itemName, int amount, List<Transform> spawnpoints)
    {
        for (int i = 0; i < amount; i++)
        {
            int r = Random.Range(0, spawnpoints.Count);
            GameObject spawnObject = PhotonNetwork.Instantiate("Items/" + itemName, spawnpoints[r].position, spawnpoints[r].rotation);
            photonView.RPC(nameof(SetNameRPC), RpcTarget.AllBufferedViaServer, spawnObject.GetComponent<PhotonView>().ViewID, itemName);
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
