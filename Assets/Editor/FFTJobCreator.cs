using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Data-driven editor utility to create all FFT job definition assets from JSON files
/// Uses a single unified function to process all job types
/// </summary>
public static class FFTJobCreator
{
    private const string JobsPath = "Assets/Resources/Jobs";
    private const string JobDataPath = "Assets/Resources/JobData";

    [MenuItem("Tactics RPG/Create FFT Jobs/Generate All Jobs from JSON")]
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
            string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
            AssetDatabase.DeleteAsset(relativePath);
            Debug.Log($"Deleted existing job: {relativePath}");
        }
        
        Debug.Log($"Deleted {jobFiles.Length} existing job assets");
    }

    private static void CreateJobDefinitionWithoutPrerequisites(JobDataFile jobData)
    {
        string assetPath = $"{JobsPath}/{jobData.jobName}.asset";
        
        // Create new JobDefinition asset
        var job = ScriptableObject.CreateInstance<JobDefinition>();
        
        // Set basic information
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
                var unlock = new JobAbilityUnlock();
                unlock.abilityName = unlockData.abilityName;
                unlock.unlockAtJobLevel = unlockData.unlockAtJobLevel;
                unlock.jpCost = unlockData.jpCost;
                job.abilityUnlocks.Add(unlock);
            }
        }
        
        // Set ability catalog name
        job.abilityCatalogName = jobData.abilityCatalogName;
        
        // Set JP requirements
        if (jobData.jpRequirements != null && jobData.jpRequirements.Length == 8)
        {
            job.jpRequirements = jobData.jpRequirements;
        }
        
        // Create the asset
        AssetDatabase.CreateAsset(job, assetPath);
        EditorUtility.SetDirty(job);
    }

    private static void SetJobPrerequisites(JobDataFile jobData)
    {
        string assetPath = $"{JobsPath}/{jobData.jobName}.asset";
        var job = AssetDatabase.LoadAssetAtPath<JobDefinition>(assetPath);
        
        if (job == null)
        {
            Debug.LogError($"Could not find job asset: {assetPath}");
            return;
        }
        
        // Set prerequisites now that all jobs exist
        job.prerequisites = new List<JobPrerequisite>();
        if (jobData.prerequisites != null)
        {
            foreach (var prereqData in jobData.prerequisites)
            {
                var prereq = new JobPrerequisite();
                prereq.requiredJob = AssetDatabase.LoadAssetAtPath<JobDefinition>($"{JobsPath}/{prereqData.requiredJobName}.asset");
                prereq.requiredLevel = prereqData.requiredLevel;
                
                if (prereq.requiredJob == null)
                {
                    Debug.LogError($"Could not find prerequisite job: {prereqData.requiredJobName}");
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
    public string abilityName;
    public int unlockAtJobLevel;
    public int jpCost;
}

#endif
