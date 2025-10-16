using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a character's progress across all jobs.
/// 
/// DESIGN: FFT-Style Job History
/// ==============================
/// This class tracks:
/// - Current active job
/// - JP (Job Points) accumulated in each job
/// - Job levels achieved in each job
/// - Job unlock status
/// 
/// STAT CALCULATION INTEGRATION:
/// When stats need to be recalculated (on job switch, level up, load):
/// 1. Get job level history for each job
/// 2. Each job contributes stats based on levels gained in that job
/// 3. Apply current job's equipment/movement bonuses
/// 
/// PERSISTENCE:
/// This data is serializable and saved in GameData for save/load functionality.
/// </summary>
[Serializable]
public class JobProgressData
{
    #region Serialized Data
    
    [Tooltip("Reference to currently active job")]
    public JobDefinition currentJob;
    
    [Tooltip("Dictionary mapping job names to JP accumulated")]
    public SerializableDictionary<string, int> jobJP = new SerializableDictionary<string, int>();
    
    [Tooltip("Dictionary mapping job names to job levels achieved")]
    public SerializableDictionary<string, int> jobLevels = new SerializableDictionary<string, int>();
    
    [Tooltip("Set of unlocked job names")]
    public List<string> unlockedJobs = new List<string>();
    
    [Tooltip("Character level at time of last stat calculation (for validation)")]
    public int lastCalculatedLevel = 1;
    
    #endregion

    #region Constructor
    
    public JobProgressData()
    {
        jobJP = new SerializableDictionary<string, int>();
        jobLevels = new SerializableDictionary<string, int>();
        unlockedJobs = new List<string>();
    }

    /// <summary>
    /// Initialize with starting jobs (Squire and Chemist in FFT)
    /// </summary>
    public void InitializeWithBasicJobs(JobDefinition startingJob)
    {
        if (startingJob == null)
        {
            Debug.LogError("Cannot initialize with null starting job");
            return;
        }

        currentJob = startingJob;
        UnlockJob(startingJob);
        SetJobJP(startingJob, 0);
        SetJobLevel(startingJob, 1);
    }
    
    #endregion

    #region Job Management
    
    /// <summary>
    /// Unlocks a job for this character
    /// </summary>
    public void UnlockJob(JobDefinition job)
    {
        if (job == null)
        {
            Debug.LogError("Cannot unlock null job");
            return;
        }

        string jobName = job.jobName;
        
        if (!unlockedJobs.Contains(jobName))
        {
            unlockedJobs.Add(jobName);
            
            // Initialize JP and level if not already present
            if (!jobJP.ContainsKey(jobName))
                jobJP[jobName] = 0;
            
            if (!jobLevels.ContainsKey(jobName))
                jobLevels[jobName] = 1;
        }
    }

    /// <summary>
    /// Checks if a job is unlocked
    /// </summary>
    public bool IsJobUnlocked(JobDefinition job)
    {
        if (job == null)
            return false;
        
        return unlockedJobs.Contains(job.jobName);
    }

    /// <summary>
    /// Switches to a different job (must be unlocked)
    /// </summary>
    public bool SwitchJob(JobDefinition newJob)
    {
        if (newJob == null)
        {
            Debug.LogError("Cannot switch to null job");
            return false;
        }

        if (!IsJobUnlocked(newJob))
        {
            Debug.LogWarning($"Job {newJob.jobName} is not unlocked");
            return false;
        }

        currentJob = newJob;
        return true;
    }
    
    #endregion

    #region JP and Level Management
    
