using UnityEngine;
using Photon.Pun;

public enum SurfaceType
{
    Default,
    Grass,
    Wood,
    Cement,
    Vent
}

public class FootstepManager : MonoBehaviourPun
{
    public AudioSource audioSource;
    public AudioClip[] grassFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] cementFootsteps;
    public AudioClip[] ventFootsteps;

    public float proneVolume = 0.05f;
    public float crouchVolume = 0.1f;
    public float walkingVolume = 0.3f;
    public float runningVolume = 0.5f;

    public void PlayFootstep(SurfaceType surfaceType, bool prone, bool crouching, bool walking, bool running)
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
        if (prone)
        {
            audioSource.volume = proneVolume;
        }
        else if (crouching)
        {
            audioSource.volume = crouchVolume;
        }
        else if (walking)
        {
            audioSource.volume = walkingVolume;
        }
        else if (running)
        {
            audioSource.volume = runningVolume;
        }
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();

        photonView.RPC("SyncFootsteps", RpcTarget.Others, surfaceType, randomIndex, photonView.ViewID, prone, crouching, walking, running);
    }

    [PunRPC]
    public void SyncFootsteps(SurfaceType surfaceType, int clipIndex, int viewid, bool prone, bool crouching, bool walking, bool running)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioSource audioSource = view.gameObject.GetComponent<FootstepManager>().audioSource;
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

        if (prone)
        {
            audioSource.volume = proneVolume;
        }
        else if (crouching)
        {
            audioSource.volume = crouchVolume;
        }
        else if (walking)
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