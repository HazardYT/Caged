using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interactions : MonoBehaviourPun
{
    public AudioSource audioSource;
    public AudioClip[] ValueablesPickupClips;
    public float InteractDistance = 4f;
    private bool DoorCooldown = false;
    private bool StaticDoorCooldown = false;
    private bool DrawerCooldown = false;
    private bool LightCooldown = false;
    private Camera playerCam;
    public InventoryManager InventoryManager;
    private HudText hudText;
    bool isinteract = true;

    // Setting up variables if the view is mine
    private void Awake(){
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
                    if (hit.collider.gameObject.CompareTag("LightSwitch") && !LightCooldown)
                    {
                        LightCooldown = true;
                        PhotonView lightview = hit.collider.gameObject.GetComponentInChildren<PhotonView>();
                        LightInfo LI = lightview.gameObject.GetComponentInChildren<LightInfo>();
                        if (lightview.Owner != PhotonNetwork.LocalPlayer) { lightview.RequestOwnership(); }
                        StartCoroutine(LightSwitchToggle(LI, lightview.ViewID));
                    }
                    // Static Door Logic
                    if (hit.collider.gameObject.CompareTag("StaticDoor") ^ hit.collider.gameObject.CompareTag("CageDoor") && !StaticDoorCooldown)
                    {
                        bool Cage = false;
                        if (hit.collider.gameObject.CompareTag("CageDoor")) { Cage = true;}
                        PhotonView sdoorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        StaticDoorInfo SDI = sdoorview.gameObject.GetComponent<StaticDoorInfo>();
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (sdoorview.Owner != PhotonNetwork.LocalPlayer) { sdoorview.RequestOwnership(); }
                        if (SDI.isLocked){
                            bool foundKey = false;
                            if (IM.Equipped.childCount > 0 && IM.Equipped.GetChild(0).name == SDI.KeyName){
                                IM.RemoveEquippedItem();
                                foundKey = true;
                            }
                            else{
                                for (int i = 0; i < IM.Slots.Length; i++)
                                {
                                    if (IM.Slots[i] == SDI.KeyName)
                                    {
                                        IM.RemoveSlotItem(i);
                                        foundKey = true;
                                        break;
                                    }
                                }
                            }
                                if (foundKey){
                                    photonView.RPC(nameof(SetStaticLockState), RpcTarget.AllViaServer, sdoorview.ViewID, false);
                                    StartCoroutine(hudText.SetHud("Door is Unlocked!", Color.green));
                                }
                                else if (!foundKey)
                                {
                                    StartCoroutine(hudText.SetHud("The Door is Locked..\nRequires " + SDI.KeyName, Color.red));
                                }
                        }
                        else
                        {
                            if (Cage)
                                StartCoroutine(StaticDoor(SDI, sdoorview.ViewID, true));
                            else 
                                StartCoroutine(StaticDoor(SDI, sdoorview.ViewID, false));
                        }
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
                            StartCoroutine(StaticDoor(SDI, sview.ViewID, false));
                        }
                    }
                    // SurveillanceMonitor Logic
                    if (hit.collider.gameObject.CompareTag("SurveillanceMonitor"))
                    { 
                        print("CAM Found Monitor");
                        SurveillanceMonitor monitor = hit.collider.transform.root.GetComponent<SurveillanceMonitor>();
                        PhotonView view = monitor.GetComponent<PhotonView>();
                        if (view.Owner != PhotonNetwork.LocalPlayer) { view.RequestOwnership(); }
                        EventSystem eventSystem = monitor.eventSystem.GetComponent<EventSystem>();
                        GraphicRaycaster raycaster = monitor.monitorCanvas.GetComponent<GraphicRaycaster>();
                        monitor.eventSystem.SetActive(true);
                        monitor.monitorCanvas.worldCamera = playerCam;
                        PointerEventData pointerEventData = new PointerEventData(eventSystem);
                        pointerEventData.position = Input.mousePosition;
                        print("CAM Calling monitor");
                        // Raycast to find the UI element under the cursor
                        List<RaycastResult> results = new List<RaycastResult>();
                        raycaster.Raycast(pointerEventData, results);

                        // Check if a button is found
                        foreach (RaycastResult result in results){
                            Debug.Log("CAM found one" + result);
                            Button button = result.gameObject.GetComponent<Button>();
                            if (button != null){
                                // Perform a button click
                                button.onClick.Invoke();
                                break; }
                        }
                        view.TransferOwnership(0);
                        monitor.eventSystem.SetActive(false);
                        monitor.monitorCanvas.worldCamera = null;
                    }
                    // Drawer Logic
                    if (hit.collider.gameObject.CompareTag("Drawer") && !DrawerCooldown)
                    {
                        PhotonView drawerview = hit.collider.gameObject.GetComponent<PhotonView>();
                        DrawerInfo DI = drawerview.gameObject.GetComponent<DrawerInfo>();
                        if (drawerview.Owner != PhotonNetwork.LocalPlayer) { drawerview.RequestOwnership(); }
                        StartCoroutine(Drawer(DI, drawerview.ViewID));
                    }
                    if (hit.collider.gameObject.CompareTag("NPC") && hit.collider.gameObject.GetComponent<NPCAI>().enabled == false){
                        int id = hit.collider.gameObject.GetComponent<PhotonView>().ViewID;
                        photonView.RPC(nameof(EnableNPC),RpcTarget.AllViaServer, id);
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
    public IEnumerator StaticDoor(StaticDoorInfo info, int viewid, bool i)
    {
        StaticDoorCooldown = true;
        if (!info.isOpen)
        {
            info.isOpen = true;
            info.StaticDoorSound(true);
            info.gameObject.GetComponent<NavMeshObstacle>().carving = true;
            float elapsedTime = 0f;
            float timeToRotate = info._speedFactor;
            while (elapsedTime < timeToRotate)
            {
                info.transform.localRotation = Quaternion.Slerp(info.OgRot, info.OpenRot, elapsedTime / timeToRotate);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            info.transform.localRotation = info.OpenRot;
            if (i) { yield return new WaitForSeconds(3f); StartCoroutine(StaticDoor(info, viewid, false)); photonView.RPC(nameof(SetStaticLockState), RpcTarget.AllViaServer, viewid, true); yield break; }
        }
        else
        {
            info.isOpen = false;
            info.StaticDoorSound(false);
            info.gameObject.GetComponent<NavMeshObstacle>().carving = false;
            float duration = info._speedFactor;
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
    public IEnumerator LightSwitchToggle(LightInfo info, int viewid)
    {
        if (!info.isLocked)
        {
            Vector3 euler = info._lightSwitch.localRotation.eulerAngles;
            if (info.isOn){
                info.isOn = false;
                info._lightSwitch.transform.localRotation = Quaternion.Euler(new Vector3(0f, euler.y, euler.z)); }
            else{ 
                info.isOn = true;
                info._lightSwitch.transform.localRotation = Quaternion.Euler(new Vector3(-60f, euler.y, euler.z)); }
            info.LightSwitchSound(info.isOn);
        }
        else StartCoroutine(hudText.SetHud("Power is Off!", Color.red));

        photonView.RPC(nameof(ToggleLightRPC), RpcTarget.AllBuffered, viewid, info.isOn);
        yield return new WaitForSeconds(0.1f);
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
    // RPCS for Networking States.
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
    public void SetDrawerState(int viewid, bool isOpen)
    {
        PhotonView view = PhotonView.Find(viewid);
        DrawerInfo info = view.transform.GetComponent<DrawerInfo>();
        info.isOpen = isOpen;
        info.gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
    }
    [PunRPC]
    public void SetLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<DoorInfo>().isLocked = i;
    }
    [PunRPC]
    public void SetStaticLockState(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.GetComponent<StaticDoorInfo>().isLocked = i;
    }
    [PunRPC]
    public void ToggleLightRPC(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        LightInfo LI = view.transform.GetComponent<LightInfo>();
        LI.isOn = i;
        LI.Light.gameObject.SetActive(i);
        if (LI.Light2 != null) { LI.Light2.gameObject.SetActive(i);}
        if (i && LI.needsMaterialSwitch){
            LI.Light.GetComponentInParent<MeshRenderer>().material = LI.onMat;     if (LI.Light2 != null) { LI.Light2.GetComponentInParent<MeshRenderer>().material = LI.onMat; }
        } else if (!i && LI.needsMaterialSwitch) { LI.Light.GetComponentInParent<MeshRenderer>().material = LI.offMat; if (LI.Light2 != null) { LI.Light2.GetComponentInParent<MeshRenderer>().material = LI.offMat; }}
    }
    [PunRPC]
    public void RPCValueablesSound(int viewid){
        PhotonView view = PhotonView.Find(viewid);
        Interactions i = view.GetComponent<Interactions>();
        int sound = Random.Range(0, i.ValueablesPickupClips.Length);
        i.audioSource.clip = ValueablesPickupClips[sound];
        i.audioSource.Play();
    }
    [PunRPC]
    public void EnableNPC(int viewid){
        PhotonView view = PhotonView.Find(viewid);
        NPCAI ai = view.gameObject.GetComponent<NPCAI>();
        ai.enabled = true;
    }
}