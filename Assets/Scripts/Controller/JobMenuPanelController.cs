using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controller for the Job Selection Menu Panel
/// Displays available jobs, allows job switching, shows prerequisites
/// 
/// CONTEXT-AWARE OPERATION:
/// - Can be shown from PostBattleController (after battle victory)
/// - Can be shown from World/Menu (when implemented)
/// - Works with UIManager for proper state management
/// - Integrates with GameStateManager for context awareness
/// </summary>
public class JobMenuPanelController : MonoBehaviour
{
    #region Constants

    private const string ShowKey = "Show";
    private const string HideKey = "Hide";
    private const string EntryPoolKey = "JobMenuPanel.Entry";
    private const int MaxMenuEntries = 20;

    #endregion

    #region Fields / Properties

    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI characterNameLabel;
    [SerializeField] private TextMeshProUGUI currentJobLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private TextMeshProUGUI prerequisitesLabel;
    [SerializeField] private Panel panel;
    [SerializeField] private GameObject canvas;

    private readonly List<JobMenuEntry> menuEntries = new List<JobMenuEntry>();
    private readonly List<JobDefinition> displayedJobs = new List<JobDefinition>();
    private JobManager currentJobManager;
    private int selection;

    public int Selection => selection;
    public JobDefinition SelectedJob => selection >= 0 && selection < displayedJobs.Count ? displayedJobs[selection] : null;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        if (entryPrefab != null)
        {
            GameObjectPoolController.AddEntry(EntryPoolKey, entryPrefab, MaxMenuEntries, MaxMenuEntries);
        }
    }

    private void Start()
    {
        if (panel != null)
        {
            panel.SetPosition(HideKey, false);
        }

        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Shows the job menu for a specific character
    /// </summary>
    public void Show(JobManager jobManager)
    {
        if (jobManager == null)
        {
            Debug.LogError("Cannot show job menu with null JobManager");
            return;
        }

        currentJobManager = jobManager;

        if (canvas != null)
            canvas.SetActive(true);

        Clear();

        // Set character info
        if (characterNameLabel != null)
            characterNameLabel.text = jobManager.gameObject.name;

        if (currentJobLabel != null && jobManager.CurrentJob != null)
            currentJobLabel.text = $"Current: {jobManager.CurrentJob.jobName}";

        if (titleLabel != null)
            titleLabel.text = "Job Selection";

        // Get all jobs
        var allJobs = jobManager.allJobs;
        if (allJobs == null || allJobs.Count == 0)
        {
            Debug.LogWarning("No jobs available");
            return;
        }

        // Display all jobs (unlocked and locked)
        displayedJobs.Clear();
        foreach (var job in allJobs)
        {
            if (job == null)
                continue;

            displayedJobs.Add(job);

            var entry = DequeueEntry();
            bool isUnlocked = jobManager.ProgressData.IsJobUnlocked(job);
            bool isCurrent = job == jobManager.CurrentJob;
            int jobLevel = jobManager.ProgressData.GetJobLevel(job);
            int jobJP = jobManager.ProgressData.GetJobJP(job);

            entry.SetJobData(job, jobLevel, jobJP, isCurrent, !isUnlocked);
            menuEntries.Add(entry);
        }

        SetSelection(0);
        TogglePosition(ShowKey);
    }

    /// <summary>
    /// Hides the job menu
    /// </summary>
    public void Hide()
    {
        var t = TogglePosition(HideKey);

        if (t != null)
        {
            t.completedEvent += delegate
            {
                OnHideComplete();
            };
        }
        else
        {
            OnHideComplete();
        }
    }

    /// <summary>
    /// Moves selection to next job
    /// </summary>
    public void Next()
    {
        for (int i = selection + 1; i < selection + menuEntries.Count; i++)
        {
            int index = i % menuEntries.Count;
            if (SetSelection(index))
                break;
        }
    }

    /// <summary>
    /// Moves selection to previous job
    /// </summary>
    public void Previous()
    {
        for (int i = selection - 1 + menuEntries.Count; i > selection; i--)
        {
            int index = i % menuEntries.Count;
            if (SetSelection(index))
                break;
        }
    }

    /// <summary>
    /// Attempts to switch to selected job
    /// </summary>
    public bool ConfirmSelection()
    {
        var selectedJob = SelectedJob;
        if (selectedJob == null || currentJobManager == null)
            return false;

        // Check if job is unlocked
        if (!currentJobManager.ProgressData.IsJobUnlocked(selectedJob))
        {
            // Try to unlock
            if (currentJobManager.TryUnlockJob(selectedJob, out string failureReason))
            {
                Debug.Log($"Unlocked job: {selectedJob.jobName}");
                // Refresh display
                Show(currentJobManager);
                return false; // Don't switch yet, let player confirm
            }
            else
            {
                Debug.LogWarning($"Cannot unlock job: {failureReason}");
                UpdatePrerequisitesDisplay(failureReason);
                return false;
            }
        }

        // Switch to job
        bool success = currentJobManager.SwitchJob(selectedJob);

        if (success)
        {
            Debug.Log($"Switched to job: {selectedJob.jobName}");
            Hide();
        }

        return success;
    }

    #endregion

    #region Private Methods

    private JobMenuEntry DequeueEntry()
    {
        var poolable = GameObjectPoolController.Dequeue(EntryPoolKey);
        var entry = poolable.GetComponent<JobMenuEntry>();

        if (entry != null && panel != null)
        {
            entry.transform.SetParent(panel.transform, false);
            entry.transform.localScale = Vector3.one;
            entry.gameObject.SetActive(true);
            entry.Reset();
        }

        return entry;
    }

    private void EnqueueEntry(JobMenuEntry entry)
    {
        if (entry == null)
            return;

        var poolable = entry.GetComponent<Poolable>();
        if (poolable != null)
        {
            GameObjectPoolController.Enqueue(poolable);
        }
    }

    private void Clear()
    {
        foreach (var entry in menuEntries)
        {
            EnqueueEntry(entry);
        }

        menuEntries.Clear();
        displayedJobs.Clear();
    }

    private bool SetSelection(int value)
    {
        if (value < 0 || value >= menuEntries.Count)
            return false;

        // Can't select null entries
        if (menuEntries[value] == null || menuEntries[value].Job == null)
            return false;

        // Deselect previous
        if (selection >= 0 && selection < menuEntries.Count && menuEntries[selection] != null)
        {
            menuEntries[selection].IsSelected = false;
        }

        // Select new
        selection = value;
        menuEntries[selection].IsSelected = true;

        // Update description panel
        UpdateJobDescription(menuEntries[selection].Job);

        return true;
    }

    private void UpdateJobDescription(JobDefinition job)
    {
        if (job == null)
            return;

        if (descriptionLabel != null)
        {
            descriptionLabel.text = job.description;
        }

        if (prerequisitesLabel != null && currentJobManager != null)
        {
            if (currentJobManager.ProgressData.IsJobUnlocked(job))
            {
                prerequisitesLabel.text = "<color=green>UNLOCKED</color>";
            }
            else
            {
                // Show prerequisites
                var prereqText = "Prerequisites:\n";
                foreach (var prereq in job.prerequisites)
                {
                    if (prereq != null)
                    {
                        bool met = prereq.IsMet(currentJobManager.ProgressData);
                        string color = met ? "green" : "red";
                        prereqText += $"<color={color}>• {prereq}</color>\n";
                    }
                }

                if (job.minimumCharacterLevel > 1)
                {
                    var rank = currentJobManager.GetComponent<Rank>();
                    int currentLevel = rank != null ? rank.LVL : 1;
                    bool levelMet = currentLevel >= job.minimumCharacterLevel;
                    string color = levelMet ? "green" : "red";
                    prereqText += $"<color={color}>• Character Lv.{job.minimumCharacterLevel}</color>\n";
                }

                prerequisitesLabel.text = prereqText;
            }
        }
    }

    private void UpdatePrerequisitesDisplay(string message)
    {
        if (prerequisitesLabel != null)
        {
            prerequisitesLabel.text = $"<color=red>{message}</color>";
        }
    }

    private Tweener TogglePosition(string key)
    {
        if (panel == null)
            return null;

        return panel.SetPosition(key, true);
    }

    private void OnHideComplete()
    {
        if (panel != null && panel.CurrentPosition == panel[HideKey])
        {
            Clear();

            if (canvas != null)
                canvas.SetActive(false);
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    [ContextMenu("Debug: Test Show")]
    private void DebugTestShow()
    {
        var jobManager = FindObjectOfType<JobManager>();
        if (jobManager != null)
        {
            Show(jobManager);
        }
        else
        {
            Debug.LogWarning("No JobManager found in scene");
        }
    }
#endif

    #endregion

    #region Post-Battle Integration

    /// <summary>
    /// Called when player wants to view next unit in post-battle
    /// Notifies PostBattleController to show next unit's job menu
    /// </summary>
    public void OnNextUnitButton()
    {
        var postBattleController = FindObjectOfType<PostBattleController>();
        if (postBattleController != null && postBattleController.enabled)
        {
            Hide();
            postBattleController.ShowNextUnitJobMenu();
        }
        else
        {
            Debug.LogWarning("[JobMenuPanel] NextUnit button pressed but not in PostBattle context");
        }
    }

    /// <summary>
    /// Called when player wants to view previous unit in post-battle
    /// Notifies PostBattleController to show previous unit's job menu
    /// </summary>
    public void OnPreviousUnitButton()
    {
        var postBattleController = FindObjectOfType<PostBattleController>();
        if (postBattleController != null && postBattleController.enabled)
        {
            Hide();
            postBattleController.ShowPreviousUnitJobMenu();
        }
        else
        {
            Debug.LogWarning("[JobMenuPanel] PreviousUnit button pressed but not in PostBattle context");
        }
    }

    /// <summary>
    /// Check if we're in post-battle context (for showing Next/Previous Unit buttons)
    /// </summary>
    public bool IsPostBattleContext()
    {
        if (GameStateManager.Instance != null)
        {
            return GameStateManager.Instance.IsPostBattle();
        }

        // Fallback: Check if PostBattleController exists and is enabled
        var postBattleController = FindObjectOfType<PostBattleController>();
        return postBattleController != null && postBattleController.enabled;
    }

    #endregion
}
