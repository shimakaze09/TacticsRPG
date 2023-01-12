using UnityEngine;
using System.Collections;

public class AudioSequenceData
{
    #region Fields & Properties

    public double startTime { get; private set; }
    public readonly AudioSource source;

    public bool isScheduled => startTime > 0;

    public double endTime => startTime + source.clip.length;

    #endregion

    #region Constructor

    public AudioSequenceData(AudioSource source)
    {
        this.source = source;
        startTime = -1;
    }

    #endregion

    #region Public

    public void Schedule(double time)
    {
        if (isScheduled)
            source.SetScheduledStartTime(time);
        else
            source.PlayScheduled(time);
        startTime = time;
    }

    public void Stop()
    {
        startTime = -1;
        source.Stop();
    }

    #endregion
}