using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

public class DoorInfo : MonoBehaviourPun
{
    private AudioSource audioSource;
    public AudioClip[] openClips;
    public AudioClip[] closeClips;
    public bool isOpen;
    public bool DoorCooldown;
    public bool Z;
    public string KeyName;
    public bool isLocked;
    public Quaternion OgRot;
    public void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        OgRot = transform.localRotation;
        if (isLocked)
        {
            gameObject.GetComponent<NavMeshObstacle>().carving = true;
        }
    }
    public void DoorSound(bool i)
    {
        if (!photonView.IsMine)
            return;
        photonView.RPC(nameof(PlayDoorSound), RpcTarget.AllViaServer, i, photonView.ViewID);
    }

    [PunRPC]
    public void PlayDoorSound(bool isopen, int viewid)
    {
        DoorInfo info = PhotonView.Find(viewid).gameObject.GetComponent<DoorInfo>();
        info.audioSource.pitch = Random.Range(0.8f, 1.2f);
        info.audioSource.volume = Random.Range(0.25f, 0.5f);
        if (isopen) { 
            info.audioSource.clip = info.openClips[Random.Range(0, info.openClips.Length)]; }
        else { 
            info.audioSource.clip = info.closeClips[Random.Range(0, info.closeClips.Length)]; }
        info.audioSource.Play();
    }
    // Normal Door Logic TO
    public IEnumerator Door(Camera playerCam)
    {
        DoorCooldown = true;
        photonView.RPC(nameof(DoorCooldownRPC),RpcTarget.Others, photonView.ViewID, true);
        if (isOpen == true)
        {
            isOpen = false;
            DoorSound(false);
            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = OgRot;
            float angleDiff = Quaternion.Angle(startRot, targetRot);
            float duration = angleDiff / 180f; // adjust duration based on angle difference
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            gameObject.GetComponent<NavMeshObstacle>().carving = false;
            transform.localRotation = targetRot;
        }
        else if (isOpen == false)
        {
            isOpen = true;
            DoorSound(true);
            float duration = 0.5f; // the duration of the rotation in seconds
            Vector3 euler = transform.localRotation.eulerAngles;
            Quaternion newRot = Quaternion.Euler(
                Z ? new Vector3(euler.x, euler.y, playerCam.transform.forward.z < 0 ? -90f : 90f) :
                new Vector3(euler.x, euler.y, playerCam.transform.forward.x < 0 ? 90f : -90f));
            Quaternion startRot = transform.localRotation;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.localRotation = Quaternion.Slerp(startRot, newRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            gameObject.GetComponent<NavMeshObstacle>().carving = true;
            transform.localRotation = newRot;
        }

        photonView.RPC(nameof(SetDoorState), RpcTarget.OthersBuffered, photonView.ViewID, isOpen);

        yield return new WaitForSeconds(0.2f);
        photonView.RPC(nameof(DoorCooldownRPC),RpcTarget.Others, photonView.ViewID, false);
        DoorCooldown = false;
    }
    [PunRPC]
    public void SetLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<DoorInfo>().isLocked = i;
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
    public void DoorCooldownRPC(int viewid,bool i){
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<DoorInfo>().DoorCooldown = i;
    }
}
