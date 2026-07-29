using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Per-unit brain of the job system: switches and unlocks jobs, accumulates JP
/// into job levels, recalculates stats from the unit's whole job-level history
/// (final stats = sum of each job's levels x its multipliers, plus the current
/// job's movement bonuses, scaled/capped for difficulty and StatLimits), learns
/// abilities as job levels rise, publishes job events, and persists progress
/// for Hero units. Lives alongside Stats/Rank/Equipment on the unit.
/// </summary>
public class JobManager : MonoBehaviour, IDataPersistence
{
    #region Fields / Properties

    /// <summary>
    /// Canonical stat calculation order shared by the job system:
    /// MHP, MMP, ATK, DEF, MAT, MDF, SPD.
    /// JobDefinition.baseStats and stat contribution arrays use this layout.
    /// </summary>
    public static readonly StatTypes[] statOrder =
    {
        StatTypes.MHP,
        StatTypes.MMP,
        StatTypes.ATK,
        StatTypes.DEF,
        StatTypes.MAT,
        StatTypes.MDF,
        StatTypes.SPD
    };

    [Header("Job System Data")]
    [Tooltip("Character's job progress and history")]
    [SerializeField]
    private JobProgressData progressData;

    [Tooltip("Ability learning and retention data")]
    [SerializeField]
    private AbilityMemory abilityMemory;

    [Header("Configuration")]
    [Tooltip("All available jobs in the game (loaded from Resources)")]
    public List<JobDefinition> allJobs = new List<JobDefinition>();

    [Tooltip("JP gained per character level up (default: 100)")]
    public int jpPerLevel = 100;

    [Tooltip("JP multiplier for battles won (not implemented in this version)")]
    public float jpBattleMultiplier = 1.0f;

    // Component references
    private Stats stats;
    private Rank rank;
    private Equipment equipment;
    private AbilityCatalog abilityCatalog;

    // Cached values
    private int lastProcessedLevel = 1;

    // Baseline MOV/JMP/EVD captured before any job bonus is applied, so
    // recalculation can set (base + bonus) instead of accumulating drift.
    private int baseMoveStat;
    private int baseJumpStat;
    private int baseEvadeStat;
    private bool movementBaseCaptured;

    #endregion

    #region Properties

    public JobDefinition CurrentJob => progressData?.currentJob;
    public JobProgressData ProgressData => progressData;
    public AbilityMemory AbilityMemory => abilityMemory;
    public int CurrentJobLevel => progressData?.GetJobLevel(CurrentJob) ?? 1;
    public int CurrentJobJP => progressData?.GetJobJP(CurrentJob) ?? 0;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        // Get component references
        stats = GetComponent<Stats>();
        rank = GetComponent<Rank>();
        equipment = GetComponent<Equipment>();
        abilityCatalog = GetComponentInChildren<AbilityCatalog>();

        // Initialize data structures if null
        if (progressData == null)
            progressData = new JobProgressData();

        if (abilityMemory == null)
            abilityMemory = new AbilityMemory();

