using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListing : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _roomText;
    
    public RoomInfo RoomInfo { get; private set; }
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomText.text = roomInfo.Name;
    }
}
