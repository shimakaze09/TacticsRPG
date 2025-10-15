using UnityEngine;

public abstract class TurnBasedStatusEffect : StatusEffect
{
    [Tooltip("Number of turns remaining. Decrement is called by TurnOrderController or a similar system.")]
    public int remainingTurns = 3;

    private Unit ownerUnit;

    private void OnEnable()
    {
        ownerUnit = GetComponentInParent<Unit>();
        if (ownerUnit != null)
            this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, ownerUnit);
    }

    private void OnDisable()
    {
        if (ownerUnit != null)
            this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, ownerUnit);
    }

    private void OnTurnBegan(object sender, object args)
    {
        // args are whatever your TurnOrderController uses; we only need to decrement when it's this unit's turn.
        var unit = sender as Unit ?? (sender as Component)?.GetComponentInParent<Unit>();
        if (unit != ownerUnit)
            return;

        remainingTurns = Mathf.Max(remainingTurns - 1, 0);
        if (remainingTurns <= 0)
        {
            // Remove the condition component (assumes StatusCondition handles actual removal)
            var cond = GetComponentInChildren<StatusCondition>() ?? GetComponent<StatusCondition>();
            if (cond != null)
            {
                cond.Remove();
            }
            else
            {
                // fallback: destroy this component if no StatusCondition exists
                Destroy(this);
            }
        }
    }
}