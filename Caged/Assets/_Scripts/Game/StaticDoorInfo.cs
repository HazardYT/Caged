using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
public class StaticDoorInfo : MonoBehaviourPun
{
    public bool isOpen;
    [SerializeField] private bool isJail;
    private AudioSource audioSource;
    [HideInInspector] public Quaternion OgRot;
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
}
