using UnityEngine;
using Photon.Pun;
using System.Collections;
public class MonsterSpawner : MonoBehaviourPun
{
    public float SafePeriodTime = 15f;
    public Transform[] SpawnPoints;
    public LayerMask whatIsPlayer;
    private HudText hudText;
    public Transform Monster;

    public void Start()
    {
        hudText = GameObject.Find("GameUI").GetComponent<HudText>();
        switch (LobbyManager.Difficulty)
        {
            case 0:
                SafePeriodTime = 60;
                break;
            case 1:
                SafePeriodTime = 45;
                break;
            case 2:
                SafePeriodTime = 30;
                break;
            case 3:
                SafePeriodTime = 15;
                break;
        }
    }
    public void GracePeriod()
    {
        StartCoroutine(nameof(SpawnDonny));
    }
    IEnumerator SpawnDonny()
    {
        yield return new WaitForSeconds(SafePeriodTime);
        Transform spawnPoint = FindSpawnPoint();
        if (spawnPoint != null)
        {
            Monster.position = spawnPoint.position;
            photonView.RPC(nameof(Activate), RpcTarget.AllBuffered);
        }
        yield return new WaitForEndOfFrame();
        photonView.RPC(nameof(NotifySpawn), RpcTarget.All);
    }
    Transform FindSpawnPoint()
    {
        foreach (Transform point in SpawnPoints)
        {
            bool playersInAttackRange = Physics.CheckSphere(point.position, 20f, whatIsPlayer);
            if (!playersInAttackRange)
            {
                return point;
            }
        }
        return null;
    }
    [PunRPC]
    public void Activate()
    {
        Monster.GetComponent<DonnyAI>().enabled = true;
        Monster.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = true;
        Monster.GetChild(2).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
    }
    [PunRPC]
    public void NotifySpawn()
    {
        StartCoroutine(hudText.SetHud("What is that noise?"));
    }
}
