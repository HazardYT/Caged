using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class DoorInfo : MonoBehaviourPun
{
    private AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
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

        photonView.RPC(nameof(PlayDoorSound), RpcTarget.All, i, photonView.ViewID);
    }

    [PunRPC]
    public void PlayDoorSound(bool i, int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        DoorInfo DI = view.gameObject.GetComponent<DoorInfo>();
        DI.audioSource.pitch = Random.Range(0.9f, 1.1f);
        if (i)
        {
            DI.audioSource.clip = DI.openClip;
        }
        else
        {
            DI.audioSource.clip = DI.closeClip;
        }
        DI.audioSource.Play();
    }
}
