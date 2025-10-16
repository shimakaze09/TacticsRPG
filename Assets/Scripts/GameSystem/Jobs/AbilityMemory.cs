using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages ability learning and retention across job changes (FFT-style).
/// 
/// DESIGN: Support Ability System
/// ================================
/// In FFT, characters can equip abilities from other jobs in limited slots:
/// - Primary Action Ability: From current job
/// - Support Abilities: 1-2 slots for passive abilities from any learned job
/// - Reaction Ability: 1 slot for counter/trigger abilities
/// - Movement Ability: 1 slot for movement-enhancing abilities
/// 
/// ABILITY RETENTION:
/// - Once an ability is unlocked (by reaching job level), it's "learned" permanently
/// - Learned abilities can be equipped even when in a different job
/// - Only abilities from unlocked jobs can be equipped
/// 
/// INTEGRATION:
/// - Works with JobProgressData to determine which abilities are unlocked
/// - Integrates with AbilityCatalog to manage available abilities
/// - Persists learned abilities in save data
/// </summary>
[Serializable]
public class AbilityMemory
{
    #region Serialized Data
    
    [Tooltip("All abilities ever learned by this character")]
    public List<string> learnedAbilities = new List<string>();
    
    [Tooltip("Currently equipped support abilities (max 5 in FFT)")]
    public List<string> equippedSupportAbilities = new List<string>();
    
    [Tooltip("Currently equipped reaction ability")]
    public string equippedReactionAbility = "";
    
    [Tooltip("Currently equipped movement ability")]
    public string equippedMovementAbility = "";
    
    [Tooltip("Maximum support ability slots (FFT default: 5)")]
    public int maxSupportSlots = 5;
    
    #endregion

    #region Constructor
    
    public AbilityMemory()
    {
        learnedAbilities = new List<string>();
        equippedSupportAbilities = new List<string>();
        equippedReactionAbility = "";
        equippedMovementAbility = "";
        maxSupportSlots = 5;
    }
    
    #endregion

    #region Learning Abilities
    
    /// <summary>
    /// Marks an ability as learned (permanently available)
    /// </summary>
    public bool LearnAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
        {
            Debug.LogWarning("Cannot learn null or empty ability name");
            return false;
        }

        if (learnedAbilities.Contains(abilityName))
        {
            return false; // Already learned
        }

