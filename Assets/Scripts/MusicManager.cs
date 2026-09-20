using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip normalClip;
    [SerializeField] private AudioClip scaredClip;
    [SerializeField] private AudioClip deadClip;
    [SerializeField] private AudioClip startSceneClip;

    private Coroutine introRoutine;

    void Start()
    {
        source.clip = introClip;
        source.loop = false;
        source.Play();

        introRoutine = StartCoroutine(SwitchToNormalMusic());
    }

    private IEnumerator SwitchToNormalMusic()
    {
        // Wait for the shorter of the intro length and 3 seconds
        float waitTime = Mathf.Min(introClip.length, 3f);
        yield return new WaitForSeconds(waitTime);

        introRoutine = null;
        PlayLoop(normalClip);
    }

    // ---- Public methods other scripts can call ----

    public void PlayNormal()     => SwitchTo(normalClip);
    public void PlayScared()     => SwitchTo(scaredClip);
    public void PlayDead()       => SwitchTo(deadClip);
    public void PlayStartScene() => SwitchTo(startSceneClip);

    private void SwitchTo(AudioClip clip)
    {
        // If the intro is still waiting to hand over to normal music, cancel it
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        // Don't restart a track that's already playing
        if (source.clip == clip && source.isPlaying) return;

        PlayLoop(clip);
    }

    private void PlayLoop(AudioClip clip)
    {
        source.clip = clip;
        source.loop = true;
        source.Play();
    }
}