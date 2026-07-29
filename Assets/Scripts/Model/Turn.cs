using System.Collections.Generic;

/// <summary>
/// Mutable state of the current activation: acting unit, chosen
/// ability/targets, movement/undo bookkeeping, and action flags.
/// </summary>
public class Turn
{
    public Ability ability;
    public Unit actor;
    public bool hasUnitActed;
    public bool hasUnitMoved;
    public bool lockMove;
    public PlanOfAttack plan;
    private Directions startDir;
    private Tile startTile;
    public List<Tile> targets;

    public void Change(Unit current)
    {
        actor = current;
        hasUnitMoved = false;
        hasUnitActed = false;
        lockMove = false;
        startTile = actor.tile;
        startDir = actor.dir;
        plan = null;
    }

    public void UndoMove()
    {
        hasUnitMoved = false;
        actor.Place(startTile);
        actor.dir = startDir;
        actor.Match();
    }
}