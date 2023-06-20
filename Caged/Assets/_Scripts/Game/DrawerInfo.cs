using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;
public class DrawerInfo : MonoBehaviourPun
{
    public bool isOpen;
    public bool DrawerCooldown = false;
    [HideInInspector] public Vector3 ClosePos = new Vector3(0,0,0);
    public Vector3 OpenPos = new Vector3(-0.0065f, 0 ,0);

    // Drawer Logic TO
    public IEnumerator Drawer()
    {
        DrawerCooldown = true;
        photonView.RPC(nameof(DrawerCooldownRPC),RpcTarget.Others, photonView.ViewID, true);
        if (!isOpen)
        {
            isOpen = true;
            gameObject.GetComponent<NavMeshObstacle>().carving = true;
            float elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                transform.localPosition = Vector3.Slerp(ClosePos, OpenPos, elapsedTime / 0.3f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = OpenPos;
        }
        else
        {
            isOpen = false;
            gameObject.GetComponent<NavMeshObstacle>().carving = false;
            float duration = 0.3f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.localPosition= Vector3.Slerp(OpenPos, ClosePos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = ClosePos;
        }
        photonView.RPC(nameof(SetDrawerState), RpcTarget.OthersBuffered, photonView.ViewID, isOpen);
        photonView.RPC(nameof(DrawerCooldownRPC),RpcTarget.Others, photonView.ViewID, false);
        DrawerCooldown = false;
    }
    
    [PunRPC]
    public void SetDrawerState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        DrawerInfo info = view.transform.GetComponent<DrawerInfo>();
        info.isOpen = isOpen;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    public void DrawerCooldownRPC(int viewid,bool i){
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<DrawerInfo>().DrawerCooldown = i;
    }
}
