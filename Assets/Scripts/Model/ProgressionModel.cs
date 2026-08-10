using UnityEngine;

/// <summary>
/// The combat-stat source of truth (issue #52). A unit's combat stats are
/// always recomputed from four inputs — job-grade history, character level,
/// worn gear, and difficulty — and never stored as authored numbers:
///
///   stat[i] = Σ over trained jobs: grade × round(base[i] × mult[i])   (job history)
///           + LevelGrowthBonus(currentJob, i, LVL)                    (character level)
///           + gear bonuses, then difficulty scaling and StatLimits caps.
///
/// The level term makes SpawnEntry / writ levels materially change enemy
/// stats without fabricating job history: each character level above 1 is
/// worth <see cref="LevelGrowthPerLevel"/> of a job grade, following the
/// CURRENT job's growth profile. Recomputing from the current job keeps the
/// model deterministic across forecast, AI, save/load, and rewards — the same
/// (level, history, job, gear, difficulty) tuple always yields the same
/// stats. Tuning: at 0.25 grade/level, a level-99 unit with a few mastered
/// jobs lands in WORLD.md §4b's designed 3,000–5,000 HP boss band.
/// Party units and generated enemies use the same model; only their inputs
/// differ (heroes earn levels and history, spawns are handed a level).
/// Known contract: because the level term follows the CURRENT job, all prior
/// level growth re-profiles retroactively on job switch — whether growth
/// should instead be banked per level-up is decided with the cumulative
/// growth redesign (issue #54).
/// </summary>
public static class ProgressionModel
{
    /// <summary>How much of one job grade each character level above 1 contributes.</summary>
    public const float LevelGrowthPerLevel = 0.25f;

    /// <summary>
    /// Stat gained from character level alone, for one stat slot in
    /// JobManager.statOrder layout. Level 1 contributes nothing — the
    /// job-history term already covers the starting kit.
    /// </summary>
    public static int LevelGrowthBonus(JobDefinition job, int statIndex, int characterLevel)
    {
        if (job == null || statIndex < 0 || statIndex >= job.baseStats.Length)
            return 0;

        int levelsAboveOne = Mathf.Max(0, characterLevel - 1);
        return Mathf.RoundToInt(
            levelsAboveOne * LevelGrowthPerLevel * job.baseStats[statIndex] * job.GetStatMultiplier(statIndex));
    }
}
