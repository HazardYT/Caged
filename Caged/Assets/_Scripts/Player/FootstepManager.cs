using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
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
    public List<AudioClip>  grassFootsteps = new List<AudioClip>();
    public List<AudioClip>  woodFootsteps = new List<AudioClip>();
    public List<AudioClip>  cementFootsteps = new List<AudioClip>();
    public List<AudioClip>  ventFootsteps = new List<AudioClip>();

    public float proneVolume = 0.05f;
    public float crouchVolume = 0.1f;
    public float walkingVolume = 0.3f;
    public float runningVolume = 0.5f;

    public void PlayFootstep(SurfaceType surfaceType, bool prone, bool crouching, bool walking, bool running)
    {
        List<AudioClip> footstepSounds = new List<AudioClip>();

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

        int randomIndex = Random.Range(0, footstepSounds.Count);
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

        photonView.RPC(nameof(SyncFootsteps), RpcTarget.Others, surfaceType, randomIndex, photonView.ViewID, audioSource.volume);
    }

    [PunRPC]
    public void SyncFootsteps(SurfaceType surfaceType, int clipIndex, int viewid, float volume)
    {
        PhotonView view = PhotonView.Find(viewid);
        AudioSource audioSource = view.gameObject.GetComponent<FootstepManager>().audioSource;
        List<AudioClip> footstepSounds = new List<AudioClip>();

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
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }
}