using UnityEngine;
using TMPro;
using Photon.Pun;

public class GameManager : MonoBehaviourPun, IPunObservable
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    private float elapsedTime;
    public bool timerOn = false;
    public float _moneyCollected = 0;

    private int syncedMinutes = 0;
    private int syncedSeconds = 0;

    public void Update()
    {
        if (!timerOn && !PhotonNetwork.IsMasterClient) { return; }
            Timer();
    }

    public void Timer()
    {
        elapsedTime += Time.deltaTime;

        syncedMinutes = Mathf.FloorToInt(elapsedTime / 60f);
        syncedSeconds = Mathf.FloorToInt(elapsedTime % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", syncedMinutes, syncedSeconds);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the timer values to other clients
            stream.SendNext(syncedMinutes);
            stream.SendNext(syncedSeconds);
        }
        else
        {
            // Receive the timer values from the master client
            syncedMinutes = (int)stream.ReceiveNext();
            syncedSeconds = (int)stream.ReceiveNext();

            timeText.text = string.Format("{0:00}:{1:00}", syncedMinutes, syncedSeconds);
        }
    }
}
