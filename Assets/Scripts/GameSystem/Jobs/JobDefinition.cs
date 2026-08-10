using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines one job (e.g. Drifter, Warden, Burner) as a generated asset: stable id,
/// display text, prerequisites in the job tree, per-level stat growth multipliers,
/// movement bonuses, ability unlocks by job level, and the JP thresholds.
/// Stats derive from levels earned in each job (history matters, not just the
/// current job); abilities unlock at job levels and are remembered permanently
/// in AbilityMemory. Max job level is 8. See Docs/WORLD.md for the roster.
/// </summary>
[CreateAssetMenu(fileName = "New Job", menuName = "Tactics RPG/Jobs/Job Definition")]
public class JobDefinition : ScriptableObject
{
    #region Basic Info
    
    [Header("Basic Information")]
    [Tooltip("Stable identifier (never changes once shipped; safe to rename jobName freely). Used for saves, prerequisites, and cross-references.")]
    public string id = "";

    [Tooltip("Display name of the job")]
    public string jobName = "New Job";
    
    [TextArea(2, 4)]
    [Tooltip("Description shown in job menu")]
    public string description = "A new job class.";
    
    [Tooltip("Icon/avatar for UI display")]
    public Sprite jobIcon;
    
    [Tooltip("Category for organizational purposes")]
    public JobCategory category = JobCategory.Common;
    
    [Tooltip("Is this job available to all characters, or unique to specific ones?")]
    public bool isUnique = false;
    
    [Tooltip("Characters who can use this job (leave empty for all)")]
    public List<string> allowedCharacterNames = new List<string>();
    
    #endregion

    #region Prerequisites
    
    [Header("Unlock Requirements")]
    [Tooltip("Jobs and levels required to unlock this job")]
    public List<JobPrerequisite> prerequisites = new List<JobPrerequisite>();
    
    [Tooltip("Minimum character level required")]
    public int minimumCharacterLevel = 1;
    
    #endregion

    #region Stat Growth
    
    [Header("Stat Growth Multipliers")]
    [Tooltip("HP multiplier per job level (FFT uses values like 1.0 - 2.5)")]
    [Range(0.5f, 3.0f)]
    public float hpMultiplier = 1.2f;
    
    [Tooltip("MP multiplier per job level")]
    [Range(0.5f, 3.0f)]
    public float mpMultiplier = 1.0f;
    
    [Tooltip("Physical Attack multiplier")]
    [Range(0.5f, 2.0f)]
    public float atkMultiplier = 1.0f;
    
    [Tooltip("Physical Defense multiplier")]
    [Range(0.5f, 2.0f)]
    public float defMultiplier = 1.0f;
    
    [Tooltip("Magic Attack multiplier")]
    [Range(0.5f, 2.0f)]
    public float matMultiplier = 1.0f;
    
    [Tooltip("Magic Defense multiplier")]
    [Range(0.5f, 2.0f)]
    public float mdfMultiplier = 1.0f;
    
    [Tooltip("Speed multiplier")]
    [Range(0.5f, 2.0f)]
    public float spdMultiplier = 1.0f;

    [Header("Base Stats (Used for stat calculation)")]
    [Tooltip("Base stats for level 1 in this job")]
    public int[] baseStats = new int[7]; // MHP, MMP, ATK, DEF, MAT, MDF, SPD

    #endregion

    #region Equipment & Movement
    
    [Header("Equipment & Movement")]
    [Tooltip("Equipment slots this job can use")]
    public List<EquipSlots> allowedEquipmentSlots = new List<EquipSlots> 
    { 
        EquipSlots.Primary, 
        EquipSlots.Secondary, 
        EquipSlots.Head, 
        EquipSlots.Body, 
        EquipSlots.Accessory 
    };
    
    [Tooltip("Movement range bonus/penalty")]
    public int movementBonus = 0;
    
    [Tooltip("Jump height bonus/penalty")]
    public int jumpBonus = 0;
    
    [Tooltip("Evade bonus")]
    public int evadeBonus = 0;
    
    #endregion

    #region Abilities
    
    [Header("Job Abilities")]
    [Tooltip("Abilities unlocked by this job at specific job levels")]
    public List<JobAbilityUnlock> abilityUnlocks = new List<JobAbilityUnlock>();
    
    [Tooltip("Path to the ability catalog for this job (e.g., 'Knight Abilities')")]
    public string abilityCatalogName;
    
    #endregion

    #region JP System
    
    /// <summary>
    /// Levels above 1 that JP can buy: exactly seven cumulative thresholds
    /// carry a job from level 2 through the level-8 cap (issue #20).
    /// </summary>
    public const int JPThresholdCount = 7;

    [Header("Job Points (JP) System")]
    [Tooltip("JP required to reach each job level (cumulative; exactly 7 entries for levels 2-8)")]
    public int[] jpRequirements = new int[JPThresholdCount]
    {
        100,   // Level 2
        250,   // Level 3
        450,   // Level 4
        700,   // Level 5
        1000,  // Level 6
        1400,  // Level 7
        1900   // Level 8
    };

