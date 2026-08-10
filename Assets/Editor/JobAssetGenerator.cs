using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Data-driven editor utility to create all job definition assets from JSON files
/// Uses a single unified function to process all job types
/// </summary>
public static class JobAssetGenerator
{
    private const string JobsPath = "Assets/Resources/Jobs";
    private const string JobDataPath = "Assets/Resources/JobData";

    [MenuItem("Tactics RPG/Generate Content/Jobs")]
    public static void GenerateJobsFromJSON()
    {
        EnsureDirectoriesExist();
        DeleteExistingJobAssets();
        
        // Get all JSON files in the JobData directory
        string[] jsonFiles = Directory.GetFiles(JobDataPath, "*.json", SearchOption.TopDirectoryOnly);
        
        // First pass: Create all jobs without prerequisites
        int jobsCreated = 0;
        List<JobDataFile> jobDataList = new List<JobDataFile>();
        
        foreach (string jsonFile in jsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            string relativePath = jsonFile.Replace(Application.dataPath, "Assets");
            
            try
            {
                JobDataFile jobData = LoadJsonData<JobDataFile>(relativePath);
                CreateJobDefinitionWithoutPrerequisites(jobData);
                jobDataList.Add(jobData);
                jobsCreated++;
                Debug.Log($"Created job: {jobData.jobName} ({jobData.category})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create job from {fileName}: {e.Message}");
                Debug.LogError($"Attempted path: {relativePath}");
            }
        }
        
        // Second pass: Set prerequisites now that all jobs exist
        foreach (var jobData in jobDataList)
        {
            try
            {
                SetJobPrerequisites(jobData);
                Debug.Log($"Set prerequisites for: {jobData.jobName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set prerequisites for {jobData.jobName}: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully created {jobsCreated} job definitions from JSON files!");
    }

    private static void EnsureDirectoriesExist()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(JobsPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Jobs");

        if (!AssetDatabase.IsValidFolder(JobDataPath))
            AssetDatabase.CreateFolder("Assets/Resources", "JobData");
    }

    private static void DeleteExistingJobAssets()
    {
        // Get all .asset files in the Jobs folder
        string[] jobFiles = Directory.GetFiles(JobsPath, "*.asset", SearchOption.TopDirectoryOnly);
        
        foreach (string file in jobFiles)
        {
            // Directory.GetFiles was given an Assets-relative path, so the
            // results are already Assets-relative — just normalize separators.
            string relativePath = file.Replace('\\', '/');
            AssetDatabase.DeleteAsset(relativePath);
            Debug.Log($"Deleted existing job: {relativePath}");
        }
        
        Debug.Log($"Deleted {jobFiles.Length} existing job assets");
    }

    /// <summary>
    /// Fallback slug for data files that predate stable ids.
    /// </summary>
    private static string Slug(string name)
    {
        var s = System.Text.RegularExpressions.Regex.Replace(name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_");
        return s.Trim('_');
    }

    private static string JobAssetPath(JobDataFile jobData)
    {
        string id = string.IsNullOrEmpty(jobData.id) ? Slug(jobData.jobName) : jobData.id;
        return $"{JobsPath}/{id}.asset";
    }

    private static void CreateJobDefinitionWithoutPrerequisites(JobDataFile jobData)
    {
        // Asset files are named by stable id so display renames never move files
        string assetPath = JobAssetPath(jobData);

        // Create new JobDefinition asset
        var job = ScriptableObject.CreateInstance<JobDefinition>();

        // Set identity + basic information
        job.id = string.IsNullOrEmpty(jobData.id) ? Slug(jobData.jobName) : jobData.id;
        job.jobName = jobData.jobName;
        job.description = jobData.description;
        job.category = ParseJobCategory(jobData.category);
        job.isUnique = jobData.isUnique;
        job.minimumCharacterLevel = jobData.minimumCharacterLevel;
        
        // Set allowed character names if specified
        if (jobData.allowedCharacterNames != null && jobData.allowedCharacterNames.Length > 0)
        {
            job.allowedCharacterNames = new List<string>(jobData.allowedCharacterNames);
        }
        
        // Prerequisites will be set in second pass
        job.prerequisites = new List<JobPrerequisite>();
        
        // Set stat multipliers
        job.hpMultiplier = jobData.statMultipliers.hp;
        job.mpMultiplier = jobData.statMultipliers.mp;
        job.atkMultiplier = jobData.statMultipliers.atk;
        job.defMultiplier = jobData.statMultipliers.def;
        job.matMultiplier = jobData.statMultipliers.mat;
        job.mdfMultiplier = jobData.statMultipliers.mdf;
        job.spdMultiplier = jobData.statMultipliers.spd;
        
        // Set base stats
        job.baseStats = jobData.baseStats;
        
        // Set movement bonuses
        job.movementBonus = jobData.movement.movementBonus;
        job.jumpBonus = jobData.movement.jumpBonus;
        job.evadeBonus = jobData.movement.evadeBonus;
        
        // Set ability unlocks
        job.abilityUnlocks = new List<JobAbilityUnlock>();
        if (jobData.abilityUnlocks != null)
        {
            foreach (var unlockData in jobData.abilityUnlocks)
            {
                var unlock = new JobAbilityUnlock
                {
                    abilityId = string.IsNullOrEmpty(unlockData.abilityId)
                        ? $"{Slug(jobData.abilityCatalogName ?? jobData.jobName)}.{Slug(unlockData.abilityName)}"
                        : unlockData.abilityId,
                    abilityName = unlockData.abilityName,
                    unlockAtJobLevel = unlockData.unlockAtJobLevel,
                    jpCost = unlockData.jpCost
                };
                job.abilityUnlocks.Add(unlock);
            }
        }
        
        // Set ability catalog name
        job.abilityCatalogName = jobData.abilityCatalogName;
        
        // Set JP requirements — the data contract is exactly seven strictly
        // increasing cumulative thresholds for levels 2-8 (issue #20); bad
        // data fails loudly and the asset keeps the validated default curve
        if (ValidateJPRequirements(jobData))
        {
            job.jpRequirements = jobData.jpRequirements;
        }
        
        // Create the asset
        AssetDatabase.CreateAsset(job, assetPath);
        EditorUtility.SetDirty(job);
    }

    // Guards the JP threshold contract (exactly 7 entries, strictly
    // increasing) and unlock levels (1-8); logs a specific error per violation
    private static bool ValidateJPRequirements(JobDataFile jobData)
    {
        var valid = true;

        if (jobData.jpRequirements == null ||
            jobData.jpRequirements.Length != JobDefinition.JPThresholdCount)
        {
            Debug.LogError($"{jobData.jobName}: jpRequirements must have exactly " +
                           $"{JobDefinition.JPThresholdCount} entries (levels 2-8), found " +
                           $"{jobData.jpRequirements?.Length ?? 0}");
            valid = false;
        }
        else
        {
            for (int i = 0; i < jobData.jpRequirements.Length; i++)
            {
                if (jobData.jpRequirements[i] > 0 &&
                    (i == 0 || jobData.jpRequirements[i] > jobData.jpRequirements[i - 1]))
                    continue;
                Debug.LogError($"{jobData.jobName}: jpRequirements[{i}] = " +
                               $"{jobData.jpRequirements[i]} must be positive and strictly " +
                               "greater than the previous threshold");
                valid = false;
            }
        }

        if (jobData.abilityUnlocks != null)
        {
            foreach (var unlock in jobData.abilityUnlocks)
            {
                if (unlock.unlockAtJobLevel >= 1 && unlock.unlockAtJobLevel <= 8)
                    continue;
                Debug.LogError($"{jobData.jobName}: ability '{unlock.abilityName}' unlocks at " +
                               $"job level {unlock.unlockAtJobLevel}, outside the 1-8 range");
                valid = false;
            }
        }

        return valid;
    }

    private static void SetJobPrerequisites(JobDataFile jobData)
    {
        string assetPath = JobAssetPath(jobData);
        var job = AssetDatabase.LoadAssetAtPath<JobDefinition>(assetPath);

        if (job == null)
        {
            Debug.LogError($"Could not find job asset: {assetPath}");
            return;
        }

        // Set prerequisites now that all jobs exist (resolved by stable id)
        job.prerequisites = new List<JobPrerequisite>();
        if (jobData.prerequisites != null)
        {
            foreach (var prereqData in jobData.prerequisites)
            {
                string requiredId = string.IsNullOrEmpty(prereqData.requiredJobId)
                    ? Slug(prereqData.requiredJobName)
                    : prereqData.requiredJobId;

                var prereq = new JobPrerequisite
                {
                    requiredJob = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/{requiredId}.asset"),
                    requiredLevel = prereqData.requiredLevel
                };

                if (prereq.requiredJob == null)
                {
                    Debug.LogError($"Could not find prerequisite job: {requiredId} (from '{prereqData.requiredJobName}')");
                }
                else
                {
                    job.prerequisites.Add(prereq);
                }
            }
        }

        EditorUtility.SetDirty(job);
    }

    private static JobCategory ParseJobCategory(string categoryString)
    {
        return categoryString.ToLower() switch
        {
            "basic" => JobCategory.Basic,
            "common" => JobCategory.Common,
            "special" => JobCategory.Special,
            "unique" => JobCategory.Unique,
            "monster" => JobCategory.Monster,
            "guest" => JobCategory.Guest,
            _ => JobCategory.Common
        };
    }

    private static T LoadJsonData<T>(string path)
    {
        string jsonContent = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(jsonContent);
    }

}

// JSON Data Structures
[System.Serializable]
public class JobDataFile
{
    public string id;
    public string abilityCatalogId;
    public string jobName;
    public string description;
    public string category;
    public bool isUnique;
    public int minimumCharacterLevel;
    public string[] allowedCharacterNames;
    public JobPrerequisiteData[] prerequisites;
    public StatMultipliers statMultipliers;
    public int[] baseStats;
    public MovementData movement;
    public JobAbilityUnlockData[] abilityUnlocks;
    public string abilityCatalogName;
    public int[] jpRequirements;
}

[System.Serializable]
public class JobPrerequisiteData
{
    public string requiredJobId;
    public string requiredJobName;
    public int requiredLevel;
}

[System.Serializable]
public class StatMultipliers
{
    public float hp;
    public float mp;
    public float atk;
    public float def;
    public float mat;
    public float mdf;
    public float spd;
}

[System.Serializable]
public class MovementData
{
    public int movementBonus;
    public int jumpBonus;
    public int evadeBonus;
}

[System.Serializable]
public class JobAbilityUnlockData
{
    public string abilityId;
    public string abilityName;
    public int unlockAtJobLevel;
    public int jpCost;
}

#endif
