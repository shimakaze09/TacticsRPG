using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for area shapes: expands the chosen target tile into the full set of
/// affected tiles.
/// </summary>
public abstract class AbilityArea : MonoBehaviour
{
    public abstract List<Tile> GetTilesInArea(Board board, Point pos);
}