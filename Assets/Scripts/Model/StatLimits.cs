/// <summary>
/// Global numeric ceilings for the whole game. Design intent (see WORLD.md §4b):
/// numbers stay small and readable — a late-game boss sits around 3,000–5,000 HP,
/// the engine hard-caps at 20,000, and no single hit ever exceeds 999. Every
/// growth source (job levels, equipment, buffs) must respect these; content
/// tuning happens under the caps, never by raising them casually.
/// </summary>
public static class StatLimits
{
    /// <summary>Hard ceiling for MHP. Bosses ~3k–5k by design; 20k is the wall.</summary>
    public const int MaxHP = 20000;

    /// <summary>Hard ceiling for MMP.</summary>
    public const int MaxMP = 9999;

    /// <summary>Ceiling for ATK/DEF/MAT/MDF/SPD.</summary>
    public const int MaxPrimaryStat = 999;

    /// <summary>No single hit or heal may exceed this.</summary>
    public const int MaxDamagePerHit = 999;

    /// <summary>Symmetric floor used by the effect pipeline (damage is negative).</summary>
    public const int MinDamagePerHit = -999;
}