        // Load all jobs from Resources
        if (allJobs == null || allJobs.Count == 0)
            LoadAllJobs();
    }

    private void OnEnable()
    {
        // Subscribe to level-up events for JP gains
        if (rank != null && stats != null)
        {
            this.SubscribeToSender<StatDidChangeEvent>(OnCharacterLevelChanged, stats);
        }

        DataPersistenceManager.Register(this);
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (rank != null && stats != null)
        {
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnCharacterLevelChanged, stats);
        }

        DataPersistenceManager.Unregister(this);
    }

    private void Start()
    {
        // Ensure we have a current job
        if (CurrentJob == null)
        {
            InitializeWithDefaultJob();
        }
        else
        {
            // Recalculate stats on start to ensure consistency
            RecalculateStats();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes job system with default starting job (Squire)
    /// </summary>
    private void InitializeWithDefaultJob()
    {
        // Try to find the default starting job (Drifter) or first available job
        JobDefinition startingJob = FindJobById("drifter");

        if (startingJob == null && allJobs.Count > 0)
        {
            startingJob = allJobs[0];
            Debug.LogWarning($"Default job 'drifter' not found, using {startingJob.jobName} as starting job");
        }

        if (startingJob == null)
        {
            Debug.LogError("No jobs available! Cannot initialize JobManager.");
            return;
        }

        progressData.InitializeWithBasicJobs(startingJob);

        // Also unlock Sawbones — every recruit starts with both basic trades
        JobDefinition sawbones = FindJobById("sawbones");
        if (sawbones != null)
        {
            progressData.UnlockJob(sawbones);
        }

        // Sync abilities
        abilityMemory.SyncLearnedAbilities(progressData, allJobs);

        // Calculate initial stats and fill HP/MP for the fresh unit
        RecalculateStats(true);

        Debug.Log($"Initialized {gameObject.name} with job: {startingJob.jobName}");
    }

    /// <summary>
    /// Loads all JobDefinition assets from Resources
    /// </summary>
    private void LoadAllJobs()
    {
        var loadedJobs = Resources.LoadAll<JobDefinition>("Jobs");

        if (loadedJobs == null || loadedJobs.Length == 0)
        {
            Debug.LogWarning("No JobDefinition assets found in Resources/Jobs folder");
            allJobs = new List<JobDefinition>();
            return;
        }

        allJobs = new List<JobDefinition>(loadedJobs);
        Debug.Log($"Loaded {allJobs.Count} job definitions");
    }

    #endregion

    #region Job Switching

    /// <summary>
    /// Attempts to switch to a new job
    /// Validates prerequisites, unlocks, and recalculates stats
    /// </summary>
    public bool SwitchJob(JobDefinition newJob)
    {
        // Validation checks
        if (newJob == null)
        {
            Debug.LogError("Cannot switch to null job");
            return false;
        }

        if (CurrentJob == newJob)
        {
            Debug.LogWarning($"Already in job: {newJob.jobName}");
            return false;
        }

        if (!progressData.IsJobUnlocked(newJob))
        {
            Debug.LogWarning($"Job not unlocked: {newJob.jobName}");
            return false;
        }

        // Store old job for event
        JobDefinition oldJob = CurrentJob;

        // Perform the switch
        bool success = progressData.SwitchJob(newJob);

        if (!success)
        {
            Debug.LogError($"Failed to switch job to {newJob.jobName}");
            return false;
        }

        // Recalculate stats based on new job
        RecalculateStats();

        // Publish job switched event
        this.Publish(new JobSwitchedEvent(this, oldJob, newJob));

        Debug.Log($"{gameObject.name} switched from {oldJob?.jobName ?? "None"} to {newJob.jobName}");

        return true;
    }

    /// <summary>
    /// Attempts to unlock a job if prerequisites are met
    /// </summary>
    public bool TryUnlockJob(JobDefinition job, out string failureReason)
    {
        failureReason = "";

        if (job == null)
        {
            failureReason = "Job is null";
            return false;
        }

        if (progressData.IsJobUnlocked(job))
        {
            failureReason = "Job already unlocked";
            return false;
        }

        int characterLevel = rank != null ? rank.LVL : 1;
        string characterName = gameObject.name;

        // Check if can unlock
        bool canUnlock = job.CanUnlock(progressData, characterLevel, characterName);

        if (!canUnlock)
        {
            // Determine specific failure reason
            if (characterLevel < job.minimumCharacterLevel)
            {
                failureReason = $"Requires character level {job.minimumCharacterLevel}";
            }
            else if (job.isUnique && !job.allowedCharacterNames.Contains(characterName))
            {
                failureReason = "This job is not available to this character";
            }
            else
            {
                // Check which prerequisites are not met
                var unmetPrereqs = new List<string>();
                foreach (var prereq in job.prerequisites)
                {
                    if (prereq != null && !prereq.IsMet(progressData))
                    {
                        unmetPrereqs.Add(prereq.ToString());
                    }
                }

                if (unmetPrereqs.Count > 0)
                {
                    failureReason = $"Missing prerequisites: {string.Join(", ", unmetPrereqs)}";
                }
                else
                {
                    failureReason = "Unknown requirement not met";
                }
            }

            return false;
        }

        // Unlock the job
        progressData.UnlockJob(job);

        // Publish unlock event
        this.Publish(new JobUnlockedEvent(this, job));

        Debug.Log($"{gameObject.name} unlocked job: {job.jobName}");

        return true;
    }

    /// <summary>
    /// Checks all jobs and unlocks any that meet prerequisites
    /// Returns list of newly unlocked jobs
    /// </summary>
    public List<JobDefinition> CheckAndUnlockJobs()
    {
        var newlyUnlocked = new List<JobDefinition>();

        foreach (var job in allJobs)
        {
            if (job == null)
                continue;

            if (TryUnlockJob(job, out _))
            {
                newlyUnlocked.Add(job);
            }
        }

        return newlyUnlocked;
    }

    #endregion

    #region JP and Level Management

    /// <summary>
    /// Adds JP to the current job
    /// Returns true if job leveled up
    /// </summary>
    public bool AddJobPoints(int jp)
    {
        if (CurrentJob == null)
        {
            Debug.LogError("No current job to add JP to");
            return false;
        }

        if (jp <= 0)
            return false;

        int oldJP = progressData.GetJobJP(CurrentJob);
        int oldLevel = progressData.GetJobLevel(CurrentJob);

        // Add JP and check for level up
        bool leveledUp = progressData.AddJobPoints(CurrentJob, jp);

        int newJP = progressData.GetJobJP(CurrentJob);
        int newLevel = progressData.GetJobLevel(CurrentJob);

        // Publish JP gained event
        this.Publish(new JobPointsGainedEvent(this, CurrentJob, jp, newJP));

        if (leveledUp)
        {
            OnJobLevelUp(CurrentJob, oldLevel, newLevel);
        }

        return leveledUp;
    }

    /// <summary>
    /// Handles job level up logic
    /// </summary>
    private void OnJobLevelUp(JobDefinition job, int oldLevel, int newLevel)
    {
        Debug.Log($"{gameObject.name}'s {job.jobName} leveled up: {oldLevel} → {newLevel}");

        // Learn new abilities unlocked at this level
        var newAbilities = job.GetUnlockedAbilities(newLevel);
        foreach (var abilityName in newAbilities)
        {
            if (abilityMemory.LearnAbility(abilityName))
            {
                this.Publish(new AbilityLearnedEvent(this, abilityName, job));
                Debug.Log($"Learned ability: {abilityName}");
            }
        }

        // Recalculate stats (job level affects stats)
        RecalculateStats();

        // Check for new job unlocks
        CheckAndUnlockJobs();

        // Publish level up event
        this.Publish(new JobLevelUpEvent(this, job, oldLevel, newLevel));
    }

    #endregion

    #region Stat Calculation (FFT-Style)

    /// <summary>
    /// CORE STAT RECALCULATION ALGORITHM
    /// 
    /// FFT-style stat calculation:
    /// 1. Start with base stats (level 1, no job)
    /// 2. For each job the character has leveled:
    ///    - Add (JobLevels * JobMultipliers) to stats
    /// 3. Apply current job's equipment bonuses
    /// 4. Apply current job's movement bonuses
    /// 
    /// This ensures stats reflect full job history, not just current job.
    /// </summary>
    public void RecalculateStats(bool restoreToFull = false)
    {
        if (stats == null)
        {
            Debug.LogWarning("Stats component not found, cannot recalculate");
            return;
        }

        // Initialize stat array (MHP, MMP, ATK, DEF, MAT, MDF, SPD)
        int[] calculatedStats = new int[7];

        // Get all job levels from progress data
        var jobLevelHistory = progressData.GetAllJobLevels(allJobs);

        // Calculate stat contributions from each job
        foreach (var kvp in jobLevelHistory)
        {
            JobDefinition job = kvp.Key;
            int jobLevels = kvp.Value;

            if (job != null && jobLevels > 0)
            {
                job.CalculateStatContribution(jobLevels, ref calculatedStats);
            }
        }

        // Apply calculated stats to Stats component
        // Using statOrder for consistency: MHP, MMP, ATK, DEF, MAT, MDF, SPD
        for (int i = 0; i < statOrder.Length && i < calculatedStats.Length; i++)
        {
            StatTypes statType = statOrder[i];
            int value = calculatedStats[i];

            // Ensure positive values, then enforce the global ceilings.
            // This write bypasses the stat-exception pipeline, so the clamp
            // must happen here — growth can never exceed StatLimits.
            value = Mathf.Max(1, value);

            // Hard difficulty: enemies get more HP (survives every recalc
            // because it's applied at the source rather than patched after)
            if (statType == StatTypes.MHP)
            {
                var unitAlliance = GetComponent<Alliance>();
                if (unitAlliance != null && unitAlliance.type == Alliances.Enemy)
                    value = Mathf.RoundToInt(value * DifficultySettings.EnemyHpMultiplier);
            }

            value = statType switch
            {
                StatTypes.MHP => Mathf.Min(value, StatLimits.MaxHP),
                StatTypes.MMP => Mathf.Min(value, StatLimits.MaxMP),
                _ => Mathf.Min(value, StatLimits.MaxPrimaryStat)
            };

            stats.SetValue(statType, value, false);
        }

        // Apply current job's movement/evade bonuses on top of the unit's
        // baseline (captured once) — never on top of the previous result,
        // which would accumulate the bonus on every recalculation.
        if (!movementBaseCaptured)
        {
            baseMoveStat = stats[StatTypes.MOV];
            baseJumpStat = stats[StatTypes.JMP];
            baseEvadeStat = stats[StatTypes.EVD];
            movementBaseCaptured = true;
        }

        int mov = baseMoveStat;
        int jmp = baseJumpStat;
        int evd = baseEvadeStat;

        if (CurrentJob != null)
        {
            mov += CurrentJob.movementBonus;
            jmp += CurrentJob.jumpBonus;
            evd += CurrentJob.evadeBonus;
        }

        stats.SetValue(StatTypes.MOV, mov, false);
        stats.SetValue(StatTypes.JMP, jmp, false);
        stats.SetValue(StatTypes.EVD, evd, false);

        // Only fill HP/MP when explicitly requested (unit creation); a
        // mid-battle recalculation (job level-up/switch) must not full-heal.
        if (restoreToFull)
        {
            stats.SetValue(StatTypes.HP, stats[StatTypes.MHP], false);
            stats.SetValue(StatTypes.MP, stats[StatTypes.MMP], false);
        }
        else
        {
            stats.SetValue(StatTypes.HP, Mathf.Min(stats[StatTypes.HP], stats[StatTypes.MHP]), false);
            stats.SetValue(StatTypes.MP, Mathf.Min(stats[StatTypes.MP], stats[StatTypes.MMP]), false);
        }

        Debug.Log($"Stats recalculated for {gameObject.name}. MHP: {stats[StatTypes.MHP]}, ATK: {stats[StatTypes.ATK]}");
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles character level changes to award JP
    /// </summary>
    private void OnCharacterLevelChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.LVL)
            return;

        int oldLevel = e.OldValue;
        int newLevel = e.NewValue;

        // Prevent duplicate processing
        if (newLevel <= lastProcessedLevel)
            return;

        // Award JP for each level gained
        int levelsGained = newLevel - Mathf.Max(oldLevel, lastProcessedLevel);

        if (levelsGained > 0 && CurrentJob != null)
        {
            int jpToAward = levelsGained * jpPerLevel;
            AddJobPoints(jpToAward);

            Debug.Log($"{gameObject.name} gained {levelsGained} levels, awarded {jpToAward} JP to {CurrentJob.jobName}");
        }

        lastProcessedLevel = newLevel;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Finds a job by name from the all jobs list
    /// </summary>
    public JobDefinition FindJobByName(string jobName)
    {
        if (string.IsNullOrEmpty(jobName))
            return null;

        return allJobs.Find(j => j != null && j.jobName == jobName);
    }

    /// <summary>
    /// Finds a job by its stable id (preferred over FindJobByName for
    /// anything persisted or cross-referenced)
    /// </summary>
    public JobDefinition FindJobById(string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
            return null;

        return allJobs.Find(j => j != null && JobProgressData.JobKey(j) == jobId);
    }

    /// <summary>
    /// Gets all unlocked jobs
    /// </summary>
    public List<JobDefinition> GetUnlockedJobs()
    {
        var unlocked = new List<JobDefinition>();

        foreach (var job in allJobs)
        {
            if (job != null && progressData.IsJobUnlocked(job))
            {
                unlocked.Add(job);
            }
        }

        return unlocked;
    }

    /// <summary>
    /// Gets all jobs that can currently be unlocked
    /// </summary>
    public List<JobDefinition> GetUnlockableJobs()
    {
        var unlockable = new List<JobDefinition>();
        int characterLevel = rank != null ? rank.LVL : 1;
        string characterName = gameObject.name;

        foreach (var job in allJobs)
        {
            if (job == null)
                continue;

            if (!progressData.IsJobUnlocked(job) && job.CanUnlock(progressData, characterLevel, characterName))
            {
                unlockable.Add(job);
            }
        }

        return unlockable;
    }

    #endregion

    #region Data Persistence

    public void LoadData(GameData data)
    {
        if (data == null || data.jobProgressData == null)
            return;

        if (!IsPersistentUnit())
            return;

        string unitName = gameObject.name;

        if (data.jobProgressData.TryGetValue(unitName, out JobProgressData loadedProgress))
        {
            progressData = loadedProgress;

            // Restore the current-job reference from its stable id
            progressData.ResolveCurrentJob(allJobs);
        }

        if (data.abilityMemoryData != null &&
            data.abilityMemoryData.TryGetValue(unitName, out AbilityMemory loadedMemory))
        {
            abilityMemory = loadedMemory;
        }

        // Recalculate stats after loading
        RecalculateStats();

        Debug.Log($"Loaded job data for {unitName}: {progressData}");
    }

    /// <summary>
    /// Only player (Hero) units persist job progress — enemies are spawned
    /// per battle and would pollute the save file.
    /// </summary>
    private bool IsPersistentUnit()
    {
        var alliance = GetComponent<Alliance>();
        return alliance != null && alliance.type == Alliances.Hero;
    }

    public void SaveData(ref GameData data)
    {
        if (data == null)
            return;

        if (!IsPersistentUnit())
            return;

        string unitName = gameObject.name;

        // Ensure dictionaries exist
        if (data.jobProgressData == null)
            data.jobProgressData = new SerializableDictionary<string, JobProgressData>();

        if (data.abilityMemoryData == null)
            data.abilityMemoryData = new SerializableDictionary<string, AbilityMemory>();

        // Save job progress
        if (progressData != null)
        {
            if (data.jobProgressData.ContainsKey(unitName))
                data.jobProgressData[unitName] = progressData;
            else
                data.jobProgressData.Add(unitName, progressData);
        }

        // Save ability memory
        if (abilityMemory != null)
        {
            if (data.abilityMemoryData.ContainsKey(unitName))
                data.abilityMemoryData[unitName] = abilityMemory;
            else
                data.abilityMemoryData.Add(unitName, abilityMemory);
        }

        Debug.Log($"Saved job data for {unitName}");
    }

    #endregion

    #region Debug / Editor

#if UNITY_EDITOR
    [ContextMenu("Debug: Print Job Status")]
    private void DebugPrintJobStatus()
    {
        Debug.Log("=== Job Manager Status ===");
        Debug.Log($"Current Job: {CurrentJob?.jobName ?? "None"}");
        Debug.Log($"Job Level: {CurrentJobLevel}");
        Debug.Log($"Job JP: {CurrentJobJP}");
        Debug.Log($"Unlocked Jobs: {progressData?.unlockedJobs.Count ?? 0}");
        Debug.Log($"Learned Abilities: {abilityMemory?.GetLearnedAbilityCount() ?? 0}");
        Debug.Log($"Character Level: {rank?.LVL ?? 0}");
    }

    [ContextMenu("Debug: Recalculate Stats")]
    private void DebugRecalculateStats()
    {
        RecalculateStats();
        Debug.Log("Stats recalculated");
    }

    [ContextMenu("Debug: Add 100 JP")]
    private void DebugAddJP()
    {
        AddJobPoints(100);
    }

    [ContextMenu("Debug: Check Unlockable Jobs")]
    private void DebugCheckUnlockableJobs()
    {
        var unlockable = GetUnlockableJobs();
        Debug.Log($"Unlockable jobs: {string.Join(", ", unlockable.ConvertAll(j => j.jobName))}");
    }
#endif

    #endregion

    #region Post-Battle Helpers

    /// <summary>
    /// Check if character has newly unlocked jobs available
    /// Used by PostBattleController to show job menu automatically
    /// </summary>
    public bool HasUnlockedJobs()
    {
        var unlockable = GetUnlockableJobs();
        return unlockable.Count > 0;
    }

    /// <summary>
    /// Get count of newly unlockable jobs
    /// </summary>
    public int GetUnlockableJobCount()
    {
        return GetUnlockableJobs().Count;
    }

    #endregion
}
