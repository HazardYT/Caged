using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.AI;

public class Interactions : MonoBehaviourPun
{
    public AudioSource audioSource;
    public AudioClip[] ValueablesPickupClips;
    public float InteractDistance = 3.5f;
    private bool DoorCooldown = false;
    private bool StaticDoorCooldown = false;
    private bool DrawerCooldown = false;
    private bool LightCooldown = false;
    private Camera playerCam;
    public InventoryManager InventoryManager;
    private HudText hudText;
    bool isinteract = true;

    // Setting up variables if the view is mine
    private void Awake()
    {
        if (photonView.IsMine) {
            playerCam = gameObject.GetComponent<PlayerMovement>().playerCam;
            hudText = GameObject.FindObjectOfType<HudText>();
        }
    }
    // Checking interact button and if so it will raycast to it and whatever tag it hits it will run the respective code.
    public void Update()
    {
        if (!photonView.IsMine)
            return;
        if (UserInput.instance.InteractPressed)
        {
            if (isinteract)
            {
                Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, InteractDistance))
                {
                    if (hit.collider.gameObject.CompareTag("Item"))
                    {
                        InventoryManager.Interact(hit);
                        if (hit.collider.gameObject.GetComponent<ItemInfo>().isValueable) { photonView.RPC(nameof(RPCValueablesSound),RpcTarget.AllViaServer, photonView.ViewID); }
                    }
                    if (hit.collider.gameObject.CompareTag("Door") && !DoorCooldown)
                    {
                        PhotonView doorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        DoorInfo DI = doorview.gameObject.GetComponent<DoorInfo>();
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (doorview.Owner != PhotonNetwork.LocalPlayer) { doorview.RequestOwnership(); }
                        if (DI.isLocked){
                            bool foundKey = false;
                            if (IM.Equipped.childCount > 0 && IM.Equipped.GetChild(0).name == DI.KeyName){
                                IM.RemoveEquippedItem();
                                foundKey = true;
                            }
                            else{
                                for (int i = 0; i < IM.Slots.Length; i++)
                                {
                                    if (IM.Slots[i] == DI.KeyName)
                                    {
                                        IM.RemoveSlotItem(i);
                                        foundKey = true;
                                        break;
                                    }
                                }
                            }
                                if (foundKey){
                                    DI.isLocked = false;
                                    photonView.RPC(nameof(SetLockState), RpcTarget.OthersBuffered, doorview.ViewID, false);
                                    StartCoroutine(hudText.SetHud("Door is Unlocked!", Color.green));
                                }
                                else if (!foundKey)
                                {
                                    StartCoroutine(hudText.SetHud("The Door is Locked..\nRequires " + DI.KeyName, Color.red));
                                }
                        }
                        else
                        {
                            StartCoroutine(Door(DI, doorview.ViewID));
                        }
                    }
                    // Light Switch Logic
                    if (hit.collider.gameObject.CompareTag("LightSwitch")&& !LightCooldown)
                    {
                        LightCooldown = true;
                        PhotonView lightview = hit.collider.gameObject.GetComponent<PhotonView>();
                        LightInfo LI = lightview.gameObject.GetComponent<LightInfo>();
                        if (lightview.Owner != PhotonNetwork.LocalPlayer) { lightview.RequestOwnership(); }
                        StartCoroutine(LightSwitchToggle(LI, lightview.ViewID));
                    }
                    // Static Door Logic
                    if (hit.collider.gameObject.CompareTag("StaticDoor") && !StaticDoorCooldown)
                    {
                        PhotonView sdoorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        StaticDoorInfo SDI = sdoorview.gameObject.GetComponent<StaticDoorInfo>();
                        if (sdoorview.Owner != PhotonNetwork.LocalPlayer) { sdoorview.RequestOwnership(); }
                        StartCoroutine(StaticDoor(SDI, sdoorview.ViewID));
                    }
                    // Safe Logic
                    if (hit.collider.gameObject.CompareTag("Safe"))
                    { 
                        Safe safe = hit.collider.transform.root.GetComponent<Safe>();
                        if (!safe.setuptoggle && safe.SafeDoor.GetComponent<StaticDoorInfo>().isLocked)
                        {
                            safe.Setup(gameObject.GetComponent<PlayerMovement>(), playerCam.transform.position, playerCam.transform.rotation, PhotonNetwork.LocalPlayer);
                        }
                        else if (!safe.SafeDoor.GetComponent<StaticDoorInfo>().isLocked && !StaticDoorCooldown)
                        {
                            PhotonView sview = safe.SafeDoor.GetComponent<PhotonView>();
                            StaticDoorInfo SDI = sview.gameObject.GetComponent<StaticDoorInfo>();
                            if (sview.Owner != PhotonNetwork.LocalPlayer) { sview.RequestOwnership(); }
                            StartCoroutine(StaticDoor(SDI, sview.ViewID));
                        }
                    }
                    // Drawer Logic
                    if (hit.collider.gameObject.CompareTag("Drawer") && !DrawerCooldown)
                    {
                        PhotonView drawerview = hit.collider.gameObject.GetComponent<PhotonView>();
                        DrawerInfo DI = drawerview.gameObject.GetComponent<DrawerInfo>();
                        if (drawerview.Owner != PhotonNetwork.LocalPlayer) { drawerview.RequestOwnership(); }
                        StartCoroutine(Drawer(DI, drawerview.ViewID));
                    }
                }
            }
        }
    }
    // Drawer Logic TO
    public IEnumerator Drawer(DrawerInfo info, int viewid)
    {
        DrawerCooldown = true;
        if (!info.isOpen)
        {
            info.isOpen = true;
            info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
            float elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                info.transform.localPosition = Vector3.Slerp(info.ClosePos, info.OpenPos, elapsedTime / 0.3f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.transform.localPosition = info.OpenPos;
        }
        else
        {
            info.isOpen = false;
            info.gameObject.GetComponent<NavMeshObstacle>().carving = false;
            float duration = 0.3f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                info.transform.localPosition= Vector3.Slerp(info.OpenPos, info.ClosePos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.transform.localPosition = info.ClosePos;
        }
        photonView.RPC(nameof(SetDrawerState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        DrawerCooldown = false;
    }
    // Static Door TO
    public IEnumerator StaticDoor(StaticDoorInfo info, int viewid)
    {
        StaticDoorCooldown = true;
        if (!info.isOpen)
        {
            info.isOpen = true;
            info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
            float elapsedTime = 0f;
            while (elapsedTime < 0.4f)
            {
                info.transform.localRotation = Quaternion.Slerp(info.OgRot, info.OpenRot, elapsedTime / 0.4f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.transform.localRotation = info.OpenRot;
        }
        else
        {
            info.isOpen = false;
            info.gameObject.GetComponent<NavMeshObstacle>().carving = false;
            float duration = 0.4f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                info.transform.localRotation = Quaternion.Slerp(info.OpenRot, info.OgRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.transform.localRotation = info.OgRot;
        }
        photonView.RPC(nameof(SetStaticDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);
        StaticDoorCooldown = false;
    }
    // Light Switch Logic TO
    IEnumerator LightSwitchToggle(LightInfo info, int viewid)
    {
        if (!info.isLocked)
        {
            if (info.isOn)
            {
                info.isOn = false;
            }
            else info.isOn = true;
        }
        else StartCoroutine(hudText.SetHud("Power is Off!", Color.red));

        photonView.RPC(nameof(ToggleLightRPC), RpcTarget.AllBuffered, viewid, info.isOn);
        yield return new WaitForSeconds(0.05f);

        LightCooldown = false;
    }
    // Normal Door Logic TO
    IEnumerator Door(DoorInfo info, int viewid)
    {
        DoorCooldown = true;
        if (info.isOpen == true)
        {
            info.isOpen = false;
            info.DoorSound(false);
            Quaternion startRot = info.transform.localRotation;
            Quaternion targetRot = info.OgRot;
            float angleDiff = Quaternion.Angle(startRot, targetRot);
            float duration = angleDiff / 180f; // adjust duration based on angle difference
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                info.transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.gameObject.GetComponent<NavMeshObstacle>().carving = false;
            info.transform.localRotation = targetRot;
        }
        else if (info.isOpen == false)
        {
            info.isOpen = true;
            info.DoorSound(true);
            float duration = 0.5f; // the duration of the rotation in seconds
            Vector3 euler = info.transform.localRotation.eulerAngles;
            Quaternion newRot = Quaternion.Euler(
                info.Z ? new Vector3(euler.x, euler.y, playerCam.transform.forward.z < 0 ? -90f : 90f) :
                new Vector3(euler.x, euler.y, playerCam.transform.forward.x < 0 ? 90f : -90f));
            Quaternion startRot = info.transform.localRotation;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                info.transform.localRotation = Quaternion.Slerp(startRot, newRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
            info.transform.localRotation = newRot;
        }

        photonView.RPC(nameof(SetDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);

        yield return new WaitForSeconds(0.2f);

        DoorCooldown = false;
    }

    // RPCS for Networking Door States.

    [PunRPC]
    public void SetDoorState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        DoorInfo info = view.transform.GetComponent<DoorInfo>();
        info.isOpen = isOpen;
        view.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    void SetStaticDoorState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        StaticDoorInfo info = view.transform.GetComponent<StaticDoorInfo>();
        info.isOpen = isOpen;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    void SetDrawerState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        DrawerInfo info = view.transform.GetComponent<DrawerInfo>();
        info.isOpen = isOpen;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    void SetLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<DoorInfo>().isLocked = i;
    }
    [PunRPC]
    public void ToggleLightRPC(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        LightInfo LI = view.transform.GetComponent<LightInfo>();
        LI.isOn = i;
        LI.lightSwitch.gameObject.SetActive(i);
    }
    [PunRPC]
    public void RPCValueablesSound(int viewid){
        PhotonView view = PhotonView.Find(viewid);
        Interactions i = view.GetComponent<Interactions>();
        int sound = Random.Range(0, i.ValueablesPickupClips.Length);
        i.audioSource.clip = ValueablesPickupClips[sound];
        i.audioSource.Play();
    }
}