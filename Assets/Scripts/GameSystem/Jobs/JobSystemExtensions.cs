using UnityEngine;

/// <summary>
/// Extension methods and helper utilities for the Job System
/// </summary>
public static class JobSystemExtensions
{
    /// <summary>
    /// Gets or adds JobManager component to a GameObject
    /// </summary>
    public static JobManager GetOrAddJobManager(this GameObject go)
    {
        var jobManager = go.GetComponent<JobManager>();
        if (jobManager == null)
        {
            jobManager = go.AddComponent<JobManager>();
        }
        return jobManager;
    }

    /// <summary>
    /// Safely gets JobManager from a Component
    /// </summary>
    public static JobManager GetJobManager(this Component component)
    {
        return component.GetComponent<JobManager>();
    }

    /// <summary>
    /// Checks if a unit has a specific job unlocked
    /// </summary>
    public static bool HasJobUnlocked(this GameObject unit, string jobName)
    {
        var jobManager = unit.GetComponent<JobManager>();
        if (jobManager == null)
            return false;

        var job = jobManager.FindJobByName(jobName);
        return job != null && jobManager.ProgressData.IsJobUnlocked(job);
    }

    /// <summary>
    /// Gets current job name for a unit
    /// </summary>
    public static string GetCurrentJobName(this GameObject unit)
    {
        var jobManager = unit.GetComponent<JobManager>();
        return jobManager?.CurrentJob?.jobName ?? "None";
    }

    /// <summary>
    /// Checks if unit can learn a specific ability
    /// </summary>
    public static bool CanLearnAbility(this GameObject unit, string abilityName)
    {
        var jobManager = unit.GetComponent<JobManager>();
        if (jobManager == null)
            return false;

        return jobManager.AbilityMemory.HasLearnedAbility(abilityName);
    }
}

/// <summary>
/// Helper class for common job system operations
/// </summary>
public static class JobSystemHelper
{
    /// <summary>
    /// Standard JP awards based on battle outcomes (can be customized)
    /// </summary>
    public static class StandardJPRewards
    {
        public const int DefeatEnemy = 10;
        public const int CriticalHit = 5;
        public const int CompleteObjective = 50;
        public const int WinBattle = 100;
        public const int PerfectVictory = 150;
    }

    /// <summary>
    /// Calculates JP reward based on enemy level difference
    /// FFT-style: harder enemies give more JP
    /// </summary>
    public static int CalculateJPReward(int playerLevel, int enemyLevel, int baseJP = 10)
    {
        float levelDifference = enemyLevel - playerLevel;
        float multiplier = 1.0f + (levelDifference * 0.1f); // +10% per level difference
        multiplier = Mathf.Clamp(multiplier, 0.5f, 2.0f); // Cap between 50% and 200%
        
        return Mathf.RoundToInt(baseJP * multiplier);
    }

    /// <summary>
    /// Gets localized job category name
    /// </summary>
    public static string GetCategoryName(JobCategory category)
    {
        return category switch
        {
            JobCategory.Basic => "Basic",
            JobCategory.Common => "Common",
            JobCategory.Special => "Special",
            JobCategory.Unique => "Unique",
            JobCategory.Monster => "Monster",
            JobCategory.Guest => "Guest",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets color for job category (for UI display)
    /// </summary>
    public static Color GetCategoryColor(JobCategory category)
    {
        return category switch
        {
            JobCategory.Basic => new Color(0.8f, 0.8f, 0.8f), // Light Gray
            JobCategory.Common => new Color(0.6f, 0.8f, 1.0f), // Light Blue
            JobCategory.Special => new Color(1.0f, 0.8f, 0.4f), // Gold
            JobCategory.Unique => new Color(1.0f, 0.4f, 0.8f), // Pink
            JobCategory.Monster => new Color(0.6f, 0.4f, 0.4f), // Brown
            JobCategory.Guest => new Color(0.8f, 1.0f, 0.6f), // Light Green
            _ => Color.white
        };
    }

    /// <summary>
    /// Formats JP display (e.g., "1,234 JP")
    /// </summary>
    public static string FormatJP(int jp)
    {
        return $"{jp:N0} JP";
    }

    /// <summary>
    /// Formats job level display (e.g., "Lv.5")
    /// </summary>
    public static string FormatJobLevel(int level)
    {
        return $"Lv.{level}";
    }

    /// <summary>
    /// Gets progress bar fill percentage
    /// </summary>
    public static float GetProgressFill(int current, int max)
    {
        if (max <= 0)
            return 0f;
        
        return Mathf.Clamp01((float)current / max);
    }
}
