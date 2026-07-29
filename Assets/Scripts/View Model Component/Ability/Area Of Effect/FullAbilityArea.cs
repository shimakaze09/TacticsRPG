using System.Collections.Generic;

/// <summary>
/// Area covering the entire board (broadcast abilities).
/// </summary>
public class FullAbilityArea : AbilityArea
{
    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        var ar = GetComponent<AbilityRange>();
        return ar.GetTilesInRange(board);
    }
}