    #endregion

    #region Public Methods
    
    /// <summary>
    /// Checks if a character can unlock this job
    /// </summary>
    public bool CanUnlock(JobProgressData progressData, int characterLevel, string characterName = "")
    {
        // Null check
        if (progressData == null)
        {
            Debug.LogError("JobProgressData is null");
            return false;
        }

        // Check character level requirement
        if (characterLevel < minimumCharacterLevel)
            return false;

        // Check if job is character-specific. A unique job with an empty
        // allow-list is locked for everyone (data error) rather than open to all.
        if (isUnique)
        {
            if (allowedCharacterNames.Count == 0 ||
                string.IsNullOrEmpty(characterName) ||
                !allowedCharacterNames.Contains(characterName))
                return false;
        }

        // Check all prerequisites
        foreach (var prereq in prerequisites)
        {
            if (prereq == null || !prereq.IsMet(progressData))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the job level for a given amount of JP
    /// </summary>
    public int GetJobLevelForJP(int jp)
    {
        if (jp <= 0)
            return 1;

        for (int i = jpRequirements.Length - 1; i >= 0; i--)
        {
            // +2 because index 0 is the level-2 gate; the clamp only guards
            // against oversized legacy arrays (the contract is 7 entries)
            if (jp >= jpRequirements[i])
                return Mathf.Min(i + 2, 8);
        }

        return 1;
    }

    /// <summary>
    /// Gets JP required for next job level
    /// </summary>
    public int GetJPForNextLevel(int currentJP)
    {
        int currentLevel = GetJobLevelForJP(currentJP);
        
        if (currentLevel >= 8)
            return jpRequirements[jpRequirements.Length - 1]; // Already max level

        int nextLevelIndex = currentLevel - 1; // -1 because level 2 is index 0
        return jpRequirements[nextLevelIndex];
    }

    /// <summary>
    /// Gets stable ids of all abilities unlocked at or below a specific job level.
    /// Falls back to the display name for entries without an id (legacy data).
    /// </summary>
    public List<string> GetUnlockedAbilities(int jobLevel)
    {
        var unlockedAbilities = new List<string>();

        foreach (var unlock in abilityUnlocks)
        {
            if (unlock != null && unlock.unlockAtJobLevel <= jobLevel)
            {
                unlockedAbilities.Add(string.IsNullOrEmpty(unlock.abilityId) ? unlock.abilityName : unlock.abilityId);
            }
        }

        return unlockedAbilities;
    }

    /// <summary>
    /// Growth multiplier for a stat slot in JobManager.statOrder layout
    /// (MHP, MMP, ATK, DEF, MAT, MDF, SPD) — the single lookup shared by
    /// job-grade and character-level growth.
    /// </summary>
    public float GetStatMultiplier(int statIndex)
    {
        switch (statIndex)
        {
            case 0: return hpMultiplier;
            case 1: return mpMultiplier;
            case 2: return atkMultiplier;
            case 3: return defMultiplier;
            case 4: return matMultiplier;
            case 5: return mdfMultiplier;
            case 6: return spdMultiplier;
            default: return 0f;
        }
    }

    #endregion

    #region Validation
    
    private void OnValidate()
    {
        // Ensure base stats array is correct length
        if (baseStats == null || baseStats.Length != 7)
        {
            baseStats = new int[7] { 50, 20, 5, 5, 5, 5, 5 };
        }

        // Enforce the threshold contract: exactly 7 entries (levels 2-8),
        // padding a short array so every level keeps a reachable gate
        if (jpRequirements == null || jpRequirements.Length != JPThresholdCount)
        {
            var resized = new int[JPThresholdCount];
            for (int i = 0; i < JPThresholdCount; i++)
                resized[i] = jpRequirements != null && i < jpRequirements.Length
                    ? jpRequirements[i]
                    : (i > 0 ? resized[i - 1] + 100 : 100);
            jpRequirements = resized;
        }

        // Ensure JP requirements are strictly increasing
        for (int i = 1; i < jpRequirements.Length; i++)
        {
            if (jpRequirements[i] <= jpRequirements[i - 1])
            {
                jpRequirements[i] = jpRequirements[i - 1] + 100;
            }
        }
    }
    
    #endregion
}

/// <summary>
/// Defines an ability unlocked at a specific job level
/// </summary>
[System.Serializable]
public class JobAbilityUnlock
{
    [Tooltip("Stable id of the ability to unlock (used in AbilityMemory/saves)")]
    public string abilityId;

    [Tooltip("Display name of the ability to unlock")]
    public string abilityName;
    
    [Tooltip("Job level at which this ability unlocks")]
    [Range(1, 8)]
    public int unlockAtJobLevel = 1;
    
    [Tooltip("JP cost to learn this ability (optional, for fine-tuned unlocking)")]
    public int jpCost = 0;
}
