using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.AI;

public class Interactions : MonoBehaviourPun
{
    public float InteractLength = 3.5f;
    private bool DoorCooldown = false;
    private bool StaticDoorCooldown = false;
    private bool LightCooldown = false;
    private Camera playerCam;
    public InventoryManager InventoryManager;
    private HudText hudText;
    bool isinteract = true;

    private void Awake()
    {
        if (photonView.IsMine)
            playerCam = gameObject.GetComponent<PlayerMovement>().playerCam;
        hudText = GameObject.Find("GameUI").GetComponent<HudText>();
    }
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

                if (Physics.Raycast(ray, out hit, InteractLength))
                {
                    if (hit.collider.gameObject.CompareTag("Item"))
                    {
                        InventoryManager.Interact(hit);
                    }
                    if (hit.collider.gameObject.CompareTag("Door") && !DoorCooldown)
                    {
                        PhotonView doorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        DoorInfo DI = doorview.gameObject.GetComponent<DoorInfo>();
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (doorview.Owner != PhotonNetwork.LocalPlayer)
                        {
                            doorview.RequestOwnership();
                        }
                        if (DI.isLocked)
                        {
                            if (IM.Equipped.childCount > 0 && (IM.Equipped.GetChild(0).name == DI.KeyName))
                            {
                                IM.RemoveEquippedItem();
                                DI.isLocked = false;
                                photonView.RPC(nameof(SetLockState), RpcTarget.OthersBuffered, doorview.ViewID, false);
                                StartCoroutine(hudText.SetHud("Door is Unlocked!"));
                                
                            }
                            else StartCoroutine(hudText.SetHud("The Door is Locked.. \n Requires " + DI.KeyName));
                        }
                        else
                        {
                            StartCoroutine(Door(DI, doorview.ViewID));
                        }
                    }
                    if (hit.collider.gameObject.CompareTag("LightSwitch")&& !LightCooldown)
                    {
                        LightCooldown = true;
                        PhotonView lightview = hit.collider.gameObject.GetComponent<PhotonView>();
                        LightInfo LI = lightview.gameObject.GetComponent<LightInfo>();
                        if (lightview.Owner != PhotonNetwork.LocalPlayer)
                        {
                            lightview.RequestOwnership();
                        }
                        StartCoroutine(LightSwitchToggle(LI, lightview.ViewID));
                    }
                    if (hit.collider.gameObject.CompareTag("StaticDoor") && !StaticDoorCooldown)
                    {
                        PhotonView sdoorview = hit.collider.gameObject.GetComponent<PhotonView>();
                        StaticDoorInfo SDI = sdoorview.gameObject.GetComponent<StaticDoorInfo>();
                        InventoryManager IM = photonView.gameObject.GetComponent<InventoryManager>();
                        if (sdoorview.Owner != PhotonNetwork.LocalPlayer)
                        {
                            sdoorview.RequestOwnership();
                        }
                        StartCoroutine(StaticDoor(SDI, sdoorview.ViewID));
                    }
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
                            if (sview.Owner != PhotonNetwork.LocalPlayer)
                            {
                                sview.RequestOwnership();
                            }
                            StartCoroutine(StaticDoor(SDI, sview.ViewID));
                        }
                    }
                }
            }
        }
    }
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
        else StartCoroutine(hudText.SetHud("Power is Off!"));

        photonView.RPC(nameof(ToggleLightRPC), RpcTarget.AllBuffered, viewid, info.isOn);
        yield return new WaitForSeconds(0.05f);

        LightCooldown = false;
    }

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

        // Call SetDoorState RPC to synchronize door state across all clients

        photonView.RPC(nameof(SetDoorState), RpcTarget.OthersBuffered, viewid, info.isOpen);

        yield return new WaitForSeconds(0.2f);

        DoorCooldown = false;
    }

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

}