using System.Collections.Generic;

/// <summary>
/// Victory when a living hero ends an activation standing on one of the
/// authored zone tiles (BattleDefinition.zone). Defeat rules from the base
/// still apply.
/// </summary>
public class ReachZoneVictoryCondition : BaseVictoryCondition
{
    public List<Point> zone = new List<Point>();

    // Positions settle when an activation ends, so that's when we check
    protected override void OnEnable()
    {
        base.OnEnable();
        this.Subscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.Unsubscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    private void OnTurnCompleted(TurnCompletedEvent e)
    {
        if (Victor != Alliances.None)
            return;

        foreach (var unit in bc.units)
        {
            if (unit == null || unit.tile == null || IsDefeated(unit))
                continue;

            var alliance = unit.GetComponent<Alliance>();
            if (alliance == null || alliance.type != Alliances.Hero)
                continue;

            if (zone.Contains(unit.tile.pos))
            {
                Victor = Alliances.Hero;
                return;
            }
        }
    }
}
