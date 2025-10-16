using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DESIGN RATIONALE - FFT-Style Job System
/// ==========================================
/// 
/// This ScriptableObject defines a single job class (like Squire, Knight, Ninja).
/// 
/// KEY DESIGN DECISIONS:
/// 
/// 1. STAT GROWTH SYSTEM (FFT-Accurate)
///    - Each job has multipliers (not flat bonuses) for stat growth per level
///    - When a character levels up IN A JOB, that job's multipliers affect permanent stats
///    - Switching jobs recalculates stats based on job level history
///    - Example: 10 levels as Knight + 5 as Wizard = different stats than 15 levels pure Knight
/// 
/// 2. ABILITY UNLOCKING
///    - Jobs have a list of abilities unlocked at specific job levels
///    - Once unlocked, abilities are "learned" permanently (stored in AbilityMemory)
///    - Characters can equip abilities from other jobs in limited "support slots"
/// 
/// 3. JOB PREREQUISITES
///    - Advanced jobs require specific job levels (e.g., Knight needs Squire Lv2)
///    - Unique jobs may require multiple prerequisites (e.g., Ninja needs Archer Lv4 + Thief Lv5)
/// 
/// 4. JOB POINTS (JP) SYSTEM
///    - Characters earn JP per battle (separate from EXP)
///    - JP accumulates per job, allowing job leveling independent of character level
///    - Max job level is 8 (like FFT), while character level can reach 99
/// 
/// INTEGRATION NOTES:
/// - Works with existing Stats component for stat recalculation
/// - Integrates with AbilityCatalog for ability management
/// - Persists via GameData extension for save/load
/// </summary>
[CreateAssetMenu(fileName = "New Job", menuName = "Tactics RPG/Jobs/Job Definition")]
public class JobDefinition : ScriptableObject
{
    #region Basic Info
    
    [Header("Basic Information")]
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
    
    [Header("Job Points (JP) System")]
    [Tooltip("JP required to reach each job level (cumulative)")]
    public int[] jpRequirements = new int[8] 
    { 
        100,   // Level 2
        250,   // Level 3
        450,   // Level 4
        700,   // Level 5
        1000,  // Level 6
        1400,  // Level 7
        1900,  // Level 8
        2500   // Level 8 (Master)
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

        // Check if job is character-specific
        if (isUnique && allowedCharacterNames.Count > 0)
        {
            if (string.IsNullOrEmpty(characterName) || !allowedCharacterNames.Contains(characterName))
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
            if (jp >= jpRequirements[i])
                return i + 2; // +2 because array starts at level 2 requirements
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
    /// Gets all abilities unlocked at or below a specific job level
    /// </summary>
    public List<string> GetUnlockedAbilities(int jobLevel)
    {
        var unlockedAbilities = new List<string>();
        
        foreach (var unlock in abilityUnlocks)
        {
            if (unlock != null && unlock.unlockAtJobLevel <= jobLevel)
            {
                unlockedAbilities.Add(unlock.abilityName);
            }
        }
        
        return unlockedAbilities;
    }

    /// <summary>
    /// Calculates stat contribution for this job based on levels gained
    /// FFT-style: Each level in a job contributes to permanent stats
    /// </summary>
    public void CalculateStatContribution(int jobLevels, ref int[] statContribution)
    {
        if (statContribution == null || statContribution.Length < 7)
        {
            Debug.LogError("Invalid stat contribution array");
            return;
        }

        // Apply job-specific growth for each level gained in this job
        for (int i = 0; i < jobLevels; i++)
        {
            statContribution[0] += Mathf.RoundToInt(baseStats[0] * hpMultiplier); // MHP
            statContribution[1] += Mathf.RoundToInt(baseStats[1] * mpMultiplier); // MMP
            statContribution[2] += Mathf.RoundToInt(baseStats[2] * atkMultiplier); // ATK
            statContribution[3] += Mathf.RoundToInt(baseStats[3] * defMultiplier); // DEF
            statContribution[4] += Mathf.RoundToInt(baseStats[4] * matMultiplier); // MAT
            statContribution[5] += Mathf.RoundToInt(baseStats[5] * mdfMultiplier); // MDF
            statContribution[6] += Mathf.RoundToInt(baseStats[6] * spdMultiplier); // SPD
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

        // Ensure JP requirements are increasing
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
    [Tooltip("Name/path of the ability to unlock")]
    public string abilityName;
    
    [Tooltip("Job level at which this ability unlocks")]
    [Range(1, 8)]
    public int unlockAtJobLevel = 1;
    
    [Tooltip("JP cost to learn this ability (optional, for fine-tuned unlocking)")]
    public int jpCost = 0;
}
