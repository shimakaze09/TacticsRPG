using System;
using System.Collections.Generic;

/// <summary>
/// The control-status contract (issue #57): which statuses count as hard
/// control (denying or seizing a unit's actions), and the bounds that keep
/// control powerful but contestable — landing chance is never certain nor
/// impossible, inflicted durations are capped, and every successful control
/// application hardens the target against the next one (SteeledStatus).
/// Data-driven inflicts route through StatusRegistry.Inflict, which enforces
/// this table; engine-internal Status.Add calls bypass it deliberately
/// (scripted story beats may exceed the budget).
/// </summary>
public static class ControlBudget
{
    /// <summary>Floor for a status-type hit chance — control may always miss the roll but never becomes impossible.</summary>
    public const int MinChance = 5;

    /// <summary>Ceiling for a status-type hit chance — control never becomes a certainty.</summary>
    public const int MaxChance = 95;

    /// <summary>Longest duration (in the target's turns) a data-driven control inflict may set.</summary>
    public const int MaxControlDuration = 3;

    /// <summary>Effective RES each Steeled stack adds against further status attempts.</summary>
    public const int SteeledResistancePerStack = 20;

    /// <summary>How many of the target's turns each Steeled application lasts.</summary>
    public const int SteeledDuration = 3;

    // Hard-control statuses: action denial (sleep/stop/disable/immobilize/
    // delayed KO/stasis) and seizure (charm/confusion/berserk). Soft tempo or
    // resource ailments (Throttle, DeadAir, Desync) stay outside the budget.
    private static readonly HashSet<string> controlStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Blackout",
        "FreezeFrame",
        "Graycast",
        "Jammed",
        "Pinned",
        "Deadline",
        "Scrambled",
        "Swayed",
        "Redline"
    };

    /// <summary>Whether a status data-name falls under the hard-control budget.</summary>
    public static bool IsControl(string statusName)
    {
        if (string.IsNullOrEmpty(statusName))
            return false;

        // Accept both bare names ("Swayed") and class names ("SwayedStatus")
        var name = statusName.EndsWith("Status", StringComparison.OrdinalIgnoreCase)
            ? statusName.Substring(0, statusName.Length - 6)
            : statusName;
        return controlStatuses.Contains(name);
    }
}
