using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityArea : MonoBehaviour
{
    public abstract List<Tile> GetTilesInArea(Board board, Point pos);
}