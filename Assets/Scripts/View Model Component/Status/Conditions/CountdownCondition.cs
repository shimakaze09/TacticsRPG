using UnityEngine;

/// <summary>
/// Condition that tracks a countdown and triggers an action when it reaches zero.
/// Used by KO (death counter) and Doom statuses.
/// </summary>
public class CountdownCondition : StatusCondition
{
    public int counter = 3;
    public System.Action<CountdownCondition> onCountdownComplete;

    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner != null)
            this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, owner);
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, owner);
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
        counter--;

        if (counter <= 0)
        {
            onCountdownComplete?.Invoke(this);
            Remove();
        }
    }

    public int GetCounter()
    {
        return counter;
    }
}
