using System.Collections.Generic;
using UnityEngine;

public class AudioSequence : MonoBehaviour
{
    #region Enum

    private enum PlayMode
    {
        Stopped,
        Playing,
        Paused
    }

    #endregion

    #region Fields

    private readonly Dictionary<AudioClip, AudioSequenceData> playMap = new();
    private PlayMode playMode = PlayMode.Stopped;
    private double pauseTime;

    #endregion

    #region Public

    public void Play(params AudioClip[] clips)
    {
        if (playMode == PlayMode.Stopped)
            playMode = PlayMode.Playing;
        else if (playMode == PlayMode.Paused)
            UnPause();

        var startTime = GetNextStartTime();
        foreach (var clip in clips)
        {
            var data = GetData(clip);
            data.Schedule(startTime);
            startTime += clip.length;
        }
    }

    public void Pause()
    {
        if (playMode != PlayMode.Playing)
            return;
        playMode = PlayMode.Paused;

        pauseTime = AudioSettings.dspTime;
        foreach (var data in playMap.Values) data.source.Pause();
    }

    public void UnPause()
    {
        if (playMode != PlayMode.Paused)
            return;
        playMode = PlayMode.Playing;

        var elapsedTime = AudioSettings.dspTime - pauseTime;
        foreach (var data in playMap.Values)
        {
            if (data.isScheduled)
                data.Schedule(data.startTime + elapsedTime);
            data.source.UnPause();
        }
    }

    public void Stop()
    {
        playMode = PlayMode.Stopped;
        foreach (var data in playMap.Values) data.Stop();
    }

    public AudioSequenceData GetData(AudioClip clip)
    {
        if (!playMap.ContainsKey(clip))
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            playMap[clip] = new AudioSequenceData(source);
        }

        return playMap[clip];
    }

    #endregion

    #region Private

    private AudioSequenceData GetFirst()
    {
        var lowestStartTime = double.MaxValue;
        AudioSequenceData firstData = null;
        foreach (var data in playMap.Values)
            if (data.isScheduled && data.startTime < lowestStartTime)
            {
                lowestStartTime = data.startTime;
                firstData = data;
            }

        return firstData;
    }

    private AudioSequenceData GetLast()
    {
        var highestEndTime = double.MinValue;
        AudioSequenceData lastData = null;
        foreach (var data in playMap.Values)
            if (data.isScheduled && data.endTime > highestEndTime)
            {
                highestEndTime = data.endTime;
                lastData = data;
            }

        return lastData;
    }

    private double GetNextStartTime()
    {
        var lastToPlay = GetLast();
        if (lastToPlay != null && lastToPlay.endTime > AudioSettings.dspTime)
            return lastToPlay.endTime;
        return AudioSettings.dspTime;
    }

    #endregion
}