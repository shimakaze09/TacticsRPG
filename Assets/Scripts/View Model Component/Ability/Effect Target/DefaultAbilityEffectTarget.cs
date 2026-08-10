/// <summary>
/// Filter matching any living unit regardless of allegiance — the explicit
/// "AnyLiving" contract in ability data. Deliberate opt-in only (e.g. an
/// effect designed to hit friend and foe alike); heals and buffs use the
/// Ally/Self filters instead (issue #53).
/// </summary>
public class DefaultAbilityEffectTarget : AbilityEffectTarget
{
    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        var s = tile.content.GetComponent<Stats>();
        return s != null && s[StatTypes.HP] > 0;
    }
}