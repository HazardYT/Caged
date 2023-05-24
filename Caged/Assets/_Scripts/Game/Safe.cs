using TMPro;
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Safe : MonoBehaviourPun
{
    public string SafeCode;
    [SerializeField] private bool timer = false;
    public Transform safeCamPos;
    public TextMeshProUGUI codeText;
    public AudioClip[] audioClips;
    public Canvas ButtonCanvas;
    public Transform SafeDoor;
    public Button enter;
    public Button reset;
    public GameObject eventSystem;
    public bool setuptoggle = false;
    Quaternion ogRot;
    Vector3 ogPos;
    [SerializeField] private float rot;
    private Quaternion OpenRot;
    private PlayerMovement currentController;
    public AudioSource audioSource;
    private PhotonView doorView;

    public void Awake()
    {
        Vector3 euler = transform.localRotation.eulerAngles;
        OpenRot = Quaternion.Euler(new Vector3(euler.x, rot, euler.z));
        doorView = SafeDoor.gameObject.GetComponent<PhotonView>();
        codeText.color = Color.red;
        codeText.text = "0000";
        if (PhotonNetwork.IsMasterClient)
        {
            SafeCode = Random.Range(1000, 9999).ToString();
            photonView.RPC(nameof(SafeCodeSet), RpcTarget.AllBuffered, SafeCode);
        }
    }
    private void Update()
    {
        if (photonView.IsMine)
        {
            if (setuptoggle)
            {
                if (UserInput.instance.ExitPressed && currentController.transform.GetComponentInChildren<InGameSettingsMenu>().menu.gameObject.activeSelf == false)
                {
                    ExitSafe();
                }
            }
        }
    }
    [PunRPC]
    public void SafeCodeSet(string i)
    {
        SafeCode = i;
    }
    public void Setup(PlayerMovement controller,Vector3 pos, Quaternion rot, Player player)
    {
        controller.gameObject.GetComponent<PlayerMovement>().isInGUI = true;
        photonView.TransferOwnership(player);
        photonView.RPC(nameof(ToggleSetup), RpcTarget.All, photonView.ViewID, true);
        eventSystem.SetActive(true);
        ogPos = pos;
        ogRot = rot;
        currentController = controller;
        ButtonCanvas.worldCamera = currentController.playerCam;
        controller.anim.enabled = false;
        controller.playerCam.transform.rotation = safeCamPos.rotation;
        controller.playerCam.transform.position = safeCamPos.position;
        controller.enabled = false;
        currentController.canvas.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void ExitSafe()
    {
        if (photonView.IsMine)
        {
            currentController.gameObject.GetComponent<PlayerMovement>().isInGUI = false;
            currentController.enabled = true;
            currentController.canvas.enabled = true;
            currentController.playerCam.transform.position = ogPos;
            currentController.playerCam.transform.rotation = ogRot;
            currentController.anim.enabled = true;
            eventSystem.SetActive(false);
            photonView.RPC(nameof(ToggleSetup), RpcTarget.All, photonView.ViewID, false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            photonView.TransferOwnership(0);
        }
    }
    [PunRPC]
    public void ToggleSetup(int viewid,bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.GetComponent<Safe>().setuptoggle = i;
    }
    [PunRPC]
    public void LockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.GetComponent<StaticDoorInfo>().isLocked = i;
    }
    public void EnterCodeCall()
    {
        Debug.Log("EnterCodeCall triggered");
        if (!timer && codeText.text.Length == 4)
        {
            StartCoroutine(CheckCode());
        }
    }
    public void ResetCodeCall()
    {
        Debug.Log("ResetCodeCall triggered");
        if (!timer && codeText.text != "0000")
        {
            StartCoroutine(ResetCode());
        }
    }
    public IEnumerator CheckCode()
    {
        timer = true;
        string code = codeText.text;
        codeText.text = "";
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        yield return new WaitForSeconds(0.3f);
        codeText.text = code;
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        yield return new WaitForSeconds(0.3f);
        codeText.text = "";
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        yield return new WaitForSeconds(0.3f);
        codeText.text = code;
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        if (codeText.text != SafeCode)
        {
            photonView.RPC(nameof(TextColorRed), RpcTarget.All);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ResetCode());
            Debug.Log("Wrong Code");
        }
        else if (codeText.text == SafeCode)
        {
            photonView.RPC(nameof(TextColorGreen), RpcTarget.All);
            photonView.RPC(nameof(SafeSound), RpcTarget.All, photonView.ViewID, 5);
            yield return new WaitForSeconds(0.2f);
            photonView.RPC(nameof(SafeSound), RpcTarget.All, photonView.ViewID, 6);
            Interactions interactions = currentController.gameObject.GetComponent<Interactions>();
            PhotonView sview = SafeDoor.GetComponent<PhotonView>();
            StaticDoorInfo SDI = sview.gameObject.GetComponent<StaticDoorInfo>();
            if (sview.Owner != PhotonNetwork.LocalPlayer)
            {
                sview.RequestOwnership();
            }
            StartCoroutine(interactions.StaticDoor(SDI, sview.ViewID, false));
            photonView.RPC(nameof(LockState), RpcTarget.AllBuffered, doorView.ViewID, false);
            ExitSafe();
            Debug.Log("Correct Code");
        }
        timer = false;
    }
    public IEnumerator ResetCode()
    {
        photonView.RPC(nameof(SafeSound), RpcTarget.All, photonView.ViewID, 4);
        timer = true;
        photonView.RPC(nameof(TextColorRed), RpcTarget.All);
        codeText.text = "0000";
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        yield return new WaitForSeconds(0.5f);
        codeText.text = "0000";
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        timer = false;
    }
    public void Type(string n)
    {
        int clip = Random.Range(0, 3);
        if (codeText.text == "0000")
        {
            codeText.text = "";
        }
        if (codeText.text.Length < 4)
        {
            codeText.text = codeText.text + n;
        }
        else
        {
            Debug.Log("4 CHaracters bitch");
        }
        photonView.RPC(nameof(RPCText), RpcTarget.All, photonView.ViewID, codeText.text);
        photonView.RPC(nameof(SafeSound), RpcTarget.All, photonView.ViewID, clip);
    }
    [PunRPC]
    public void TextColorRed()
    {
        codeText.color = Color.red;
    }
    [PunRPC]
    public void TextColorGreen()
    {
        codeText.color = Color.green;
    }
    [PunRPC]
    public void RPCText(int viewid, string text)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.GetComponent<Safe>().codeText.text = text;
    }
    [PunRPC]
    public void SafeSound(int viewid, int clip)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioSource audioS = view.gameObject.GetComponent<Safe>().audioSource;
        audioS.clip = audioClips[clip];
        audioS.Play();
    }
}
