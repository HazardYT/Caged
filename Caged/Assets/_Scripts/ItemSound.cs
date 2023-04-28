using UnityEngine;
using Photon.Pun;
public class ItemSound : MonoBehaviourPun
{
    public bool locked = false;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    private void OnCollisionEnter(Collision collision)
    {
        if (!locked)
        {
            if (!collision.collider.CompareTag("Player"))
            {
                int clipNum = Random.Range(0, audioClips.Length);
                photonView.RPC(nameof(RPCItemSound), RpcTarget.All, photonView.ViewID, clipNum);
            }
        }
    }
    [PunRPC]
    public void RPCItemSound(int viewid,int num)
    {
        PhotonView view = PhotonView.Find(viewid);
        ItemSound itemSound = view.gameObject.GetComponent<ItemSound>();
        itemSound.audioSource.clip = itemSound.audioClips[num];
        itemSound.audioSource.Play();
    }
}
