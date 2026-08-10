using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The typed vocabulary of inflictable statuses: maps data names ("Static",
/// "Doused") to their classes and to strongly-typed inflict calls — no
/// runtime reflection, and an unknown name fails loudly at first use.
/// Adding a status class = adding one Register line here
/// (ARCHITECTURE.md "Adding a status").
/// </summary>
public static class StatusRegistry
{
    private class Entry
    {
        public Type type;
        public Func<Status, DurationStatusCondition> inflict;
    }

    private static readonly Dictionary<string, Entry> entries = new(StringComparer.OrdinalIgnoreCase);

    static StatusRegistry()
    {
        // Ailments
        Register<BlackoutStatus>();
        Register<DeadAirStatus>();
        Register<DeadlineStatus>();
        Register<DousedStatus>();
        Register<FreezeFrameStatus>();
        Register<GraycastStatus>();
        Register<JammedStatus>();
        Register<PinnedStatus>();
        Register<RevenantStatus>();
        Register<ScrambledStatus>();
        Register<ScrappedStatus>();
        Register<SepsisStatus>();
        Register<ShreddedStatus>();
        Register<StaticStatus>();
        Register<SwayedStatus>();
        Register<ThirstStatus>();
        Register<ThrottleStatus>();
        Register<YellowbellyStatus>();

        // Beneficial
        Register<BulwarkStatus>();
        Register<FailsafeStatus>();
        Register<FirewallStatus>();
        Register<GhostedStatus>();
        Register<KnitStatus>();
        Register<NullgravStatus>();
        Register<OverclockStatus>();
        Register<SteeledStatus>();

        // Other
        Register<DesyncStatus>();
        Register<RedlineStatus>();
        Register<ReflectStatus>();
        Register<SyncStatus>();
    }

    private static void Register<T>() where T : StatusEffect
    {
        var entry = new Entry
        {
            type = typeof(T),
            inflict = status => status.Add<T, DurationStatusCondition>()
        };

        var className = typeof(T).Name;
        entries[className] = entry;
        // Data uses bare names ("Static" for StaticStatus)
        if (className.EndsWith("Status"))
            entries[className.Substring(0, className.Length - 6)] = entry;
    }

    /// <summary>The status class for a data name, or null when unknown.</summary>
    public static Type Resolve(string name)
    {
        return !string.IsNullOrEmpty(name) && entries.TryGetValue(name, out var entry) ? entry.type : null;
    }

    /// <summary>
    /// Puts the named status on the target for a duration. Returns the
    /// condition, or null (with an error) for unknown names/targets.
    /// Hard-control statuses go through the ControlBudget contract: the
    /// duration is capped and the target gains a Steeled stack so repeat
    /// control hits diminishing returns (issue #57).
    /// </summary>
    public static DurationStatusCondition Inflict(Unit target, string name, int duration)
    {
        if (target == null)
            return null;

        if (string.IsNullOrEmpty(name) || !entries.TryGetValue(name, out var entry))
        {
            Debug.LogError($"[StatusRegistry] Unknown status '{name}'");
            return null;
        }

        var status = target.GetComponent<Status>();
        if (status == null)
            return null;

        bool isControl = ControlBudget.IsControl(name);
        var condition = entry.inflict(status);
        condition.duration = isControl
            ? Mathf.Min(duration, ControlBudget.MaxControlDuration)
            : duration;

        if (isControl)
            HardenAgainstControl(status);

        return condition;
    }

    // Adds or deepens the target's Steeled protection after a control status
    // lands: one effect object, one duration condition, a stack counter —
    // never a second Steeled instance per application.
    private static void HardenAgainstControl(Status status)
    {
        var steeled = status.GetComponentInChildren<SteeledStatus>();
        if (steeled == null)
        {
            var condition = status.Add<SteeledStatus, DurationStatusCondition>();
            condition.duration = ControlBudget.SteeledDuration;
            return;
        }

        steeled.stacks++;
        var existing = steeled.GetComponentInChildren<DurationStatusCondition>();
        if (existing != null)
            existing.duration = Mathf.Max(existing.duration, ControlBudget.SteeledDuration);
    }
}
