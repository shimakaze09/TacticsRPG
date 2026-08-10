/// <summary>
/// Filter matching living units allied to the caster, including the caster
/// itself — the standard contract for heals, cleanses, and buffs. Routes
/// through Alliance.IsMatch so control statuses that invert allegiance
/// (Swayed) affect players, AI, and forecasts identically (issue #53).
/// </summary>
public class AllyAbilityEffectTarget : AbilityEffectTarget
{
    private Alliance alliance;

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        var stats = tile.content.GetComponent<Stats>();
        if (stats == null || stats[StatTypes.HP] <= 0)
            return false;

        // Resolved lazily: the ability prefab is parented to its unit after
        // instantiation, so the owner isn't findable until first use
        if (alliance == null)
            alliance = GetComponentInParent<Alliance>();
        if (alliance == null)
            return false;

        var other = tile.content.GetComponentInChildren<Alliance>();
        return other != null && alliance.IsMatch(other, Targets.Ally);
    }
}
