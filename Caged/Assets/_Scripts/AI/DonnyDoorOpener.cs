using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class DonnyDoorOpener : MonoBehaviourPun
{
    private bool DoorCooldown = false;
    public bool StaticDoorCooldown = false;
    public StaticDoorInfo CageInfo;
    public Camera DonnyCam;
    [SerializeField] private DonnyAI donnyAI;

    public void Start()
    {
        if (!photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            DonnyCam.GetComponent<Camera>().enabled = false;
            return;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Door"))
        {
            PhotonView doorview = other.gameObject.GetComponent<PhotonView>();
            DoorInfo DI = doorview.gameObject.GetComponent<DoorInfo>();
            if (doorview.Owner != PhotonNetwork.LocalPlayer)
            {
                doorview.RequestOwnership();
            }
            if (DI.isLocked)
            {
                donnyAI.SearchWalkPoint();
                return;
            }
            else if (!DoorCooldown && !DI.isOpen)
            {
                DoorCooldown = true;
                StartCoroutine(Door(DI, doorview.ViewID));
            }
        }
        if (other.gameObject.CompareTag("StaticDoor"))
        {
            PhotonView staticdoorview = other.gameObject.GetComponent<PhotonView>();
            StaticDoorInfo SDI = staticdoorview.gameObject.GetComponent<StaticDoorInfo>();
            if (staticdoorview.Owner != PhotonNetwork.LocalPlayer)
            {
                staticdoorview.RequestOwnership();
            }
            if (!StaticDoorCooldown && !SDI.isOpen)
            {
                StaticDoorCooldown = true;
                StartCoroutine(StaticDoor(SDI, staticdoorview.ViewID));
            }
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Door"))
        {
            PhotonView doorview = other.gameObject.GetComponent<PhotonView>();
            DoorInfo DI = doorview.gameObject.GetComponent<DoorInfo>();
            if (doorview.Owner != PhotonNetwork.LocalPlayer)
            {
                doorview.RequestOwnership();
            }
            if (DI.isLocked)
            {
                donnyAI.SearchWalkPoint();
                return;
            }
            else if (!DoorCooldown && !DI.isOpen)
            {
                DoorCooldown = true;
                StartCoroutine(Door(DI, doorview.ViewID));
            }
        }
        if (other.gameObject.CompareTag("StaticDoor"))
        {
            PhotonView staticdoorview = other.gameObject.GetComponent<PhotonView>();
            StaticDoorInfo SDI = staticdoorview.gameObject.GetComponent<StaticDoorInfo>();
            if (staticdoorview.Owner != PhotonNetwork.LocalPlayer)
            {
                staticdoorview.RequestOwnership();
            }
            if (!StaticDoorCooldown && !SDI.isOpen)
            {
                StaticDoorCooldown = true;
                StartCoroutine(StaticDoor(SDI, staticdoorview.ViewID));
            }
        }
    }
    public IEnumerator LockCage(){
        photonView.RPC(nameof(SetStaticDoorState), RpcTarget.AllViaServer, CageInfo.gameObject.GetComponent<PhotonView>().ViewID, true);
        if (CageInfo.isOpen){
            photonView.RPC(nameof(SetCageSpeed), RpcTarget.AllViaServer, 0.4f);
            StartCoroutine(CageDoorClose(CageInfo.GetComponent<PhotonView>().ViewID));
            yield return new WaitForSeconds(1f);
            photonView.RPC(nameof(SetCageSpeed), RpcTarget.AllViaServer, 1.6f);
        }
    }
    [PunRPC]
    public void SetCageSpeed(float i){
        CageInfo._speedFactor = i;
    }
    IEnumerator Door(DoorInfo info, int viewid)
    {
        donnyAI.anim.SetTrigger("OpenDoor");
        info.isOpen = true;
        donnyAI.DoorStates[viewid] = info.isOpen;
        StartCoroutine(EnableListeningAfterDelay(2f));
        info.DoorSound(true);
        Vector3 euler = info.transform.localRotation.eulerAngles;
        Quaternion newRot = Quaternion.Euler(
            info.Z ? new Vector3(euler.x, euler.y, DonnyCam.transform.forward.z < 0 ? -90f : 90f) :
            new Vector3(euler.x, euler.y, DonnyCam.transform.forward.x < 0 ? 90f : -90f));
        Quaternion startRot = info.transform.localRotation;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            info.transform.localRotation = Quaternion.Slerp(startRot, newRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        info.transform.localRotation = newRot;
        photonView.RPC(nameof(SetDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        yield return new WaitForSeconds(1.5f);
        DoorCooldown = false;
    }
    public IEnumerator StaticDoor(StaticDoorInfo info, int viewid)
    {
        donnyAI.anim.SetTrigger("OpenDoor");
        info.isOpen = true;
        donnyAI.DoorStates[viewid] = info.isOpen;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        StartCoroutine(EnableListeningAfterDelay(2f));
        info.StaticDoorSound(true);
        float elapsedTime = 0f;
        while (elapsedTime < info._speedFactor){
            info.transform.localRotation = Quaternion.Slerp(info.OgRot, info.OpenRot, elapsedTime / info._speedFactor);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        info.transform.localRotation = info.OpenRot;
        photonView.RPC(nameof(SetStaticDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        yield return new WaitForSeconds(2f);
        StaticDoorCooldown = false;
    }
    public IEnumerator CageDoorClose(int viewid){
        CageInfo.isOpen = false;
        donnyAI.DoorStates[viewid] = CageInfo.isOpen;
        CageInfo.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        StartCoroutine(EnableListeningAfterDelay(4f));
        CageInfo.StaticDoorSound(true);
        float elapsedTime = 0f;
        while (elapsedTime < CageInfo._speedFactor){
            CageInfo.transform.localRotation = Quaternion.Slerp(CageInfo.OpenRot, CageInfo.OgRot, elapsedTime / CageInfo._speedFactor);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        CageInfo.transform.localRotation = CageInfo.OgRot;
        photonView.RPC(nameof(SetStaticDoorState), RpcTarget.OthersBuffered, viewid, CageInfo.isOpen);
        photonView.RPC(nameof(SetStaticLockState), RpcTarget.AllBufferedViaServer, viewid, true);
        yield return new WaitForSeconds(2f);
        StaticDoorCooldown = false;
    }
    public IEnumerator EnableListeningAfterDelay(float delay)
    {
        donnyAI.isListening = false;
        yield return new WaitForSeconds(delay);
        donnyAI.isListening = true;
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
    [PunRPC]
    public void SetStaticLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<StaticDoorInfo>().isLocked = i;
    }
}
