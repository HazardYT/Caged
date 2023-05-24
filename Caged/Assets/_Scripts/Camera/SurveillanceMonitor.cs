using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class SurveillanceMonitor : MonoBehaviourPun
{
    [SerializeField] private byte currentCamID = 1;
    public AudioSource audioSource;
    public GameObject eventSystem;
    public RawImage screenImage;
    public Canvas monitorCanvas;
    public List<SurveillanceCamera> _cameras = new List<SurveillanceCamera>();
    public List<AudioClip> _buttonSounds = new List<AudioClip>();
    public bool isScreenOn = false;

    public void TogglePower(){
        switch(isScreenOn){
            case true:
                isScreenOn = false;
                photonView.RPC(nameof(TogglePowerRPC),RpcTarget.AllViaServer, photonView.ViewID, false);
                break;
            case false:
                isScreenOn = true;
                photonView.RPC(nameof(TogglePowerRPC),RpcTarget.AllViaServer, photonView.ViewID, true);
                break;
        }
        photonView.RPC(nameof(PlayButtonSoundRPC),RpcTarget.AllViaServer, photonView.ViewID);
    }
    public void CamSwitch(bool i){
        if (!isScreenOn){ print("Surveillance Monitor is Off cant Cycle _cameras!"); return;}
        switch(i){
            case true:
                if (currentCamID == 11) { currentCamID = 1; break;}
                currentCamID++;
                break;
            case false:
                if (currentCamID == 1) { currentCamID = 11; break;}
                currentCamID--;
                break;
        }
        photonView.RPC(nameof(CameraChangeLogicRPC),RpcTarget.AllViaServer, photonView.ViewID, currentCamID);
        photonView.RPC(nameof(PlayButtonSoundRPC),RpcTarget.AllViaServer, photonView.ViewID);
    }
    [PunRPC]
    public void CameraChangeLogicRPC(int viewid, byte id)
    {
        PhotonView view = PhotonView.Find(viewid);
        SurveillanceMonitor _surveillanceMonitor = view.gameObject.GetComponent<SurveillanceMonitor>();
        _surveillanceMonitor.currentCamID = id;
        foreach(SurveillanceCamera _cam in _surveillanceMonitor._cameras){
            if (_cam.id != id && _cam.cam.isActiveAndEnabled)
            {
                _cam.cam.enabled = false;
            }
            else if (_cam.id == id)
            {
                _cam.cam.enabled = true;
                _surveillanceMonitor.screenImage.texture = _cam.renderTexture;
            }
        }
    }
    [PunRPC]
    public void TogglePowerRPC(int viewid, bool i){
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.GetComponent<SurveillanceMonitor>().screenImage.enabled = i;
    }
    [PunRPC]
    public void PlayButtonSoundRPC(int viewid){
        PhotonView view = PhotonView.Find(viewid);
        SurveillanceMonitor _survCam = view.GetComponent<SurveillanceMonitor>();
        int i = Random.Range(0, _survCam._buttonSounds.Count);
        _survCam.audioSource.clip = _survCam._buttonSounds[i];
        _survCam.audioSource.Play();
    }
}
