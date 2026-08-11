using UnityEngine;

/// <summary>
/// The confirm-or-revert timer behind risky display changes (issue #62): a
/// resolution apply arms it, an explicit confirm keeps the new mode, and if
/// the deadline passes unconfirmed the caller reverts — so a resolution the
/// monitor cannot show never strands the player. Pure time arithmetic (the
/// caller supplies "now", normally unscaled time) so it stays probe-testable
/// and immune to Time.timeScale.
/// </summary>
public class RevertCountdown
{
    /// <summary>Seconds the player gets to confirm before the revert.</summary>
    public const float DefaultSeconds = 10f;

    /// <summary>True while an unconfirmed change is awaiting its verdict.</summary>
    public bool Armed { get; private set; }

    private float deadline;

    /// <summary>Starts the countdown at "now"; a second Arm restarts it.</summary>
    public void Arm(float now, float seconds = DefaultSeconds)
    {
        Armed = true;
        deadline = now + Mathf.Max(0f, seconds);
    }

    /// <summary>The player accepted the change — no revert will happen.</summary>
    public void Confirm()
    {
        Armed = false;
    }

    /// <summary>Cancels without judgement (e.g. the caller reverted manually).</summary>
    public void Disarm()
    {
        Armed = false;
    }

    /// <summary>Whole seconds left to confirm (0 when disarmed or expired).</summary>
    public int RemainingSeconds(float now)
    {
        return Armed ? Mathf.Max(0, Mathf.CeilToInt(deadline - now)) : 0;
    }

    /// <summary>True exactly when the caller must revert: armed and past deadline.</summary>
    public bool HasExpired(float now)
    {
        return Armed && now >= deadline;
    }
}
