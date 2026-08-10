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

    [Tooltip("Stable id of the currently active job (survives save/load and display renames)")]
    public string currentJobId = "";

    [Tooltip("Runtime reference to the currently active job. Not serialized — a ScriptableObject reference does not survive JsonUtility round-trips; resolved from currentJobId on load.")]
    [NonSerialized]
    public JobDefinition currentJob;

    [Tooltip("Dictionary mapping job ids to JP accumulated")]
    public SerializableDictionary<string, int> jobJP = new SerializableDictionary<string, int>();

    [Tooltip("Dictionary mapping job ids to job levels achieved")]
    public SerializableDictionary<string, int> jobLevels = new SerializableDictionary<string, int>();

    [Tooltip("Set of unlocked job ids")]
    public List<string> unlockedJobs = new List<string>();

    [Tooltip("Cert (JP) spent on ability purchases per job id. The spendable bank is earned minus spent; grades always derive from the earned total, so purchases never regress a job level (issue #51)")]
    public SerializableDictionary<string, int> jobSpentJP = new SerializableDictionary<string, int>();

    [Tooltip("Character level at time of last stat calculation (for validation)")]
    public int lastCalculatedLevel = 1;

    #endregion

    #region Identity

    /// <summary>
    /// Stable key for a job in progress/save data. Prefers the job's id;
    /// falls back to display name for legacy assets without ids.
    /// </summary>
    public static string JobKey(JobDefinition job)
    {
        if (job == null)
            return "";
        return string.IsNullOrEmpty(job.id) ? job.jobName : job.id;
    }

    /// <summary>
    /// Restores the currentJob reference from currentJobId after deserialization.
    /// </summary>
    public void ResolveCurrentJob(List<JobDefinition> allJobs)
    {
        if (string.IsNullOrEmpty(currentJobId) || allJobs == null)
            return;

        currentJob = allJobs.Find(j => j != null && JobKey(j) == currentJobId);
    }

    #endregion

    #region Constructor
    
    public JobProgressData()
    {
        jobJP = new SerializableDictionary<string, int>();
        jobLevels = new SerializableDictionary<string, int>();
        unlockedJobs = new List<string>();
        jobSpentJP = new SerializableDictionary<string, int>();
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
        currentJobId = JobKey(startingJob);
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

        string key = JobKey(job);

        if (!unlockedJobs.Contains(key))
        {
            unlockedJobs.Add(key);

            // Initialize JP and level if not already present
            if (!jobJP.ContainsKey(key))
                jobJP[key] = 0;

            if (!jobLevels.ContainsKey(key))
                jobLevels[key] = 1;
        }
    }

    /// <summary>
    /// Checks if a job is unlocked
    /// </summary>
    public bool IsJobUnlocked(JobDefinition job)
    {
        if (job == null)
            return false;

        return unlockedJobs.Contains(JobKey(job));
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
        currentJobId = JobKey(newJob);
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

        string key = JobKey(job);

        if (!jobJP.ContainsKey(key))
            jobJP[key] = 0;

        int oldJP = jobJP[key];
        int newJP = oldJP + jp;
        jobJP[key] = newJP;

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

        return jobJP.TryGetValue(JobKey(job), out int jp) ? jp : 0;
    }

    /// <summary>
    /// Sets JP for a specific job (used for loading saved data)
    /// </summary>
    public void SetJobJP(JobDefinition job, int jp)
    {
        if (job == null)
            return;

        jobJP[JobKey(job)] = Mathf.Max(0, jp);
        
        // Update level based on JP
        int level = job.GetJobLevelForJP(jp);
        SetJobLevel(job, level);
    }

    /// <summary>Cert already spent on ability purchases from this job.</summary>
    public int GetSpentJP(JobDefinition job)
    {
        if (job == null)
            return 0;

        return jobSpentJP != null && jobSpentJP.TryGetValue(JobKey(job), out int spent) ? spent : 0;
    }

    /// <summary>
    /// The spendable Cert bank for a job: earned total minus purchases.
    /// Grades derive from the earned total, so spending never lowers them.
    /// </summary>
    public int GetAvailableJP(JobDefinition job)
    {
        return Mathf.Max(0, GetJobJP(job) - GetSpentJP(job));
    }

    /// <summary>
    /// Spends Cert from a job's bank (issue #51: Cert buys abilities).
    /// Returns false without side effects when the bank cannot cover it.
    /// </summary>
    public bool TrySpendJP(JobDefinition job, int amount)
    {
        if (job == null || amount < 0)
            return false;

        if (GetAvailableJP(job) < amount)
            return false;

        jobSpentJP ??= new SerializableDictionary<string, int>();
        jobSpentJP[JobKey(job)] = GetSpentJP(job) + amount;
        return true;
    }

    /// <summary>
    /// Gets job level for a specific job. Returns 0 when the job has no
    /// progress entry — "never trained" must stay distinct from Grade 1, or
    /// locked jobs leak their Grade-1 abilities (issue #51).
    /// </summary>
    public int GetJobLevel(JobDefinition job)
    {
        if (job == null)
            return 0;

        return jobLevels.TryGetValue(JobKey(job), out int level) ? level : 0;
    }

    /// <summary>
    /// Whether this character has any progress entry for the job (created on
    /// unlock). Jobs without an entry contribute nothing to stats or abilities.
    /// </summary>
    public bool HasJobEntry(JobDefinition job)
    {
        if (job == null)
            return false;

        return jobLevels.ContainsKey(JobKey(job));
    }

    /// <summary>
    /// Sets job level for a specific job
    /// </summary>
    public void SetJobLevel(JobDefinition job, int level)
    {
        if (job == null)
            return;

        jobLevels[JobKey(job)] = Mathf.Clamp(level, 1, 8);
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
            if (job != null && jobLevels.TryGetValue(JobKey(job), out int level))
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
