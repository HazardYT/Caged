using Photon.Pun;
using UnityEngine.AI;

public class DonnyRPC : MonoBehaviourPun
{
    [PunRPC]
    public void DonnyCatching(int playerid, int viewid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        PhotonView view = PhotonView.Find(viewid);
        playerview.gameObject.transform.SetParent(view.transform.GetChild(0).transform);
        playerview.gameObject.GetComponent<PlayerMovement>().enabled = false;
        playerview.gameObject.GetComponent<Interactions>().enabled = false;
        playerview.gameObject.tag = "Grabbed";
        playerview.gameObject.layer = 11;
    }
    [PunRPC]
    public void DonnyRelease(int playerid)
    {
        PhotonView playerview = PhotonView.Find(playerid);
        playerview.transform.SetParent(null);
        playerview.gameObject.GetComponent<PlayerMovement>().enabled = true;
        playerview.gameObject.GetComponent<Interactions>().enabled = true;
        playerview.gameObject.tag = "Player";
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
