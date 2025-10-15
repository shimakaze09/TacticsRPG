using UnityEngine;

/// <summary>
/// Unit has leapt high upwards (Dragoon's Jump).
/// Cannot be targeted while in this condition.
/// </summary>
public class JumpingStatus : StatusEffect
{
    private Unit owner;
    [Tooltip("Number of turns before landing")]
    public int jumpDuration = 1;
    private int currentTurn = 0;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();

        if (owner != null)
        {
            this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, owner);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, owner);
        }
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
        currentTurn++;

        if (currentTurn >= jumpDuration)
        {
            // Land and execute jump attack
            var cond = GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }
}
