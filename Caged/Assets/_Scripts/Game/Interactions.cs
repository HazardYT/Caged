using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
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
    public Camera playerCam;
    public InventoryManager _inventoryManager;
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
                        _inventoryManager.Interact(hit);
                        if (hit.collider.gameObject.GetComponent<ItemInfo>().isValueable) { photonView.RPC(nameof(RPCValueablesSound),RpcTarget.AllViaServer, photonView.ViewID); }
                    }
                    if (hit.collider.gameObject.CompareTag("Door") && !DoorCooldown)
                    {
                        PhotonView doorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        DoorInfo DI = doorview.gameObject.GetComponent<DoorInfo>();
                        if (DI.DoorCooldown) {return;}
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (doorview.Owner != PhotonNetwork.LocalPlayer) { doorview.RequestOwnership(); }
                        if (DI.isLocked){
                            bool foundKey = false;
                            if (IM.Equipped.childCount > 0 && IM.Equipped.GetChild(0).name == DI.KeyName){
                                IM.RemoveEquippedItem();
                                foundKey = true;
                            }
                            else{
                                for (int i = 0; i < IM.Slots.Length; i++){
                                    if (IM.Slots[i] == DI.KeyName){
                                        IM.RemoveSlotItem(i);
                                        foundKey = true;
                                        break;}
                                }
                            }
                            if (foundKey){
                                doorview.RPC(nameof(DI.SetLockState), RpcTarget.AllBufferedViaServer, doorview.ViewID, false);
                                StartCoroutine(hudText.SetHud("Door is Unlocked!", Color.green));
                            }
                            else if (!foundKey){
                                StartCoroutine(hudText.SetHud("The Door is Locked..\nRequires " + DI.KeyName, Color.red));
                            }
                        }
                        else{
                            StartCoroutine(DI.Door(playerCam));
                        }
                    }
                    // Light Switch Logic
                    if (hit.collider.gameObject.CompareTag("LightSwitch"))
                    {
                        PhotonView lightview = hit.collider.gameObject.GetComponentInChildren<PhotonView>();
                        LightInfo LI = lightview.gameObject.GetComponentInChildren<LightInfo>();
                        if (LI.LightSwitchCooldown) { return; }
                        if (lightview.Owner != PhotonNetwork.LocalPlayer) { lightview.RequestOwnership(); }
                        StartCoroutine(LI.LightSwitchToggle());
                    }
                    // Static Door Logic
                    if (hit.collider.gameObject.CompareTag("StaticDoor") ^ hit.collider.gameObject.CompareTag("CageDoor"))
                    {
                        bool Cage = false;
                        if (hit.collider.gameObject.CompareTag("CageDoor")) { Cage = true;}
                        PhotonView sdoorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        StaticDoorInfo SDI = sdoorview.gameObject.GetComponent<StaticDoorInfo>();
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (SDI.StaticDoorCooldown) { return; }
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
                                    photonView.RPC(nameof(SDI.SetStaticLockState), RpcTarget.AllViaServer, sdoorview.ViewID, false);
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
                                StartCoroutine(SDI.StaticDoor(true));
                            else 
                                StartCoroutine(SDI.StaticDoor(false));
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
                            StartCoroutine(SDI.StaticDoor(false));
                        }
                    }
                    // SurveillanceMonitor Logic
                    if (hit.collider.gameObject.CompareTag("SurveillanceMonitor"))
                    { 
                        SurveillanceMonitor monitor = hit.collider.transform.root.GetComponent<SurveillanceMonitor>();
                        PhotonView view = monitor.GetComponent<PhotonView>();
                        if (view.Owner != PhotonNetwork.LocalPlayer) { view.RequestOwnership(); }
                        EventSystem eventSystem = monitor.eventSystem.GetComponent<EventSystem>();
                        GraphicRaycaster raycaster = monitor.monitorCanvas.GetComponent<GraphicRaycaster>();
                        monitor.eventSystem.SetActive(true);
                        monitor.monitorCanvas.worldCamera = playerCam;
                        PointerEventData pointerEventData = new PointerEventData(eventSystem);
                        pointerEventData.position = Input.mousePosition;
                        // Raycast to find the UI element under the cursor
                        List<RaycastResult> results = new List<RaycastResult>();
                        raycaster.Raycast(pointerEventData, results);

                        // Check if a button is found
                        foreach (RaycastResult result in results){
                            Button button = result.gameObject.GetComponent<Button>();
                            if (button != null){
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
                        if (DI.DrawerCooldown) {return;}
                        if (drawerview.Owner != PhotonNetwork.LocalPlayer) { drawerview.RequestOwnership(); }
                        StartCoroutine(DI.Drawer());
                    }
                    if (hit.collider.gameObject.CompareTag("NPC") && hit.collider.gameObject.GetComponent<NPCAI>().enabled == false){
                        int id = hit.collider.gameObject.GetComponent<PhotonView>().ViewID;
                        photonView.RPC(nameof(EnableNPC),RpcTarget.AllViaServer, id);
                    }
                }
            }
        }
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