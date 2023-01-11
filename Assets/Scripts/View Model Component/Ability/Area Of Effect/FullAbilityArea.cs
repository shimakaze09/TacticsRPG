using System.Collections.Generic;

public class FullAbilityArea : AbilityArea
{
    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        var ar = GetComponent<AbilityRange>();
        return ar.GetTilesInRange(board);
    }
}