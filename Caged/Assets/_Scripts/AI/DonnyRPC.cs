using Photon.Pun;
using UnityEngine.AI;
using UnityEngine;

public class DonnyRPC : MonoBehaviourPun
{
    [PunRPC]
    public void DonnyCatching(int playerid, int viewid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        PhotonView view = PhotonView.Find(viewid);
        playerview.transform.SetParent(view.transform.GetChild(0).transform.GetChild(0).transform);
        playerview.GetComponent<PlayerMovement>().enabled = false;
        playerview.GetComponent<InventoryManager>().enabled = false;
        playerview.GetComponent<CharacterController>().enabled = false;
        playerview.GetComponent<CapsuleCollider>().enabled = false;
        playerview.GetComponent<Interactions>().enabled = false;
        playerview.tag = "Grabbed";
        playerview.gameObject.layer = 11;
    }
    [PunRPC]
    public void DonnyRelease(int playerid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        playerview.transform.SetParent(null);
        playerview.GetComponent<PlayerMovement>().enabled = true;
        playerview.GetComponent<InventoryManager>().enabled = true;
        playerview.GetComponent<CharacterController>().enabled = true;
        playerview.GetComponent<CapsuleCollider>().enabled = true;
        playerview.GetComponent<Interactions>().enabled = true;
        playerview.tag = "Player";
        playerview.gameObject.layer = 8;
    }

    [PunRPC]
    public void SetDoorState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        DoorInfo info = view.transform.GetComponent<DoorInfo>();
        info.isOpen = isOpen;
        view.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    public void SetStaticDoorState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        StaticDoorInfo info = view.transform.GetComponent<StaticDoorInfo>();
        info.isOpen = isOpen;
        view.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
}
