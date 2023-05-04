using UnityEngine;
using Photon.Pun;

public class DonnyFootstepManager : MonoBehaviourPun
{
    [SerializeField] private DonnyAI AI;
    public AudioSource audioSource;
    public AudioClip SpawnClip;
    public AudioClip[] grassFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] cementFootsteps;
    public AudioClip[] ventFootsteps;

    //Variables
    [SerializeField] private LayerMask groundLayerMask;
    public float walkingFootstepDistance = 13.5f;
    public float runningFootstepDistance = 8;
    public float crouchingFootstepDistance = 16;
    public float proneFootstepDistance = 20f;
    private float _footstepDistanceCounter;

    public float walkingVolume = 0.3f;
    public float runningVolume = 0.5f;

    private void Start()
    {
        audioSource.volume = 1;
        audioSource.PlayOneShot(SpawnClip);
    }
   /* public void Update()
    {
        float footstepDistance = AI._walking ? walkingFootstepDistance : runningFootstepDistance;
        if (AI.agent.speed == AI.agentWalkSpeed && AI.agent.velocity != Vector3.zero)
        {
            _footstepDistanceCounter += AI.agent.speed * 5 * Time.deltaTime;
        }
        else if (AI.agent.speed == AI.agentRunSpeed && AI.agent.velocity != Vector3.zero)
        {
            _footstepDistanceCounter += AI.agent.speed * 3 * Time.deltaTime;
        }
        if (_footstepDistanceCounter >= footstepDistance)
        {
            _footstepDistanceCounter = 0;
            SurfaceType surfaceType = DetectSurfaceType();
            PlayFootstep(surfaceType, AI._walking, AI._running);
        }
    */

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
                footstepSounds = grassFootsteps;
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

        photonView.RPC("SyncFootsteps", RpcTarget.Others, surfaceType, randomIndex, photonView.ViewID, walking, running);
    }

    [PunRPC]
    public void SyncFootsteps(SurfaceType surfaceType, int clipIndex, int viewid,bool walking, bool running)
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
                footstepSounds = grassFootsteps;
                break;
        }

        audioSource.clip = footstepSounds[clipIndex];

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
    }
}