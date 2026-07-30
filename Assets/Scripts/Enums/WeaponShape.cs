/// <summary>
/// The footprint of a weapon's basic attack. Target strikes one tile;
/// Line sprays a straight ray to the weapon's reach (flamethrower-style);
/// Sweep swings through the target tile and both tiles beside it. Wide
/// footprints trade per-target damage for coverage via GearData's
/// damagePercent (the weapon behavior model, GDD §3.3).
/// </summary>
public enum WeaponShape
{
    Target,
    Line,
    Sweep
}