        learnedAbilities.Add(abilityName);
        return true;
    }

    /// <summary>
    /// Learns multiple abilities at once
    /// </summary>
    public int LearnAbilities(List<string> abilityNames)
    {
        int learned = 0;
        
        if (abilityNames == null)
            return 0;

        foreach (var abilityName in abilityNames)
        {
            if (LearnAbility(abilityName))
                learned++;
        }

        return learned;
    }

    /// <summary>
    /// Checks if an ability is learned
    /// </summary>
    public bool HasLearnedAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
            return false;

        return learnedAbilities.Contains(abilityName);
    }

    /// <summary>
    /// Updates learned abilities based on current job progress
    /// Call this when job levels change
    /// </summary>
    public int UpdateLearnedAbilities(JobDefinition job, int jobLevel)
    {
        if (job == null)
            return 0;

        var unlockedAbilities = job.GetUnlockedAbilities(jobLevel);
        return LearnAbilities(unlockedAbilities);
    }

    /// <summary>
    /// Syncs all learned abilities from job progress data
    /// Useful for initialization and data integrity
    /// </summary>
    public void SyncLearnedAbilities(JobProgressData progressData, List<JobDefinition> allJobs)
    {
        if (progressData == null || allJobs == null)
            return;

        foreach (var job in allJobs)
        {
            if (job == null)
                continue;

            int jobLevel = progressData.GetJobLevel(job);
            if (jobLevel > 0)
            {
                UpdateLearnedAbilities(job, jobLevel);
            }
        }
    }
    
    #endregion

    #region Equipping Abilities
    
    /// <summary>
    /// Equips a support ability (passive/enhancement ability)
    /// </summary>
    public bool EquipSupportAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
            return false;

        // Check if ability is learned
        if (!HasLearnedAbility(abilityName))
        {
            Debug.LogWarning($"Cannot equip unlearned ability: {abilityName}");
            return false;
        }

        // Check if already equipped
        if (equippedSupportAbilities.Contains(abilityName))
            return false;

        // Check slot limit
        if (equippedSupportAbilities.Count >= maxSupportSlots)
        {
            Debug.LogWarning($"Support ability slots full ({maxSupportSlots})");
            return false;
        }

        equippedSupportAbilities.Add(abilityName);
        return true;
    }

    /// <summary>
    /// Unequips a support ability
    /// </summary>
    public bool UnequipSupportAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
            return false;

        return equippedSupportAbilities.Remove(abilityName);
    }

    /// <summary>
    /// Unequips all support abilities
    /// </summary>
    public void ClearSupportAbilities()
    {
        equippedSupportAbilities.Clear();
    }

    /// <summary>
    /// Equips a reaction ability (counter/response ability)
    /// </summary>
    public bool EquipReactionAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
        {
            equippedReactionAbility = "";
            return true;
        }

        if (!HasLearnedAbility(abilityName))
        {
            Debug.LogWarning($"Cannot equip unlearned reaction ability: {abilityName}");
            return false;
        }

        equippedReactionAbility = abilityName;
        return true;
    }

    /// <summary>
    /// Equips a movement ability
    /// </summary>
    public bool EquipMovementAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
        {
            equippedMovementAbility = "";
            return true;
        }

        if (!HasLearnedAbility(abilityName))
        {
            Debug.LogWarning($"Cannot equip unlearned movement ability: {abilityName}");
            return false;
        }

        equippedMovementAbility = abilityName;
        return true;
    }

    /// <summary>
    /// Gets all currently equipped abilities
    /// </summary>
    public List<string> GetAllEquippedAbilities()
    {
        var equipped = new List<string>();
        
        equipped.AddRange(equippedSupportAbilities);
        
        if (!string.IsNullOrEmpty(equippedReactionAbility))
            equipped.Add(equippedReactionAbility);
        
        if (!string.IsNullOrEmpty(equippedMovementAbility))
            equipped.Add(equippedMovementAbility);

        return equipped;
    }

    /// <summary>
    /// Checks if an ability is currently equipped
    /// </summary>
    public bool IsAbilityEquipped(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName))
            return false;

        return equippedSupportAbilities.Contains(abilityName) ||
               equippedReactionAbility == abilityName ||
               equippedMovementAbility == abilityName;
    }
    
    #endregion

    #region Utility
    
    /// <summary>
    /// Gets total number of learned abilities
    /// </summary>
    public int GetLearnedAbilityCount()
    {
        return learnedAbilities.Count;
    }

    /// <summary>
    /// Gets number of equipped support abilities
    /// </summary>
    public int GetEquippedSupportCount()
    {
        return equippedSupportAbilities.Count;
    }

    /// <summary>
    /// Gets available support slots
    /// </summary>
    public int GetAvailableSupportSlots()
    {
        return Mathf.Max(0, maxSupportSlots - equippedSupportAbilities.Count);
    }

    /// <summary>
    /// Validates equipped abilities against learned abilities
    /// Removes any equipped abilities that are no longer learned (data integrity)
    /// </summary>
    public void ValidateEquippedAbilities()
    {
        // Validate support abilities
        for (int i = equippedSupportAbilities.Count - 1; i >= 0; i--)
        {
            if (!HasLearnedAbility(equippedSupportAbilities[i]))
            {
                Debug.LogWarning($"Removing invalid support ability: {equippedSupportAbilities[i]}");
                equippedSupportAbilities.RemoveAt(i);
            }
        }

        // Validate reaction ability
        if (!string.IsNullOrEmpty(equippedReactionAbility) && !HasLearnedAbility(equippedReactionAbility))
        {
            Debug.LogWarning($"Removing invalid reaction ability: {equippedReactionAbility}");
            equippedReactionAbility = "";
        }

        // Validate movement ability
        if (!string.IsNullOrEmpty(equippedMovementAbility) && !HasLearnedAbility(equippedMovementAbility))
        {
            Debug.LogWarning($"Removing invalid movement ability: {equippedMovementAbility}");
            equippedMovementAbility = "";
        }
    }
    
    #endregion

    #region Debug
    
    public override string ToString()
    {
        return $"Learned: {learnedAbilities.Count}, Equipped Support: {equippedSupportAbilities.Count}/{maxSupportSlots}";
    }
    
    #endregion
}