    /// <summary>
    /// Adds JP to a specific job and updates job level if threshold reached
    /// </summary>
    public bool AddJobPoints(JobDefinition job, int jp)
    {
        if (job == null || jp <= 0)
            return false;

        string jobName = job.jobName;
        
        if (!jobJP.ContainsKey(jobName))
            jobJP[jobName] = 0;

        int oldJP = jobJP[jobName];
        int newJP = oldJP + jp;
        jobJP[jobName] = newJP;

        // Calculate new job level
        int oldLevel = job.GetJobLevelForJP(oldJP);
        int newLevel = job.GetJobLevelForJP(newJP);

        bool leveledUp = false;
        
        if (newLevel > oldLevel)
        {
            SetJobLevel(job, newLevel);
            leveledUp = true;
        }

        return leveledUp;
    }

    /// <summary>
    /// Gets JP for a specific job
    /// </summary>
    public int GetJobJP(JobDefinition job)
    {
        if (job == null)
            return 0;

        return jobJP.TryGetValue(job.jobName, out int jp) ? jp : 0;
    }

    /// <summary>
    /// Sets JP for a specific job (used for loading saved data)
    /// </summary>
    public void SetJobJP(JobDefinition job, int jp)
    {
        if (job == null)
            return;

        jobJP[job.jobName] = Mathf.Max(0, jp);
        
        // Update level based on JP
        int level = job.GetJobLevelForJP(jp);
        SetJobLevel(job, level);
    }

    /// <summary>
    /// Gets job level for a specific job
    /// </summary>
    public int GetJobLevel(JobDefinition job)
    {
        if (job == null)
            return 0;

        return jobLevels.TryGetValue(job.jobName, out int level) ? level : 1;
    }

    /// <summary>
    /// Sets job level for a specific job
    /// </summary>
    public void SetJobLevel(JobDefinition job, int level)
    {
        if (job == null)
            return;

        jobLevels[job.jobName] = Mathf.Clamp(level, 1, 8);
    }

    /// <summary>
    /// Gets all jobs and their levels (for stat calculation)
    /// </summary>
    public Dictionary<JobDefinition, int> GetAllJobLevels(List<JobDefinition> allJobs)
    {
        var result = new Dictionary<JobDefinition, int>();

        if (allJobs == null)
            return result;

        foreach (var job in allJobs)
        {
            if (job != null && jobLevels.TryGetValue(job.jobName, out int level))
            {
                result[job] = level;
            }
        }

        return result;
    }
    
    #endregion

    #region Utility
    
    /// <summary>
    /// Gets progress towards next job level
    /// </summary>
    public float GetJobLevelProgress(JobDefinition job)
    {
        if (job == null)
            return 0f;

        int currentJP = GetJobJP(job);
        int currentLevel = job.GetJobLevelForJP(currentJP);

        if (currentLevel >= 8)
            return 1f; // Max level

        int jpForCurrentLevel = currentLevel > 1 ? job.jpRequirements[currentLevel - 2] : 0;
        int jpForNextLevel = job.jpRequirements[currentLevel - 1];

        if (jpForNextLevel <= jpForCurrentLevel)
            return 1f;

        int jpInCurrentLevel = currentJP - jpForCurrentLevel;
        int jpNeededForLevel = jpForNextLevel - jpForCurrentLevel;

        return Mathf.Clamp01((float)jpInCurrentLevel / jpNeededForLevel);
    }

    /// <summary>
    /// Checks if a job is mastered (level 8)
    /// </summary>
    public bool IsJobMastered(JobDefinition job)
    {
        return GetJobLevel(job) >= 8;
    }

    /// <summary>
    /// Gets count of mastered jobs
    /// </summary>
    public int GetMasteredJobCount(List<JobDefinition> allJobs)
    {
        if (allJobs == null)
            return 0;

        int count = 0;
        foreach (var job in allJobs)
        {
            if (IsJobMastered(job))
                count++;
        }
        return count;
    }
    
    #endregion

    #region Debug
    
    public override string ToString()
    {
        string current = currentJob != null ? currentJob.jobName : "None";
        return $"CurrentJob: {current}, Unlocked: {unlockedJobs.Count}, JobLevels: {jobLevels.Count}";
    }
    
    #endregion
}
