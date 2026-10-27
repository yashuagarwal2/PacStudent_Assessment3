using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource source;
    public AudioClip introClip;
    public AudioClip normalClip;
    public AudioClip scaredClip;
    public AudioClip deadClip;
    public AudioClip startSceneClip;

    public void PlayIntro()
    {
        source.clip = introClip;
        source.loop = false;
        source.Play();
    }

    public void PlayNormal()
    {
        PlayLoop(normalClip);
    }

    public void PlayScared()
    {
        PlayLoop(scaredClip);
    }

    public void PlayDead()
    {
        PlayLoop(deadClip);
    }

    public void PlayStartScene()
    {
        PlayLoop(startSceneClip);
    }

    void PlayLoop(AudioClip clip)
    {
        if (source.clip == clip && source.isPlaying)
        {
            return;
        }

        source.clip = clip;
        source.loop = true;
        source.Play();
    }
}