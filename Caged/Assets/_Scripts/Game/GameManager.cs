using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;
public class GameManager : MonoBehaviourPun, IPunObservable
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    public VideoPlayer video;
    public RawImage videoImage;
    private float elapsedTime;
    public bool timerOn = false;
    public float _moneyCollected = 0;
    public DonnyAI donnyAI;

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
    [PunRPC]
    public void StartJumpScare(int viewid){
        GameManager manager = PhotonView.Find(viewid).GetComponent<GameManager>();
        manager.videoImage.enabled = true;
        manager.video.Play();
        StartCoroutine(manager.DonnyJumpScare(viewid));
    }
    public IEnumerator DonnyJumpScare(int viewid){
        GameManager manager = PhotonView.Find(viewid).GetComponent<GameManager>();
        yield return new WaitForSeconds(2f);
        yield return new WaitUntil(() => !manager.video.isPlaying);
        manager.video.Stop();
        manager.videoImage.enabled = false;
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
