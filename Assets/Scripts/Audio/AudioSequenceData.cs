using UnityEngine;

/// <summary>
/// Bookkeeping for one entry in an AudioSequence: the source playing a clip and
/// the scheduled start/end times.
/// </summary>
public class AudioSequenceData
{
    #region Constructor

    public AudioSequenceData(AudioSource source)
    {
        this.source = source;
        startTime = -1;
    }

    #endregion

    #region Fields & Properties

    public double startTime { get; private set; }
    public readonly AudioSource source;

    public bool isScheduled => startTime > 0;

    public double endTime => startTime + source.clip.length;

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