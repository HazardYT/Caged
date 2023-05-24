using UnityEngine;
using Photon.Pun;
public class SurveillanceCamera : MonoBehaviourPun
{
    public Camera cam;
    public RenderTexture renderTexture;
    public byte id;

    public void Start(){
        if (id == 1) { cam.enabled = true; }
    }
}
