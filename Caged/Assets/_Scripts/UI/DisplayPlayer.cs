using UnityEngine;
using Photon.Pun;
public class DisplayPlayer : MonoBehaviourPun
{
    [SerializeField] private Transform contentTransform;
    public void CallPlayerChange()
    {
        // Get the actor number of the local player
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // Loop through all child objects of the content transform
        foreach (Transform child in contentTransform)
        {
            // Check if the child has a PhotonView component
            PhotonView childPhotonView = child.GetComponent<PhotonView>();
            if (childPhotonView != null)
            {
                // Check if the actor number of the child's PhotonView matches the actor number of the local player
                if (childPhotonView.OwnerActorNr == actorNumber)
                {
                    child.GetComponent<PlayerObjects>().ChangePlayer();
                }
            }
        }
    }
}
