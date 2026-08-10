/// <summary>
/// Filter matching knocked-out units allied to the caster — the contract for
/// revives, which must not raise enemy bodies (issue #53).
/// </summary>
public class KOdAllyAbilityEffectTarget : AbilityEffectTarget
{
    private Alliance alliance;

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        var stats = tile.content.GetComponent<Stats>();
        if (stats == null || stats[StatTypes.HP] > 0)
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
