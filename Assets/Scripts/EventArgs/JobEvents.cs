using System;

/// <summary>
/// Event arguments for job system events
/// </summary>
public class JobEventArgs : EventArgs
{
    public JobDefinition Job { get; set; }
    public int OldValue { get; set; }
    public int NewValue { get; set; }
}

/// <summary>
/// Event fired when a job is switched
/// </summary>
public class JobSwitchedEvent
{
    public JobDefinition OldJob { get; set; }
    public JobDefinition NewJob { get; set; }
    public object Source { get; set; }

    public JobSwitchedEvent(object source, JobDefinition oldJob, JobDefinition newJob)
    {
        Source = source;
        OldJob = oldJob;
        NewJob = newJob;
    }
}

/// <summary>
/// Event fired when a job is unlocked
/// </summary>
public class JobUnlockedEvent
{
    public JobDefinition Job { get; set; }
    public object Source { get; set; }

    public JobUnlockedEvent(object source, JobDefinition job)
    {
        Source = source;
        Job = job;
    }
}

/// <summary>
/// Event fired when a job levels up
/// </summary>
public class JobLevelUpEvent
{
    public JobDefinition Job { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
    public object Source { get; set; }

    public JobLevelUpEvent(object source, JobDefinition job, int oldLevel, int newLevel)
    {
        Source = source;
        Job = job;
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}

/// <summary>
/// Event fired when JP is gained
/// </summary>
public class JobPointsGainedEvent
{
    public JobDefinition Job { get; set; }
    public int JPGained { get; set; }
    public int TotalJP { get; set; }
    public object Source { get; set; }

    public JobPointsGainedEvent(object source, JobDefinition job, int jpGained, int totalJP)
    {
        Source = source;
        Job = job;
        JPGained = jpGained;
        TotalJP = totalJP;
    }
}

/// <summary>
/// Event fired when an ability is learned
/// </summary>
public class AbilityLearnedEvent
{
    public string AbilityName { get; set; }
    public JobDefinition FromJob { get; set; }
    public object Source { get; set; }

    public AbilityLearnedEvent(object source, string abilityName, JobDefinition fromJob)
    {
        Source = source;
        AbilityName = abilityName;
        FromJob = fromJob;
    }
}

/// <summary>
/// Event fired when job prerequisites are checked
/// </summary>
public class JobPrerequisiteCheckEvent
{
    public JobDefinition Job { get; set; }
    public bool CanUnlock { get; set; }
    public string FailureReason { get; set; }
    public object Source { get; set; }

    public JobPrerequisiteCheckEvent(object source, JobDefinition job, bool canUnlock, string failureReason = "")
    {
        Source = source;
        Job = job;
        CanUnlock = canUnlock;
        FailureReason = failureReason;
    }
}
