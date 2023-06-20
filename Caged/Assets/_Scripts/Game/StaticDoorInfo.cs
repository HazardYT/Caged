using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Collections;
public class StaticDoorInfo : MonoBehaviourPun
{
    public bool isOpen;
    [SerializeField] private bool isJail;
    private AudioSource audioSource;
    [HideInInspector] public Quaternion OgRot;
    public bool StaticDoorCooldown = false;
    [HideInInspector] public Quaternion OpenRot;
    [SerializeField] private int rotation;
    public bool isLocked;
    public string KeyName;
    public float _speedFactor;
    public bool _Zaxis;
    
    public List<AudioClip> openClips = new List<AudioClip>();
    public List<AudioClip> closeClips = new List<AudioClip>();
    public void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();   
        if (!_Zaxis){
        Vector3 euler = transform.localRotation.eulerAngles;
        OpenRot = Quaternion.Euler(new Vector3(euler.x, rotation, euler.z));
        }
        else {
        Vector3 euler = transform.localRotation.eulerAngles;
        OpenRot = Quaternion.Euler(new Vector3(euler.x, euler.y, rotation));   
        }
        OgRot = transform.localRotation;
    }

    public void StaticDoorSound(bool i)
    {
        if (!photonView.IsMine)
            return;
        photonView.RPC(nameof(PlayStaticDoorSound), RpcTarget.AllViaServer, i, photonView.ViewID, isJail);
    }

    [PunRPC]
    public void PlayStaticDoorSound(bool isopen, int viewid, bool i)
    {
        StaticDoorInfo info = PhotonView.Find(viewid).gameObject.GetComponent<StaticDoorInfo>();
        info.audioSource.pitch = Random.Range(0.8f, 1.2f);
        if (!i) {info.audioSource.volume = Random.Range(0.25f, 0.5f); }
        if (isopen) { 
            info.audioSource.clip = info.openClips[Random.Range(0, info.openClips.Count)]; }
        else { 
            info.audioSource.clip = info.closeClips[Random.Range(0, info.closeClips.Count)]; }
        info.audioSource.Play();
    }
    public IEnumerator StaticDoor(bool i)
    {
        StaticDoorCooldown = true;
        photonView.RPC(nameof(StaticDoorCooldownRPC),RpcTarget.Others, photonView.ViewID, true);
        if (!isOpen)
        {
            isOpen = true;
            StaticDoorSound(true);
            gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;
            float elapsedTime = 0f;
            float timeToRotate = _speedFactor;
            while (elapsedTime < timeToRotate)
            {
                transform.localRotation = Quaternion.Slerp(OgRot, OpenRot, elapsedTime / timeToRotate);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localRotation = OpenRot;
            if (i) { yield return new WaitForSeconds(3f); StartCoroutine(StaticDoor(false)); photonView.RPC(nameof(SetStaticLockState), RpcTarget.AllViaServer, photonView.ViewID, true); yield break; }
        }
        else
        {
            isOpen = false;
            StaticDoorSound(false);
            gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = false;
            float duration = _speedFactor;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.localRotation = Quaternion.Slerp(OpenRot, OgRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localRotation = OgRot;
        }
        photonView.RPC(nameof(SetStaticDoorState), RpcTarget.OthersBuffered, photonView.ViewID, isOpen);
        photonView.RPC(nameof(StaticDoorCooldownRPC),RpcTarget.Others, photonView.ViewID, false);
        StaticDoorCooldown = false;
    }
    // RPCS for Networking States.
    [PunRPC]
    void SetStaticDoorState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        StaticDoorInfo info = view.transform.GetComponent<StaticDoorInfo>();
        info.isOpen = isOpen;
        info.gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    public void SetStaticLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<StaticDoorInfo>().isLocked = i;
    }
    [PunRPC]
    public void StaticDoorCooldownRPC(int viewid,bool i){
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<StaticDoorInfo>().StaticDoorCooldown = i;
    }
}
