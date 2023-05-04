using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class DonnyDoorCloser : MonoBehaviourPun
{
    private bool DoorCooldown = false;
    [SerializeField] private DonnyAI donnyAI;
    [SerializeField] private DonnyDoorOpener doorOpener;

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
            if (!DoorCooldown && DI.isOpen)
            {
                StartCoroutine(Door(DI, doorview.ViewID));
            }
        }
    }

    IEnumerator Door(DoorInfo info, int viewid)
    {
        DoorCooldown = true;
        info.isOpen = false;
       // donnyAI.doorStates[viewid] = info.isOpen;
        StartCoroutine(doorOpener.EnableListeningAfterDelay(1.2f));
        info.DoorSound(false);
        Quaternion startRot = info.transform.localRotation;
        Quaternion targetRot = info.OgRot;
        float angleDiff = Quaternion.Angle(startRot, targetRot);
        float duration = angleDiff / 180f; // adjust duration based on angle difference
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            info.transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        info.gameObject.GetComponent<NavMeshObstacle>().carving = false;
        info.transform.localRotation = targetRot;
        photonView.RPC(nameof(DonnyRPC.SetDoorState), RpcTarget.All, viewid, info.isOpen);
        DoorCooldown = false;
    }
}
