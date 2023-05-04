using UnityEngine;
using Photon.Pun;

public class DonnyFootstepManager : MonoBehaviourPun
{
    [SerializeField] private DonnyAI AI;
    [SerializeField] private ProceduralAnimation PA;
    public AudioSource audioSource;
    public AudioClip SpawnClip;
    public AudioClip[] grassFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] cementFootsteps;
    public AudioClip[] ventFootsteps;

    //Variables
    [SerializeField] private LayerMask groundLayerMask;

    public float crouchingVolume = 0.2f;
    public float walkingVolume = 0.3f;
    public float runningVolume = 0.5f;
    [SerializeField] float distanceThreshold = 0.5f;

    public Vector3 lastRightFootPos;
    public Vector3 lastLeftFootPos;
    private float rightFootTimer = 0f;
    private float leftFootTimer = 0f;
    public float footstepInterval = 0.5f;
    private void Start()
    {
        audioSource.volume = 1;
        audioSource.PlayOneShot(SpawnClip);
    }
    public void Update()
    {
        rightFootTimer += Time.deltaTime;
        leftFootTimer += Time.deltaTime;

        if (Physics.Raycast(PA.rightFootTarget.position, Vector3.down, distanceThreshold, groundLayerMask) && rightFootTimer >= footstepInterval)
        {
            Vector3 currentRightFootPosition = PA.rightFootTarget.position;
            if (currentRightFootPosition != lastRightFootPos)
            {
                SurfaceType surfaceType = DetectSurfaceType();
                PlayFootstep(surfaceType, AI._walking, AI._running);
            }
            lastRightFootPos = currentRightFootPosition;
            rightFootTimer = 0f;
        }
        if (Physics.Raycast(PA.leftFootTarget.position, Vector3.down, distanceThreshold, groundLayerMask) && leftFootTimer >= footstepInterval)
        {
            Vector3 currentLeftFootPosition = PA.leftFootTarget.position;
            if (currentLeftFootPosition != lastLeftFootPos)
            {
                SurfaceType surfaceType = DetectSurfaceType();
                PlayFootstep(surfaceType, AI._walking, AI._running);
            }
            lastLeftFootPos = currentLeftFootPosition;
            leftFootTimer = 0f;
        }
    }

    private SurfaceType DetectSurfaceType()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayerMask))
        {
            switch (hit.collider.tag)
            {
                case "Grass":
                    return SurfaceType.Grass;
                case "Wood":
                    return SurfaceType.Wood;
                case "Cement":
                    return SurfaceType.Cement;
                case "Vent":
                    return SurfaceType.Vent;
                default:
                    return SurfaceType.Default;
            }
        }
        return SurfaceType.Default;
    }
    public void PlayFootstep(SurfaceType surfaceType, bool walking, bool running)
    {
        AudioClip[] footstepSounds;

        switch (surfaceType)
        {
            case SurfaceType.Grass:
                footstepSounds = grassFootsteps;
                break;
            case SurfaceType.Wood:
                footstepSounds = woodFootsteps;
                break;
            case SurfaceType.Cement:
                footstepSounds = cementFootsteps;
                break;
            case SurfaceType.Vent:
                footstepSounds = ventFootsteps;
                break;
            default:
                footstepSounds = woodFootsteps;
                break;
        }

        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.clip = footstepSounds[randomIndex];

        if (walking)
        {
            audioSource.volume = walkingVolume;
        }
        else if (running)
        {
            audioSource.volume = runningVolume;
        }

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();

        photonView.RPC("SyncFootsteps", RpcTarget.Others, surfaceType, randomIndex, photonView.ViewID, walking, running, audioSource.volume);
    }

    [PunRPC]
    public void SyncFootsteps(SurfaceType surfaceType, int clipIndex, int viewid,bool walking, bool running, float volume)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioSource audioSource = view.gameObject.GetComponent<DonnyFootstepManager>().audioSource;
        AudioClip[] footstepSounds;

        switch (surfaceType)
        {
            case SurfaceType.Grass:
                footstepSounds = grassFootsteps;
                break;
            case SurfaceType.Wood:
                footstepSounds = woodFootsteps;
                break;
            case SurfaceType.Cement:
                footstepSounds = cementFootsteps;
                break;
            default:
                footstepSounds = woodFootsteps;
                break;
        }
        audioSource.clip = footstepSounds[clipIndex];
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }
}