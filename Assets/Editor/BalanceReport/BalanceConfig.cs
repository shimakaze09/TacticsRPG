#if UNITY_EDITOR

/// <summary>
/// The versioned balance contract for the automated report (issue #58): every
/// target band, tolerance, and reference point the generator measures content
/// against lives here as a named constant. Numbers come from WORLD.md §4b
/// (damage/heal conventions, level-1 tuning targets) and the #54 progression
/// decisions; changing any of them is a design decision, so bump Version in
/// the same edit — reports carry the version so two runs are only comparable
/// when it matches.
/// </summary>
public static class BalanceConfig
{
    /// <summary>Config version — bump whenever any band or tolerance changes.</summary>
    public const int Version = 1;

    /// <summary>
    /// Fixed seed recorded in every report. v1 uses no sampling at all, but
    /// the seed is part of the schema so any future simulation stays
    /// reproducible and diffs stay honest (issue #58 acceptance criterion).
    /// </summary>
    public const int RandomSeed = 20260810;

    #region Level-1 target bands (WORLD.md §4b, job level 1, no gear)

    /// <summary>Lower edge of the level-1 HP pool band per archetype.</summary>
    public const int Level1HpMin = 35;

    /// <summary>Upper edge of the level-1 HP pool band per archetype.</summary>
    public const int Level1HpMax = 110;

    /// <summary>Basic attacks should land at least this at level 1.</summary>
    public const int BasicDamageMin = 10;

    /// <summary>Basic attacks should land at most this at level 1.</summary>
    public const int BasicDamageMax = 15;

    /// <summary>Ordinary kit hits should land at least this at level 1.</summary>
    public const int KitDamageMin = 15;

    /// <summary>Ordinary kit hits should land at most this at level 1.</summary>
    public const int KitDamageMax = 25;

    /// <summary>Capstone nukes should land at least this at level 1.</summary>
    public const int CapstoneDamageMin = 30;

    /// <summary>Capstone nukes should land at most this at level 1.</summary>
    public const int CapstoneDamageMax = 45;

    /// <summary>Heals below this restored-HP value are review-worthy dribbles.</summary>
    public const int HealPowerMin = 10;

    /// <summary>Heals above this restored-HP value outpace the intended pools.</summary>
    public const int HealPowerMax = 60;

    #endregion

    #region Matchup bands

    /// <summary>A level-matched basic-attack KO should take at least this many hits.</summary>
    public const int TurnsToKoMin = 2;

    /// <summary>A level-matched basic-attack KO should take at most this many hits.</summary>
    public const int TurnsToKoMax = 6;

    /// <summary>
    /// Basic-attack power used for the damage/TTK matrix: 100 = one ATK's
    /// worth (WORLD §4b). The shipped common.attack sits at 150 and weapon
    /// damagePercent scales on top; the matrix deliberately measures the raw
    /// stat curves, not gear.
    /// </summary>
    public const int BasicAttackPower = 100;

    #endregion

    #region Hard invariants (CI failures)

    /// <summary>
    /// No primary stat (ATK/DEF/MAT/MDF/SPD) may reach the 999 cap on a
    /// single-job grade-8 path before this character level — saturation
    /// earlier than the intended endgame flattens all tuning above it.
    /// </summary>
    public const int CapSaturationMinLevel = 90;

    /// <summary>
    /// Expected effect values beyond band-top × this multiplier are absurd
    /// outliers (data typos), not tuning drift — they fail the run.
    /// </summary>
    public const int OutlierMultiplier = 10;

    #endregion

    #region Warning tolerances (review prompts, never CI failures)

    /// <summary>
    /// A zero-MP damage ability above this power is free power that undercuts
    /// the whole MP economy (the #55 sustain problem) — flagged for review.
    /// </summary>
    public const int FreeDamagePowerWarnThreshold = 200;

    /// <summary>
    /// Encounter sides whose average unit levels differ by more than this are
    /// flagged — a lopsided level budget usually means a data entry slip.
    /// </summary>
    public const int EncounterLevelDeltaTolerance = 2;

    #endregion

    #region Reference points (where measurements are taken)

    /// <summary>Character levels sampled by the job stat tables.</summary>
    public static readonly int[] StatTableLevels = { 1, 10, 30, 60, 99 };

    /// <summary>Character levels sampled by the damage/TTK matrix.</summary>
    public static readonly int[] MatrixLevels = { 1, 30, 99 };

    /// <summary>Character level at which ability efficiency is evaluated (mid Chapter 1).</summary>
    public const int AbilityAuditLevel = 10;

    /// <summary>Job grade at which ability efficiency is evaluated (mid Chapter 1).</summary>
    public const int AbilityAuditGrade = 3;

    /// <summary>
    /// How many mastered cross-jobs the "grade 8 + cross-training" stat path
    /// assumes (the first N other Basic/Common jobs in id order, so the pick
    /// is deterministic).
    /// </summary>
    public const int CrossJobSampleCount = 3;

    #endregion
}

#endif
