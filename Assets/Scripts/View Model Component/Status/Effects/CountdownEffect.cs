using UnityEngine;

public class CountdownEffect : MonoBehaviour
{
    public int remainingTurns = 3;
    public bool onExpireKill = false;

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
        remainingTurns = Mathf.Max(remainingTurns - 1, 0);
        if (remainingTurns <= 0)
        {
            if (onExpireKill && owner != null)
            {
                var s = owner.GetComponentInChildren<Stats>();
                if (s != null)
                    s.SetValue(StatTypes.HP, 0, false);
            }

            var cond = GetComponentInChildren<StatusCondition>() ?? GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }
}