/// <summary>
/// Filter matching units hostile to the caster.
/// </summary>
public class EnemyAbilityEffectTarget : AbilityEffectTarget
{
    private Alliance alliance;

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        // Resolved lazily: the ability prefab is parented to its unit after
        // instantiation, so the owner isn't findable until first use
        if (alliance == null)
            alliance = GetComponentInParent<Alliance>();
        if (alliance == null)
            return false;

        var other = tile.content.GetComponentInChildren<Alliance>();
        return alliance.IsMatch(other, Targets.Foe);
    }
}