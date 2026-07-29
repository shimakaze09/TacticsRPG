using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for targeting ranges: which tiles can be aimed at from the caster's
/// position (position/direction-oriented flags drive AI planning).
/// </summary>
public abstract class AbilityRange : MonoBehaviour
{
    public int horizontal = 1;
    public int vertical = int.MaxValue;
    public virtual bool positionOriented => true;
    public virtual bool directionOriented => false;
    protected Unit unit => GetComponentInParent<Unit>();

    public abstract List<Tile> GetTilesInRange(Board board);
}