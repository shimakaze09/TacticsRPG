using UnityEngine;

/// <summary>
/// Enforces the battle-loadout projection (issue #17): whenever any ability
/// under this unit checks CanPerform, the gate vetoes it unless the
/// JobManager's projection — current job, grade gates, and (for player-owned
/// units) purchased abilities — includes it. One seam covers the player's
/// action menu and the AI alike, since both route through CanPerform.
/// </summary>
public class AbilityLoadoutGate : MonoBehaviour
{
    private readonly EventSubscriptions subscriptions = new EventSubscriptions();

    private Unit unit;
    private JobManager jobManager;

    // Subscribes globally: the check event's sender is the ability itself,
    // so the identity guard below filters to this unit's own abilities
    private void OnEnable()
    {
        subscriptions.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
    }

    // Symmetric cleanup per the event-bus contract
    private void OnDisable()
    {
        subscriptions.Clear();
    }

    // Vetoes abilities outside the unit's projected loadout
    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        if (e.Ability == null)
            return;

        unit = unit != null ? unit : GetComponent<Unit>();
        var owner = e.Ability.GetComponentInParent<Unit>();
        if (owner != unit)
            return;

        jobManager = jobManager != null ? jobManager : GetComponent<JobManager>();
        if (jobManager == null)
            return;

        if (!jobManager.IsAbilityUsable(e.Ability.name) && e.Exception.toggle)
            e.Exception.FlipToggle();
    }
}
