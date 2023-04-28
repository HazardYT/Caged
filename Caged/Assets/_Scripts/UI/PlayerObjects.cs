using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerObjects : MonoBehaviourPun
{
    public Image playerImage;
    public TextMeshProUGUI playerName;
    public Sprite[] playerSprites;
    public static int currentSelectionIndex;
    public Transform playerObjects;

    void Start()
    {
        playerObjects = GameObject.Find("PLAYERS").transform;
        currentSelectionIndex = 0;
        playerObjects.GetChild(0).gameObject.SetActive(true);
        playerImage.sprite = playerSprites[currentSelectionIndex];
    }

    public void ChangePlayer()
    {
        if (!photonView.IsMine)
            return;
        Debug.Log("1");
        currentSelectionIndex++;
        DisableAllPlayers();
        if (currentSelectionIndex > 3)
        {
            currentSelectionIndex = 0;
        }
        playerObjects.GetChild(currentSelectionIndex).gameObject.SetActive(true);
        Debug.Log("2");
        photonView.RPC(nameof(UpdatePlayerSelection), RpcTarget.All, photonView.ViewID, currentSelectionIndex);
    }

    [PunRPC]
    void UpdatePlayerSelection(int viewid, int selectionIndex)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = playerSprites[selectionIndex];
        Debug.Log("Changed Sprite to: " + selectionIndex + "for playerview: " + viewid + " / " + view.ViewID);
    }
    private void DisableAllPlayers()
    {
        // Disable all players
        for (int i = 0; i < playerObjects.childCount; i++)
        {
            Transform child = playerObjects.transform.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }
}
