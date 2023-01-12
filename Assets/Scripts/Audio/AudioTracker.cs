using UnityEngine;
using System;
using System.Collections;

public class AudioTracker : MonoBehaviour
{
    #region Actions

    // Triggers when an audiosource isPlaying changes to true (play or unpause)
    public Action<AudioTracker> onPlay;

    // Triggers when an audiosource isPlaying changes to false without completing (pause)
    public Action<AudioTracker> onPause;

    // Triggers when an audiosource isPlaying changes to false (stop or played to end)
    public Action<AudioTracker> onComplete;

    // Triggers when an audiosource repeats
    public Action<AudioTracker> onLoop;

    #endregion

    #region Fields & Properties

    // If true, will automatically stop tracking an audiosource when it stops playing
    public bool autoStop = false;

    // The source that this component is tracking
    public AudioSource source { get; private set; }

    // The last tracked time of the audiosource
    private float lastTime;

    // The last tracked value for whether or not the audioSource was playing
    private bool lastIsPlaying;

    private const string trackingCoroutine = "TrackSequence";

    #endregion

    #region Public

    public void Track(AudioSource source)
    {
        Cancel();
        this.source = source;
        if (source != null)
        {
            lastTime = source.time;
            lastIsPlaying = source.isPlaying;
            StartCoroutine(trackingCoroutine);
        }
    }

    public void Cancel()
    {
        StopCoroutine(trackingCoroutine);
    }

    #endregion

    #region Private

    private IEnumerator TrackSequence()
    {
        while (true)
        {
            yield return null;
            SetTime(source.time);
            SetIsPlaying(source.isPlaying);
        }
    }

    private void AudioSourceBegan()
    {
        if (onPlay != null) onPlay(this);
    }

    private void AudioSourceLooped()
    {
        if (onLoop != null)
            onLoop(this);
    }

    private void AudioSourceCompleted()
    {
        if (onComplete != null)
            onComplete(this);
    }

    private void AudioSourcePaused()
    {
        if (onPause != null)
            onPause(this);
    }

    private void SetIsPlaying(bool isPlaying)
    {
        if (lastIsPlaying == isPlaying)
            return;

        lastIsPlaying = isPlaying;

        if (isPlaying)
            AudioSourceBegan();
        else if (Mathf.Approximately(source.time, 0))
            AudioSourceCompleted();
        else
            AudioSourcePaused();

        if (isPlaying == false && autoStop == true)
            StopCoroutine(trackingCoroutine);
    }

    private void SetTime(float time)
    {
        if (lastTime > time) AudioSourceLooped();
        lastTime = time;
    }

    #endregion
}