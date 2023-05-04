using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class DonnyDoorOpener : MonoBehaviourPun
{
    private bool DoorCooldown = false;
    public bool StaticDoorCooldown = false;
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

    IEnumerator Door(DoorInfo info, int viewid)
    {
        info.isOpen = true;
        donnyAI.doorStates[viewid] = info.isOpen;
        StartCoroutine(EnableListeningAfterDelay(2f));
        info.DoorSound(true);
        float duration = 0.5f;
        Vector3 euler = info.transform.localRotation.eulerAngles;
        Quaternion newRot = Quaternion.Euler(
            info.Z ? new Vector3(euler.x, euler.y, DonnyCam.transform.forward.z < 0 ? -90f : 90f) :
            new Vector3(euler.x, euler.y, DonnyCam.transform.forward.x < 0 ? 90f : -90f));
        Quaternion startRot = info.transform.localRotation;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            info.transform.localRotation = Quaternion.Slerp(startRot, newRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        info.transform.localRotation = newRot;
        photonView.RPC(nameof(DonnyRPC.SetDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        yield return new WaitForSeconds(1.5f);
        DoorCooldown = false;
    }
    public IEnumerator StaticDoor(StaticDoorInfo info, int viewid)
    {
        info.isOpen = true;
        donnyAI.doorStates[viewid] = info.isOpen;
        StartCoroutine(EnableListeningAfterDelay(2f));
        float elapsedTime = 0f;
        while (elapsedTime < 0.4f)
        {
            info.transform.localRotation = Quaternion.Slerp(info.OgRot, info.OpenRot, elapsedTime / 0.4f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        info.transform.localRotation = info.OpenRot;
        photonView.RPC(nameof(DonnyRPC.SetStaticDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        yield return new WaitForSeconds(3f);
        StaticDoorCooldown = false;
    }
    public IEnumerator EnableListeningAfterDelay(float delay)
    {
        donnyAI.isListening = false;
        yield return new WaitForSeconds(delay);
        donnyAI.isListening = true;
    }
}
