using UnityEngine;
using Photon.Pun;
public class HeadFollow : MonoBehaviourPun
{
    public Transform cam;
    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            float rot = cam.localRotation.eulerAngles.x;
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rot));
        }
    }
}
