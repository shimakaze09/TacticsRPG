using System;

/// <summary>
/// Guard for hypothetical unit placement during AI planning. Range, hit-rate,
/// and Predict() all read the caster's live tile and facing, so evaluating a
/// move candidate requires actually placing the unit there — this scope makes
/// that safe: it captures the unit's real tile and facing on creation and
/// restores both (including tile occupancy, via Unit.Place) on Dispose, so a
/// planning exception can never leave the board in a hypothetical state.
/// Use inside a using-statement; call Restore between independent evaluations.
/// </summary>
public sealed class AiPlacementScope : IDisposable
{
    private readonly Unit unit;
    private readonly Tile startTile;
    private readonly Directions startDir;

    /// <summary>Captures the unit's real position and facing before any hypothetical moves.</summary>
    public AiPlacementScope(Unit unit)
    {
        this.unit = unit;
        startTile = unit.tile;
        startDir = unit.dir;
    }

    /// <summary>Hypothetically stands the unit on a tile for evaluation.</summary>
    public void MoveTo(Tile tile)
    {
        unit.Place(tile);
    }

    /// <summary>Hypothetically turns the unit for directional evaluation.</summary>
    public void Face(Directions direction)
    {
        unit.dir = direction;
    }

    /// <summary>Puts the unit back on its real tile with its real facing.</summary>
    public void Restore()
    {
        if (unit.tile != startTile)
            unit.Place(startTile);
        unit.dir = startDir;
    }

    /// <summary>Guaranteed restoration — runs even when evaluation throws.</summary>
    public void Dispose()
    {
        Restore();
    }
}
