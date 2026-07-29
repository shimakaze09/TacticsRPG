using System.Collections.Generic;

/// <summary>
/// Caster's own tile only (self-buffs and self-centered areas).
/// </summary>
public class SelfAbilityRange : AbilityRange
{
    public override bool positionOriented => false;

    public override List<Tile> GetTilesInRange(Board board)
    {
        var retValue = new List<Tile>(1) { unit.tile };
        return retValue;
    }
}