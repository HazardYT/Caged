using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using Photon.Realtime;
public class ListPlayers : MonoBehaviourPunCallbacks
{
    public GameObject textPrefab;
    public Transform contentTransform;

    private GameObject playerObject;

    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine(nameof(InstantiatePlayer));
    }
    IEnumerator InstantiatePlayer()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnected);
        yield return new WaitForEndOfFrame();

        // Instantiate the player object and set its parent to the content transform
        Debug.Log("Instantiating player object");
        GameObject newPlayerObject = PhotonNetwork.Instantiate("Other/" + textPrefab.name, Vector3.zero, Quaternion.identity);
        Debug.Log("Player object instantiated: " + newPlayerObject);
        newPlayerObject.transform.SetParent(contentTransform);
        PhotonView playerView = newPlayerObject.gameObject.GetComponent<PhotonView>();
        newPlayerObject.name = "" + playerView.ViewID;
        playerView.TransferOwnership(PhotonNetwork.LocalPlayer);
        Player pl = PhotonNetwork.LocalPlayer;

        // Set the player object's text to the player's nickname
        TextMeshProUGUI text = newPlayerObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        text.text = PhotonNetwork.LocalPlayer.NickName + "  *";

        // Sync the player object with other players
        photonView.RPC(nameof(SyncPlayerObject), RpcTarget.Others, PhotonNetwork.NickName, pl, playerView.ViewID);
    }

    [PunRPC]
    private void SyncPlayerObject(string nickname, Player player, int viewId)
    {
        // Get the existing player object associated with the view ID
        GameObject existingPlayerObject = PhotonView.Find(viewId).gameObject;
        existingPlayerObject.transform.SetParent(contentTransform);
        PhotonView playerView = existingPlayerObject.GetComponent<PhotonView>();
        playerView.TransferOwnership(player);

        // Set the player object's text to the player's nickname
        TextMeshProUGUI text = existingPlayerObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        text.text = nickname;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Loop through all players in the room (excluding the new player)
        for (int i = 0; i < contentTransform.transform.childCount; i++)
        {
            Debug.Log("looking");
            Transform child = contentTransform.transform.GetChild(i);
            PhotonView view = child.gameObject.GetComponent<PhotonView>();
            if (view != null && view.Owner != newPlayer)
            {
                // Sync the existing player object with the new player
                photonView.RPC(nameof(SyncPlayerObject), newPlayer, view.Owner.NickName, view.Owner, view.ViewID);
                Debug.Log("found=================");
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Find the player object associated with the leaving player and destroy it
        Transform content = contentTransform.transform;
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            PhotonView view = child.gameObject.GetComponent<PhotonView>();
            if (view != null && (view.OwnerActorNr == otherPlayer.ActorNumber || view.OwnerActorNr == 0))
            {
                PhotonNetwork.Destroy(child.gameObject);
                break;
            }
        }
    }


}

