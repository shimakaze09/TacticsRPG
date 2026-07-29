using System.Collections.Generic;

/// <summary>
/// Whole-board range (Wakener engine strikes) — aim anywhere regardless of
/// position.
/// </summary>
public class InfiniteAbilityRange : AbilityRange
{
    public override bool positionOriented => false;

    public override List<Tile> GetTilesInRange(Board board)
    {
        return new List<Tile>(board.tiles.Values);
    }
}