using UnityEngine;
using Photon.Pun;

public class ItemInfo : MonoBehaviourPun
{
    public bool locked = false;
    public bool isValueable;
    public int ValueableWorthMin;
    public int ValueableWorthMax;
    public AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    private void OnCollisionEnter(Collision collision){
        if (locked || isValueable) {return;}
        int clipNum = Random.Range(0, audioClips.Length);
        photonView.RPC(nameof(RPCItemSound), RpcTarget.AllViaServer, photonView.ViewID, clipNum);
    }
    [PunRPC]
    public void RPCItemSound(int viewid,int num){
        PhotonView view = PhotonView.Find(viewid);
        ItemInfo itemSound = view.gameObject.GetComponent<ItemInfo>();
        itemSound.audioSource.clip = itemSound.audioClips[num];
        itemSound.audioSource.Play();
    }
}
