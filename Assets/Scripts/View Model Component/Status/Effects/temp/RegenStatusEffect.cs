using UnityEngine;


public class RegenStatusEffect : StatusEffect
{
    private Stats stats;

    // healPercent is percentage of max HP to heal each turn, in decimals (e.g. 0.05 for 5%)
    public float healPercent = 0.05f;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats)
            this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, GetComponentInParent<Unit>());
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, GetComponentInParent<Unit>());
    }


    private void OnNewTurn(object sender, object args)
    {
        var s = GetComponentInParent<Stats>();
        var maxHP = s[StatTypes.MHP];
        var heal = Mathf.Max(1, Mathf.FloorToInt(maxHP * healPercent));
        var currentHP = s[StatTypes.HP];
        s.SetValue(StatTypes.HP, Mathf.Min(currentHP + heal, maxHP), false);
    }
}