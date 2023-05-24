using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Rendering.HighDefinition;
public class Flashlight : MonoBehaviourPun
{
    [SerializeField] private HDAdditionalLightData flashLight;
    public AudioSource audioSource;
    public AudioClip[] clips;
    public AudioClip useClip;
    private bool isOn;
    public float BatteryPercentage = 100;
    private float drainTimer;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private float DepletionRate;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float intensityMax;
    [SerializeField] private float rangeMax;
    [SerializeField] private float spotangleMin;
    [SerializeField] private float intensityDefault = 75;
    [SerializeField] private float rangeDefault = 35;
    [SerializeField] private float spotangleDefault = 100;
    [Header("Info")]
    [SerializeField] float angle = 75;
    [SerializeField] float range = 35;
    [SerializeField] float intensity = 100;
    private bool isZooming;
    
    private void Awake(){
        photonView.RPC(nameof(PlaySound), RpcTarget.All, photonView.ViewID, 1);
    }
    public void BatteryDrain()
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
        if (BatteryPercentage <= 0f)
        {
            photonView.RPC(nameof(FlashLightToggle),RpcTarget.All, photonView.ViewID, false);
        }
    }
    public void Update()
    {
        if (!photonView.IsMine)
            return;
        if (flashLight.gameObject.activeSelf) { BatteryDrain(); }
        
        if (UserInput.instance.FlashlightTogglePressed){
            ToggleFlashLight();
        }
        if (UserInput.instance.UsePressed){
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
        if (UserInput.instance.FlashlightZoomHeld){
            isZooming = true;
            angle -= zoomSpeed * 2 * Time.deltaTime;
            angle = Mathf.Clamp(angle, spotangleMin, spotangleDefault);
            range += zoomSpeed * Time.deltaTime;
            range = Mathf.Clamp(range, rangeDefault, rangeMax);
            intensity += zoomSpeed * 4 * Time.deltaTime;
            intensity = Mathf.Clamp(intensity, intensityDefault, intensityMax);
            photonView.RPC(nameof(FlashlightZoom),RpcTarget.AllViaServer, photonView.ViewID, angle, range, intensity);
        }
        else if (UserInput.instance.FlashlightZoomReleased){
            isZooming = false;
        }
        if (!isZooming){
            angle = Mathf.Lerp(angle, spotangleDefault, zoomSpeed * Time.deltaTime);
            range = Mathf.Lerp(range, rangeDefault, zoomSpeed * Time.deltaTime);
            intensity = Mathf.Lerp(intensity, intensityDefault, zoomSpeed * Time.deltaTime);
            photonView.RPC(nameof(FlashlightZoom),RpcTarget.AllViaServer, photonView.ViewID, angle, range, intensity);
        }
    }
    public void ToggleFlashLight(){
        if (BatteryPercentage > 0){
            isOn = !isOn;
            photonView.RPC(nameof(FlashLightToggle), RpcTarget.All, photonView.ViewID, isOn);
            photonView.RPC(nameof(PlaySound), RpcTarget.AllViaServer, photonView.ViewID, isOn);
        }
    }
    [PunRPC]
    public void FlashlightZoom(int viewId, float angle, float range, float intensity)
    {
        PhotonView view = PhotonView.Find(viewId);
        HDAdditionalLightData data = view.gameObject.GetComponent<Flashlight>().flashLight;
        data.SetSpotAngle(angle, angle / 2);
        data.range = range;
        data.intensity = intensity;

    }
    [PunRPC]
    public void FlashLightToggle(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        Flashlight FL = view.gameObject.GetComponent<Flashlight>();
        FL.flashLight.gameObject.SetActive(i);
    }
    [PunRPC]
    public void PlaySound(int viewid, bool i)
    {
        PhotonView view = PhotonView.Find(viewid);
        Flashlight aud = view.gameObject.GetComponent<Flashlight>();
        if (i) {aud.audioSource.clip = clips[0];}
        if (!i) {aud.audioSource.clip = clips[1];}
        aud.audioSource.Play();
    }
}
