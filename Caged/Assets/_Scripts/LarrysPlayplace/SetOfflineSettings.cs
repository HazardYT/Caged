using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SetOfflineSettings : MonoBehaviourPun
{
    public static SetOfflineSettings instance;
    // Start is called before the first frame update
    void Awake()
    {
        ToggleSingle(true);
        if(instance == null){instance = this;}
    }
    void ToggleSingle(bool tf){
        PhotonNetwork.OfflineMode = tf;
    }
}
