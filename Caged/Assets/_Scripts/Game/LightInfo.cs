using UnityEngine;
using Photon.Pun;
using System.Collections;    
public class LightInfo : MonoBehaviourPun
{
    public bool isOn;
    public bool isLocked;
    public bool needsMaterialSwitch = true;
    public bool LightSwitchCooldown = false;
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
    public IEnumerator LightSwitchToggle()
    {
        if (!isLocked)
        {
            LightSwitchCooldown = true;
            photonView.RPC(nameof(LightSwitchCooldownRPC),RpcTarget.Others, photonView.ViewID, true);
            Vector3 euler = _lightSwitch.localRotation.eulerAngles;
            if (isOn){
                isOn = false;
                _lightSwitch.transform.localRotation = Quaternion.Euler(new Vector3(0f, euler.y, euler.z)); }
            else{ 
                isOn = true;
                _lightSwitch.transform.localRotation = Quaternion.Euler(new Vector3(-60f, euler.y, euler.z)); }
            LightSwitchSound(isOn);
        }
        else StartCoroutine(GameObject.FindObjectOfType<HudText>().SetHud("Power is Off!", Color.red));

        photonView.RPC(nameof(LightInfo.ToggleLightRPC), RpcTarget.AllBuffered, photonView.ViewID, isOn);
        yield return new WaitForSeconds(0.1f);
        photonView.RPC(nameof(LightSwitchCooldownRPC),RpcTarget.Others, photonView.ViewID, false);
        LightSwitchCooldown = false;
    }

    [PunRPC]
    public void ToggleLightRPC(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        LightInfo LI = view.transform.GetComponent<LightInfo>();
        LI.isOn = i;
        LI.Light.gameObject.SetActive(i);
        if (LI.Light2 != null) { LI.Light2.gameObject.SetActive(i);}
        if (i && LI.needsMaterialSwitch){
            LI.Light.GetComponentInParent<MeshRenderer>().material = LI.onMat;     if (LI.Light2 != null) { LI.Light2.GetComponentInParent<MeshRenderer>().material = LI.onMat; }
        } else if (!i && LI.needsMaterialSwitch) { LI.Light.GetComponentInParent<MeshRenderer>().material = LI.offMat; if (LI.Light2 != null) { LI.Light2.GetComponentInParent<MeshRenderer>().material = LI.offMat; }}
    }
    [PunRPC]
    public void LightSwitchCooldownRPC(int viewid,bool i){
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<LightInfo>().LightSwitchCooldown = i;
    }
}
