using UnityEngine;
using Photon.Pun;
public class LightInfo : MonoBehaviourPun
{
    public bool isOn;
    public bool isLocked;
    public Transform Light;
    public Transform Light2;
    public Transform _lightSwitch;
    public Material offMat, onMat;
    [SerializeField] private AudioClip OnClip;
    [SerializeField] private AudioClip OffClip;
    [SerializeField] private AudioSource audioSource;

    public void LightSwitchSound(bool i){
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(PlayLightSwitchSound), RpcTarget.AllViaServer, i, photonView.ViewID);
    }

    [PunRPC]
    public void PlayLightSwitchSound(bool isopen, int viewid)
    {
        LightInfo info = PhotonView.Find(viewid).gameObject.GetComponent<LightInfo>();
        if (isopen) { 
            info.audioSource.clip = info.OnClip;
            }
        else{
            info.audioSource.clip = info.OffClip;
        }
        info.audioSource.Play();
    }
}
