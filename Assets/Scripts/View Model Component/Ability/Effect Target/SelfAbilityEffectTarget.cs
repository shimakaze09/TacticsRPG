/// <summary>
/// Filter matching only the casting unit's own tile — the contract for
/// personal stances and self-buffs that must never spill onto anyone else
/// (issue #53).
/// </summary>
public class SelfAbilityEffectTarget : AbilityEffectTarget
{
    private Unit owner;

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        // Resolved lazily: the ability prefab is parented to its unit after
        // instantiation, so the owner isn't findable until first use
        if (owner == null)
            owner = GetComponentInParent<Unit>();

        return owner != null && tile.content == owner.gameObject;
    }
}
