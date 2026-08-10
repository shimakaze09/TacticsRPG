using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Core animation timer: ticks a 0..1 value through an easing equation with
/// play/pause/reverse/loop controls and events. Tweeners build on this.
/// Lifecycle contract (issue #23): disabling mid-play pauses and re-enabling
/// resumes the in-flight state; Play/Reverse requested while disabled start on
/// the next enable; on completion the control is Stopped before completedEvent
/// fires, so handlers (including destroy-on-complete) see final state.
/// </summary>
public class EasingControl : MonoBehaviour
{
    #region Events

    public event EventHandler updateEvent;
    public event EventHandler stateChangeEvent;
    public event EventHandler completedEvent;
    public event EventHandler loopedEvent;

    #endregion

    #region Enums

    public enum TimeType
    {
        Normal,
        Real,
        Fixed
    }

    public enum PlayState
    {
        Stopped,
        Paused,
        Playing,
        Reversing
    }

    public enum EndBehaviour
    {
        Constant,
        Reset
    }

    public enum LoopType
    {
        Repeat,
        PingPong
    }

    #endregion

    #region Properties

    public TimeType timeType = TimeType.Normal;
    public PlayState playState { get; private set; }
    public PlayState previousPlayState { get; private set; }
    public EndBehaviour endBehaviour = EndBehaviour.Constant;
    public LoopType loopType = LoopType.Repeat;
    public bool IsPlaying => playState is PlayState.Playing or PlayState.Reversing;

    public float startValue = 0.0f;
    public float endValue = 1.0f;
    public float duration = 1.0f;
    public int loopCount = 0;
    public Func<float, float, float, float> equation = EasingEquations.Linear;

    public float currentTime { get; private set; }
    public float currentValue { get; private set; }
    public float currentOffset { get; private set; }
    public int loops { get; private set; }

    private Coroutine tickerRoutine;

    #endregion

    #region MonoBehaviour

    // Re-enabling resumes whatever state the disable interrupted
    private void OnEnable()
    {
        Resume();
    }

    // Disabling pauses; SetPlayState records the in-flight state for resume
    private void OnDisable()
    {
        Pause();
    }

    #endregion

    #region Public

    public void Play()
    {
        SetPlayState(PlayState.Playing);
    }

    public void Reverse()
    {
        SetPlayState(PlayState.Reversing);
    }

    public void Pause()
    {
        if (IsPlaying)
            SetPlayState(PlayState.Paused);
    }

    public void Resume()
    {
        if (playState == PlayState.Paused)
            SetPlayState(previousPlayState);
    }

    public void Stop()
    {
        SetPlayState(PlayState.Stopped);
        previousPlayState = PlayState.Stopped;
        loops = 0;
        if (endBehaviour == EndBehaviour.Reset)
            SeekToBeginning();
    }

    public void SeekToTime(float time)
    {
        currentTime = Mathf.Clamp01(time / duration);
        var newValue = (endValue - startValue) * currentTime + startValue;
        currentOffset = newValue - currentValue;
        currentValue = newValue;
        OnUpdate();
    }

    public void SeekToBeginning()
    {
        SeekToTime(0.0f);
    }

    public void SeekToEnd()
    {
        SeekToTime(duration);
    }

    #endregion

    #region Protected

    protected virtual void OnUpdate()
    {
        updateEvent?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnLoop()
    {
        loopedEvent?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnComplete()
    {
        completedEvent?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnStateChange()
    {
        stateChangeEvent?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Private

    // Applies a state transition. While inactive no coroutine may run, so the
    // requested state is recorded for the next enable instead — critically,
    // pausing from OnDisable must keep the in-flight state as the resume
    // target rather than overwrite it (the old code lost it, so a disabled
    // then re-enabled control never resumed; issue #23)
    private void SetPlayState(PlayState target)
    {
        if (isActiveAndEnabled)
        {
            if (playState == target)
                return;

            previousPlayState = playState;
            playState = target;
            OnStateChange();
            if (tickerRoutine != null)
            {
                StopCoroutine(tickerRoutine);
                tickerRoutine = null;
            }

            if (IsPlaying)
                tickerRoutine = StartCoroutine(Ticker());
        }
        else
        {
            if (target == PlayState.Stopped)
            {
                previousPlayState = PlayState.Stopped;
                playState = PlayState.Stopped;
            }
            else if (target == PlayState.Paused)
            {
                if (IsPlaying)
                    previousPlayState = playState;
                playState = PlayState.Paused;
            }
            else // Play/Reverse while disabled — defer to the next enable
            {
                previousPlayState = target;
                playState = PlayState.Paused;
            }

            // Unity already killed the coroutine with the disable
            tickerRoutine = null;
        }
    }

    private IEnumerator Ticker()
    {
        while (true)
            switch (timeType)
            {
                case TimeType.Normal:
                    yield return new WaitForEndOfFrame();
                    Tick(Time.deltaTime);
                    break;
                case TimeType.Real:
                    yield return new WaitForEndOfFrame();
                    Tick(Time.unscaledDeltaTime);
                    break;
                default: // Fixed
                    yield return new WaitForFixedUpdate();
                    Tick(Time.fixedDeltaTime);
                    break;
            }
    }

    private void Tick(float time)
    {
        var finished = false;
        if (playState == PlayState.Playing)
        {
            currentTime = Mathf.Clamp01(currentTime + time / duration);
            finished = Mathf.Approximately(currentTime, 1.0f);
        }
        else // Reversing
        {
            currentTime = Mathf.Clamp01(currentTime - time / duration);
            finished = Mathf.Approximately(currentTime, 0.0f);
        }

        var frameValue = (endValue - startValue) * equation(0.0f, 1.0f, currentTime) + startValue;
        currentOffset = frameValue - currentValue;
        currentValue = frameValue;
        OnUpdate();

        if (finished)
        {
            ++loops;
            if (loopCount < 0 || loopCount >= loops)
            {
                if (loopType == LoopType.Repeat)
                    SeekToBeginning();
                else // PingPong
                    SetPlayState(playState == PlayState.Playing ? PlayState.Reversing : PlayState.Playing);

                OnLoop();
            }
            else
            {
                // Stop first so completion handlers observe final state; a
                // Tweener's destroy-on-complete then tears down a component
                // that is already fully stopped (issue #23)
                Stop();
                OnComplete();
            }
        }
    }

    #endregion
}