using UnityEngine;

/// <summary>
/// The combat-stat source of truth (issues #52/#54, model version 2). A
/// unit's combat stats are always recomputed from four inputs — job history,
/// character level, worn gear, and difficulty — and never stored:
///
///   stat[i] = CurrentJobContribution(currentJob, i, grade)   (the trade you wear)
///           + Σ other unlocked jobs: CrossJobContribution    (bounded carryover)
///           + LevelGrowthBonus(currentJob, i, LVL)           (the person's experience)
///           + gear bonuses, then difficulty scaling and StatLimits caps.
///
/// Design decisions locked by #54:
/// - Grade 1 is zero trained growth: a job's kit (base × multiplier) comes
///   from wearing the certification, training starts at Grade 2. Unlocking a
///   job you never train grants nothing.
/// - The current job dominates: its kit plus TrainedGradeStep per earned
///   grade, and the character-level term follows its growth profile. Bands
///   (tank/striker/caster/support) therefore stay distinct all campaign —
///   switching jobs re-profiles the level term (the certification calibrates
///   the operator), a deliberate current-job modifier, not banked history.
/// - Multiclass carryover is bounded: every other unlocked job contributes at
///   most CrossJobCarryover of its kit, scaled by training progress toward
///   mastery. A completionist gains a broad, small spread — never the stacked
///   full blocks that drove every build toward the 999 caps.
/// - Saves need no data migration: only JP/grades are stored, and stats are
///   recomputed deterministically from them on every load under this model.
/// </summary>
public static class ProgressionModel
{
    /// <summary>Model version — bump when the formula changes shape.</summary>
    public const int Version = 2;

    /// <summary>How much of one job grade each character level above 1 contributes.</summary>
    public const float LevelGrowthPerLevel = 0.25f;

    /// <summary>Extra kit-fraction the current job gains per grade earned beyond 1.</summary>
    public const float TrainedGradeStep = 0.5f;

    /// <summary>Kit-fraction a fully mastered non-current job carries over.</summary>
    public const float CrossJobCarryover = 0.25f;

    /// <summary>Highest job grade (mastery).</summary>
    public const int MaxGrade = 8;

    /// <summary>
    /// Chapter-1 writ band (issue #52): generated writ battles draw and
    /// clamp their spawn levels here so repeatable contracts stay inside the
    /// slice's tuned difficulty. Later chapters widen the band deliberately.
    /// </summary>
    public const int WritLevelMin = 8;

    /// <summary>Upper bound of the chapter-1 writ band.</summary>
    public const int WritLevelMax = 12;

    /// <summary>
    /// The current job's whole contribution for one statOrder slot: its kit
    /// once, plus TrainedGradeStep of the kit per grade earned beyond 1.
    /// </summary>
    public static int CurrentJobContribution(JobDefinition job, int statIndex, int grade)
    {
        float kit = Kit(job, statIndex);
        if (kit <= 0f)
            return 0;

        int earned = Mathf.Clamp(grade, 1, MaxGrade) - 1;
        return Mathf.RoundToInt(kit * (1f + earned * TrainedGradeStep));
    }

    /// <summary>
    /// Permanent carryover from a non-current unlocked job: a fraction of its
    /// kit, growing linearly with training progress from nothing at Grade 1
    /// to CrossJobCarryover at mastery. Bounded so multiclassing broadens a
    /// build without saturating the global caps (issue #54).
    /// </summary>
    public static int CrossJobContribution(JobDefinition job, int statIndex, int grade)
    {
        float kit = Kit(job, statIndex);
        if (kit <= 0f)
            return 0;

        int earned = Mathf.Clamp(grade, 1, MaxGrade) - 1;
        return Mathf.RoundToInt(kit * CrossJobCarryover * earned / (MaxGrade - 1));
    }

    /// <summary>Everyone's status resistance before level or job factors.</summary>
    public const int ResistanceBase = 15;

    /// <summary>RES gained per character level above 1.</summary>
    public const float ResistancePerLevel = 0.5f;

    /// <summary>RES gained per point of the current job's MDF kit — protocol-hardened trades resist better.</summary>
    public const float ResistanceFromMdfKit = 0.5f;

    /// <summary>
    /// Status resistance derived from level and the current job's magic-
    /// defense profile (issue #57): RES was previously never initialized, so
    /// every unit sat at 0 and control accuracy was effectively unopposed.
    /// Recomputed like every other combat stat; capped by StatLimits.MaxRES
    /// so max-accuracy control always retains a real chance.
    /// </summary>
    public static int ResistanceFor(JobDefinition currentJob, int characterLevel)
    {
        // MDF sits at statOrder index 5 (MHP, MMP, ATK, DEF, MAT, MDF, SPD)
        float mdfKit = currentJob != null ? Kit(currentJob, 5) : 0f;
        int levelsAboveOne = Mathf.Max(0, characterLevel - 1);
        int res = ResistanceBase +
                  Mathf.RoundToInt(levelsAboveOne * ResistancePerLevel + mdfKit * ResistanceFromMdfKit);
        return Mathf.Clamp(res, 0, StatLimits.MaxRES);
    }

    /// <summary>
    /// Stat gained from character level alone, following the current job's
    /// growth profile. Level 1 contributes nothing — the kit already covers
    /// the starting band.
    /// </summary>
    public static int LevelGrowthBonus(JobDefinition job, int statIndex, int characterLevel)
    {
        float kit = Kit(job, statIndex);
        if (kit <= 0f)
            return 0;

        int levelsAboveOne = Mathf.Max(0, characterLevel - 1);
        return Mathf.RoundToInt(levelsAboveOne * LevelGrowthPerLevel * kit);
    }

    // One job grade's worth of a stat: base value times growth multiplier
    private static float Kit(JobDefinition job, int statIndex)
    {
        if (job == null || statIndex < 0 || statIndex >= job.baseStats.Length)
            return 0f;

        return job.baseStats[statIndex] * job.GetStatMultiplier(statIndex);
    }
}
