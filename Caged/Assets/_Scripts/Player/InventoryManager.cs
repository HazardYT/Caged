using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviourPun
{
    [SerializeField] private int ItemCount;
    public int InventorySlotCount = 2;
    public Transform playerCam;
    public Transform Equipped = null;
    private HudText hudText;
    private GameManager Manager;
    public float maxThrowDistance = 20f;
    [SerializeField] private Slider slider;
    private float throwTimer = 0f;
    private bool isThrowing = false;
    [SerializeField] private TextMeshProUGUI useText;
    int currentSlotController = 0;
    public int CurrentSlot;
    private float slotSwitchDelay = 0.05f;
    private float slotSwitchTimer = 0f;

    public string[] Slots;
    public Image[] SlotImage;
    public Transform[] fill;
    public Image[] equippedslot;

    public void Start()
    {
        Manager = GameObject.FindObjectOfType<GameManager>();
        hudText = GameObject.FindObjectOfType<HudText>();
        if (!photonView.IsMine)
        {
            this.enabled = false;
        }
        for (int i = 0; i < InventorySlotCount; i++)
        {
            Slots[i] = null;
            fill[i].gameObject.SetActive(true);
            equippedslot[i].gameObject.SetActive(true);
        }

    }
    IEnumerator DestroyObject(PhotonView pView)
    {
        while (!pView.IsMine)
        {
            yield return null;
        }
        AddItem(pView.transform.name);
        PhotonNetwork.Destroy(pView.gameObject);
    }
    public void Interact(RaycastHit hit)
    {
        bool hasItems = false;
        for (int j = 0; j < InventorySlotCount; j++)
        {
            if (Slots[j] == null)
            {
            hasItems = true;
            break;
            }
        }
        if (!hasItems)
        {
            Debug.Log("Cant Add item Inventory is Full hehe");
            return;
        }
        PhotonView hitview = PhotonView.Find(hit.transform.GetComponent<PhotonView>().ViewID);
        ItemInfo info = hitview.gameObject.GetComponent<ItemInfo>();
        if (!hitview.IsMine && hitview != null)
        {
            hitview.RequestOwnership();
            StartCoroutine(DestroyObject(hitview));
        }
        else
        {           
            if (info.isValueable){
                if (info.ValueableWorthMax != 0) {
                    int i = Random.Range(info.ValueableWorthMin, info.ValueableWorthMax);
                    photonView.RPC(nameof(AddMoneyToManager),RpcTarget.AllViaServer, i);
                    StartCoroutine(hudText.SetHud("+ $" + i, Color.green));
                }
                else{
                    photonView.RPC(nameof(AddMoneyToManager),RpcTarget.AllViaServer, info.ValueableWorthMin);
                    StartCoroutine(hudText.SetHud("+ $" + info.ValueableWorthMin, Color.green));
                }
            }
            else{
                AddItem(hitview.transform.name);
                StartCoroutine(hudText.SetHud("Picked up " + hitview.transform.name, Color.white));
            }
            PhotonNetwork.Destroy(hitview.gameObject);

        }
    }
    [PunRPC]
    public void AddMoneyToManager(int amount){
        Manager._moneyCollected += amount;
        Manager.moneyText.text = "$" + Manager._moneyCollected;
    }
    public void Update()
    {
        if (!photonView.IsMine)
            return;

        slotSwitchTimer -= Time.deltaTime;

        if (UserInput.instance.LeftBumperPressed && slotSwitchTimer <= 0f)
        {
            int nextSlotController = currentSlotController;

            do
            {
                nextSlotController++;
                if (nextSlotController >= InventorySlotCount)
                {
                    nextSlotController = 0;
                }

                if (Slots[nextSlotController] != null)
                {
                    currentSlotController = nextSlotController;
                    EquipItem(currentSlotController);
                    slotSwitchTimer = slotSwitchDelay;
                    break;
                }

            } while (nextSlotController != currentSlotController);
        }

        if (UserInput.instance.RightBumperPressed && slotSwitchTimer <= 0f)
        {
            int nextSlotController = currentSlotController;

            do
            {
                nextSlotController--;
                if (nextSlotController < 0)
                {
                    nextSlotController = InventorySlotCount - 1;
                }

                if (Slots[nextSlotController] != null)
                {
                    currentSlotController = nextSlotController;
                    EquipItem(currentSlotController);
                    slotSwitchTimer = slotSwitchDelay;
                    break;
                }

            } while (nextSlotController != currentSlotController);
        }

        if (UserInput.instance.Slot1Pressed && slotSwitchTimer <= 0f)
        {
            if (InventorySlotCount > 0)
            {
                if (Slots[0] != null)
                {
                    EquipItem(0);
                    slotSwitchTimer = slotSwitchDelay;
                }
            }
        }
        if (UserInput.instance.Slot2Pressed && slotSwitchTimer <= 0f)
        {
            if (InventorySlotCount > 1)
            {
                if (Slots[1] != null)
                {
                    EquipItem(1);
                    slotSwitchTimer = slotSwitchDelay;
                }
            }
        }
        if (UserInput.instance.Slot3Pressed && slotSwitchTimer <= 0f)
        {
            if (InventorySlotCount > 2)
            {
                if (Slots[2] != null)
                {
                    EquipItem(2);
                    slotSwitchTimer = slotSwitchDelay;
                }
            }
        }
        if (UserInput.instance.Slot4Pressed && slotSwitchTimer <= 0f)
        {
            if (InventorySlotCount > 3)
            {
                if (Slots[3] != null)
                {
                    EquipItem(3);
                    slotSwitchTimer = slotSwitchDelay;
                }
            }
        }
        if (UserInput.instance.Slot5Pressed && slotSwitchTimer <= 0f)
        {
            if (InventorySlotCount > 4)
            {
                if (Slots[4] != null)
                {
                    EquipItem(4);
                    slotSwitchTimer = slotSwitchDelay;
                }
            }
        }
        if (UserInput.instance.ThrowHeld)
        {
            if (Equipped.childCount > 0)
            {
                isThrowing = true;
                slider.gameObject.SetActive(true);
            }
        }
        if (isThrowing)
        {
            throwTimer += Time.deltaTime * 4;
            slider.value = Mathf.Clamp(throwTimer * 5f, 0f, maxThrowDistance);
        }
        if (UserInput.instance.ThrowReleased)
        {
            if (Equipped.childCount > 0)
            {
                ThrowItem(Equipped.GetChild(0).name, slider.value);
            }   
            slider.gameObject.SetActive(false);
            isThrowing = false;
            throwTimer = 0f;
            
        }
    }
    public void AddItem(string name)
    {
        for (int i = 0; i < InventorySlotCount; i++)
        {
            if (Slots[i] == null)
            {
                if (SlotImage[i].enabled == false)
                {
                    SlotImage[i].enabled = true;
                }
                string parseString = FindArrayNumber(name);
                int arrayNumber = int.Parse(parseString);
                SlotImage[i].sprite = ItemManager.instance.ItemSprites[arrayNumber];
                Debug.Log("Added Item to slot: " + arrayNumber);
                Slots[i] = name;
                ItemCount++;
                EquipItem(i);
                return;
            } 
        }
    }
    public void DropAllItems(){
        for (int i = 0; i < InventorySlotCount; i++)
        {
            if (Slots[i] != null){ DropItem(i);}
        }
        if (Equipped.childCount != 0) {        
            string itemname = Equipped.GetChild(0).name;
        PhotonNetwork.Destroy(Equipped.GetChild(0).gameObject);
        GameObject obj = PhotonNetwork.Instantiate("Items/" + itemname, transform.position, Quaternion.identity);
        photonView.RPC(nameof(ItemSettingsRPC),RpcTarget.AllViaServer, obj.GetComponent<PhotonView>().ViewID, itemname);
        }
    }
    public void DropItem(int i){
        if (Slots[i] != null){
        string itemname = Slots[i];
        Slots[i] = null;
        SlotImage[i].enabled = false;
        ItemCount--;
        PhotonNetwork.Instantiate("Items/" + itemname, transform.position, Quaternion.identity);
        }
    }
    public void RemoveEquippedItem()
    {
        PhotonNetwork.Destroy(Equipped.GetChild(0).gameObject);
        Slots[CurrentSlot] = null;
        SlotImage[CurrentSlot].enabled = false;
        equippedslot[CurrentSlot].enabled = false;
        ItemCount--;
    }
    public void RemoveSlotItem(int i){
        Slots[i] = null;
        SlotImage[i].enabled = false;
        ItemCount--;
    }
    public void EquipItem(int i)
    {
        bool hasItems = false;
        for (int j = 0; j < InventorySlotCount; j++)
        {
            if (Slots[j] != null)
            {
                hasItems = true;
                break;
            }
        }
        if (!hasItems)
        {
            Debug.Log("You have no Items to Equip!");
            return;
        }
        for (int k = 0; k < equippedslot.Length; k++)
        {
            if (equippedslot[k].isActiveAndEnabled)
            {
                if (k == i) // Equipping the same slot
                {
                    return;
                }
                else
                {
                    PhotonNetwork.Destroy(Equipped.GetChild(0).gameObject); // Destroy the currently equipped item
                    break;
                }
            }
        }
        for (int k = 0; k < equippedslot.Length; k++)
        {
            if (k != i)
            {
                equippedslot[k].enabled = false;
            }
        }
        equippedslot[i].enabled = true;
        if (Equipped.childCount == 0)
        {
            GameObject obj = PhotonNetwork.Instantiate("Items/" + Slots[i], Equipped.position, Equipped.rotation);
            obj.transform.SetParent(Equipped);
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            obj.gameObject.GetComponent<ItemInfo>().locked = true;
            obj.name = Slots[i];
            obj.tag = "Equipped";
            photonView.RPC(nameof(EquipRPC), RpcTarget.Others, obj.GetComponent<PhotonView>().ViewID, photonView.ViewID);
            CurrentSlot = i;
            return;
        }
        if (Equipped.childCount > 0)
        {
            PhotonNetwork.Destroy(Equipped.GetChild(0).gameObject);
            GameObject obj = PhotonNetwork.Instantiate("Items/" + Slots[i], Equipped.position, Equipped.rotation);
            obj.transform.SetParent(Equipped);
            obj.gameObject.GetComponent<ItemInfo>().locked = true;
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            obj.name = Slots[i];
            obj.tag = "Equipped";
            photonView.RPC(nameof(EquipRPC), RpcTarget.Others, obj.GetComponent<PhotonView>().ViewID, photonView.ViewID);
            CurrentSlot = i;
            return;
        }
        if (Equipped.GetChild(0).name == "Battery")
        {
            useText.text = UserInput.instance._useAction.GetBindingDisplayString();
            useText.enabled = true;
        }
        else useText.enabled = false;
    }



    public void ThrowItem(string name, float force)
    {
        PhotonNetwork.Destroy(Equipped.GetChild(0).gameObject);
        GameObject obj = PhotonNetwork.Instantiate("Items/" + name, playerCam.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
        obj.name = name;
        obj.tag = "Item";
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(playerCam.transform.forward * force, ForceMode.Impulse);
        obj.gameObject.GetComponent<ItemInfo>().locked = false;
        obj.GetComponent<PhotonView>().TransferOwnership(0);
        Slots[CurrentSlot] = null;
        SlotImage[CurrentSlot].enabled = false;
        equippedslot[CurrentSlot].enabled = false;
        photonView.RPC(nameof(ItemSettingsRPC), RpcTarget.OthersBuffered, obj.GetComponent<PhotonView>().ViewID, name);
        ItemCount--;
    }
    public string FindArrayNumber(string T)
    {
        for (int i = 0; i < ItemManager.instance.ItemNames.Count; i++)
        {
            if (ItemManager.instance.ItemNames[i] == T)
            {
                return i.ToString();
            }
        }
        return null;
    }

    [PunRPC]
    public void ItemSettingsRPC(int viewid, string name)
    {
        PhotonView view = PhotonView.Find(viewid);
        view.gameObject.name = name;
        view.gameObject.tag = "Item";
        view.gameObject.GetComponent<ItemInfo>().locked = false;
        view.TransferOwnership(0);
    }
    [PunRPC]
    public void EquipRPC(int objid, int viewid)
    {
        PhotonView Objview = PhotonView.Find(objid);
        PhotonView view = PhotonView.Find(viewid);
        Transform viewIM = view.gameObject.GetComponentInChildren<InventoryManager>().Equipped;
        Objview.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Objview.gameObject.transform.SetParent(viewIM);
        Objview.gameObject.tag = "Equipped";
        Objview.gameObject.GetComponent<ItemInfo>().locked = true;
        Objview.gameObject.transform.position = viewIM.position;
        Objview.gameObject.transform.rotation = viewIM.rotation;
    }
}

