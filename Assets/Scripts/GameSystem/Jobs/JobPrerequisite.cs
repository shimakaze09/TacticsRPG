using System;
using UnityEngine;

/// <summary>
/// Represents a prerequisite for unlocking a job.
/// FFT-style: requires specific job levels before unlocking advanced jobs.
/// Example: Knight requires Squire Lv2
/// </summary>
[Serializable]
public class JobPrerequisite
{
    [Tooltip("The job that must be leveled")]
    public JobDefinition requiredJob;
    
    [Tooltip("The minimum level required in that job")]
    [Range(1, 8)]
    public int requiredLevel = 2;

    /// <summary>
    /// Checks if this prerequisite is met based on job progress data
    /// </summary>
    public bool IsMet(JobProgressData progressData)
    {
        if (requiredJob == null)
        {
            Debug.LogWarning("JobPrerequisite has null requiredJob");
            return false;
        }

        if (progressData == null)
        {
            Debug.LogWarning("JobProgressData is null when checking prerequisites");
            return false;
        }

        int currentLevel = progressData.GetJobLevel(requiredJob);
        return currentLevel >= requiredLevel;
    }

    public override string ToString()
    {
        if (requiredJob == null)
            return "Invalid Prerequisite";
        
        return $"{requiredJob.jobName} Lv.{requiredLevel}";
    }
}
