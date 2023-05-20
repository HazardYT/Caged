using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class DoorInfo : MonoBehaviourPun
{
    private AudioSource audioSource;
    public AudioClip[] openClips;
    public AudioClip[] closeClips;
    public bool isOpen;
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
}
