public class UndeadAbilityEffectTarget : AbilityEffectTarget
{
    /// <summary>
    /// Indicates whether the Undead component must be present (true)
    /// or must not be present (false) for the target to be valid.
    /// </summary>
    public bool toggle;

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null) return false;

        var hasComponent = tile.content.GetComponent<Undead>() != null;
        if (hasComponent != toggle)
            return false;

        var s = tile.content.GetComponent<Stats>();
        return s != null && s[StatTypes.HP] > 0;
    }
}