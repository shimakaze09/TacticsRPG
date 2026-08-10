using UnityEngine;

/// <summary>
/// Battle-wide expiry law for CT-frozen units (issue #57): a unit whose CT is
/// frozen (ICtFreezingStatus) never begins a turn, so its duration conditions
/// would never tick — the #12 failure. On every completed activation this
/// rules component advances the frozen-window fallback for each battle unit's
/// conditions. Living centrally — one component on the BattleController, per
/// the battle-wide-law rule — lets it snapshot each unit's frozen state
/// BEFORE ticking any of its conditions, so a control status expiring
/// mid-event still counts that denied turn for every sibling condition
/// (Steeled included) instead of racing per-condition subscription order.
/// </summary>
public class StatusExpiryRules : MonoBehaviour
{
    private BattleController controller;
    private BattleClock clock;

    private void OnEnable()
    {
        this.Subscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    // One frozen snapshot per unit per activation, then every condition of
    // that unit advances against the same truth. Siblings resolved lazily —
    // this component may be added before the clock exists on the controller.
    private void OnTurnCompleted(TurnCompletedEvent e)
    {
        if (controller == null)
            controller = GetComponent<BattleController>();
        if (clock == null)
            clock = GetComponent<BattleClock>();
        if (controller == null || clock == null)
            return;

        foreach (var unit in controller.units)
        {
            if (unit == null)
                continue;

            bool frozen = unit.GetComponentInChildren<ICtFreezingStatus>() != null;
            foreach (var condition in unit.GetComponentsInChildren<DurationStatusCondition>())
                condition.FallbackActivation(frozen, clock.RoundLength);
        }
    }
}
