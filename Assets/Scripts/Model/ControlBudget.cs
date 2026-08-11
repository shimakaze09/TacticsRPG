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

        return controlStatuses.Contains(Normalize(statusName));
    }

    /// <summary>
    /// One status's control contract (issue #57): how many of the victim's
    /// actions each afflicted turn denies (seizure counts extra — the stolen
    /// turn also works against its own side), the duration and data-accuracy
    /// ceilings the status may reach, and whether boss-tier targets (unique
    /// jobs) shrug it off entirely.
    /// </summary>
    public readonly struct ControlProfile
    {
        /// <summary>Expected actions denied per afflicted turn (seizure > 1).</summary>
        public readonly float LostActionsPerTurn;

        /// <summary>Duration ceiling for this status (within MaxControlDuration).</summary>
        public readonly int MaxDuration;

        /// <summary>Data-accuracy ceiling — stronger denial gets a lower ceiling.</summary>
        public readonly int MaxAccuracy;

        /// <summary>Boss-tier targets ignore this status entirely.</summary>
        public readonly bool BossImmune;

        /// <summary>Builds one immutable contract row.</summary>
        public ControlProfile(float lostActionsPerTurn, int maxDuration, int maxAccuracy, bool bossImmune)
        {
            LostActionsPerTurn = lostActionsPerTurn;
            MaxDuration = maxDuration;
            MaxAccuracy = maxAccuracy;
            BossImmune = bossImmune;
        }
    }

    // The per-status contract table. Values follow the mechanics: full CT/
    // action denial is strongest (short + low accuracy ceiling), partial
    // denial is softer, and seizure (the victim's turn is spent against its
    // own side) counts more than one lost action and never touches bosses —
    // a seized boss trivializes its encounter.
    private static readonly Dictionary<string, ControlProfile> profiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blackout"] = new ControlProfile(1.0f, 3, 75, false),
        ["Graycast"] = new ControlProfile(1.0f, 3, 75, false),
        ["FreezeFrame"] = new ControlProfile(1.0f, 2, 65, false),
        ["Jammed"] = new ControlProfile(0.6f, 3, 85, false),
        ["Pinned"] = new ControlProfile(0.4f, 3, 85, false),
        ["Scrambled"] = new ControlProfile(1.2f, 2, 70, true),
        ["Redline"] = new ControlProfile(1.2f, 2, 70, true),
        ["Swayed"] = new ControlProfile(1.5f, 2, 60, true),
        ["Deadline"] = new ControlProfile(2.0f, 3, 60, true)
    };

    /// <summary>
    /// Looks up a status's control contract; false for non-control statuses.
    /// </summary>
    public static bool TryGetProfile(string statusName, out ControlProfile profile)
    {
        profile = default;
        return !string.IsNullOrEmpty(statusName) && profiles.TryGetValue(Normalize(statusName), out profile);
    }

    /// <summary>
    /// Boss policy scope: units wearing a unique job are boss-tier — their
    /// encounters are authored around them acting (issue #57).
    /// </summary>
    public static bool IsBossTier(Unit unit)
    {
        if (unit == null)
            return false;

        var jm = unit.GetComponent<JobManager>();
        return jm != null && jm.CurrentJob != null && jm.CurrentJob.isUnique;
    }

    // Accept both bare names ("Swayed") and class names ("SwayedStatus")
    private static string Normalize(string statusName)
    {
        return statusName.EndsWith("Status", StringComparison.OrdinalIgnoreCase)
            ? statusName.Substring(0, statusName.Length - 6)
            : statusName;
    }
}
