using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource source;
    public AudioClip introClip;
    public AudioClip normalClip;
    public AudioClip scaredClip;
    public AudioClip deadClip;
    public AudioClip startSceneClip;

    Coroutine introRoutine;

    void Start()
    {
        source.clip = introClip;
        source.loop = false;
        source.Play();

        introRoutine = StartCoroutine(PlayNormalAfterIntro());
    }

    IEnumerator PlayNormalAfterIntro()
    {
        
        float wait = Mathf.Min(introClip.length, 3f);
        yield return new WaitForSeconds(wait);

        PlayLoop(normalClip);
    }

    public void PlayNormal()
    {
        SwitchTo(normalClip);
    }

    public void PlayScared()
    {
        SwitchTo(scaredClip);
    }

    public void PlayDead()
    {
        SwitchTo(deadClip);
    }

    public void PlayStartScene()
    {
        SwitchTo(startSceneClip);
    }

    void SwitchTo(AudioClip clip)
    {
        // stop the intro timer so it doesn't override this music later
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
        }

        
        if (source.clip == clip && source.isPlaying)
        {
            return;
        }

        PlayLoop(clip);
    }

    void PlayLoop(AudioClip clip)
    {
        source.clip = clip;
        source.loop = true;
        source.Play();
    }
}
 
