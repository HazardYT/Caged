using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class Flashlight : MonoBehaviourPun
{
    [SerializeField] private Light flashLight;
    public AudioSource audioSource;
    public AudioClip[] clips;
    private bool isOn;
    public float BatteryPercentage = 100;
    private float drainTimer;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private float DepletionRate;
    private int state = 0;
    
    private void Awake()
    {
        photonView.RPC(nameof(DefaultState), RpcTarget.All, photonView.ViewID);
        photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 1);
    }
    public void ToggleFlashLight(){
        isOn = !isOn;
        photonView.RPC(nameof(FlashLightToggle), RpcTarget.All, photonView.ViewID, isOn);
        photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 0);
    }
    public void Update()
    {
        if (!photonView.IsMine)
            return;
        BatteryDrain();
        if (UserInput.instance.FlashlightTogglePressed && BatteryPercentage > 0)
        {
            ToggleFlashLight();
        }
        if (UserInput.instance.UsePressed)
        {
            InventoryManager iv = transform.root.gameObject.GetComponent<InventoryManager>();
            if (iv.Equipped.childCount > 0)
            {
                if (iv.Equipped.GetChild(0).name == "Battery")
                {
                    iv.RemoveEquippedItem();
                    BatteryPercentage = 100;
                }
            }
        }
        if (UserInput.instance.FlashlightModePressed)
        {
            if (state > 1)
            {
                state = 0;
            }
            else state++;
            GetState();

        }
    }
    public void BatteryDrain()
    {
        if (flashLight.gameObject.activeSelf)
        {
            BatteryPercentage -= Time.fixedDeltaTime * DepletionRate;
            BatteryPercentage = Mathf.Clamp(BatteryPercentage, 0f, 100f);

            batterySlider.value = 
                BatteryPercentage == 0f ? 0 :
                BatteryPercentage <= 25f ? 1 :
                BatteryPercentage <= 50f ? 2 :
                BatteryPercentage <= 75f ? 3 : 
                4;
            batterySlider.fillRect.GetComponent<Image>().color =
                BatteryPercentage == 0f ? Color.black :
                BatteryPercentage <= 25f ? Color.red :
                BatteryPercentage <= 50f ? Color.yellow :
                BatteryPercentage <= 75f ? Color.green :
                Color.green;
        }
        if (BatteryPercentage <= 0f)
        {
            photonView.RPC(nameof(FlashLightToggle),RpcTarget.All, photonView.ViewID, false);
        }
    }
    private void GetState()
    {
        switch (state)
        {
            case 0:
                photonView.RPC(nameof(DefaultState), RpcTarget.All, photonView.ViewID);
                photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 1);
                break;
            case 1:
                photonView.RPC(nameof(WideState), RpcTarget.All, photonView.ViewID);
                photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 2);
                break;
            case 2:
                photonView.RPC(nameof(ZoomState), RpcTarget.All, photonView.ViewID);
                photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 3);
                break;
        }
    }
    [PunRPC]
    public void DefaultState(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        Light flashlight = view.gameObject.GetComponent<Flashlight>().flashLight;
        flashlight.innerSpotAngle = 70;
        flashlight.spotAngle = 75;
        flashlight.intensity = 100;
        flashlight.range = 35;
    }
    [PunRPC]
    public void WideState(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        Light flashlight = view.gameObject.GetComponent<Flashlight>().flashLight;
        flashlight.innerSpotAngle = 105;
        flashlight.spotAngle = 110;
        flashlight.intensity = 60;
        flashlight.range = 20;
    }
    [PunRPC]
    public void ZoomState(int viewid)
    {
        PhotonView view = PhotonView.Find(viewid);
        Light flashlight = view.gameObject.GetComponent<Flashlight>().flashLight;
        flashlight.innerSpotAngle = 30;
        flashlight.spotAngle = 35;
        flashlight.intensity = 300;
        flashlight.range = 85;
    }
    [PunRPC]
    public void FlashLightToggle(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        Flashlight FL = view.gameObject.GetComponent<Flashlight>();
        FL.flashLight.gameObject.SetActive(i);
    }
    [PunRPC]
    public void PlaySound(int viewid, int i)
    {
        PhotonView view = PhotonView.Find(viewid);
        Flashlight aud = view.gameObject.GetComponent<Flashlight>();
        aud.audioSource.clip = clips[i];
        aud.audioSource.Play();
    }
}
