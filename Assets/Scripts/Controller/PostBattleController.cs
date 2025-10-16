using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages post-battle flow: results, rewards, job changes, shop access
/// Activated after battle victory, deactivated when player continues
/// </summary>
public class PostBattleController : MonoBehaviour
{
    #region Configuration

    [Header("UI References")]
    [Tooltip("Reference to UIManager (auto-found if not set)")]
    public UIManager uiManager;

    [Tooltip("Job menu panel for job changes")]
    public JobMenuPanelController jobMenuPanel;

    [Header("Settings")]
    [Tooltip("Auto-show job menu if units can unlock new jobs")]
    public bool autoShowJobMenuOnUnlock = true;

    [Tooltip("Allow access to shop after battle")]
    public bool allowShopAccess = true;

    [Tooltip("Time to wait before showing results (seconds)")]
    public float resultsDelay = 1f;

    #endregion

    #region State

    private BattleResultsData resultsData;
    private List<Unit> unitsWithNewJobs = new List<Unit>();
    private List<Unit> leveledUpUnits = new List<Unit>();
    private int currentUnitIndex = 0;
    private bool initialized = false;

    #endregion

    #region Events

    public event System.Action OnResultsShown;
    public event System.Action OnJobChangeCompleted;
    public event System.Action OnShopEntered;
    public event System.Action OnPostBattleCompleted;

    #endregion

    #region Initialization

    private void Awake()
    {
        // Auto-find UIManager if not set
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
        }

        // Auto-find JobMenuPanel if not set
        if (jobMenuPanel == null && uiManager != null)
        {
            jobMenuPanel = uiManager.GetPanel<JobMenuPanelController>();
        }
    }

    private void OnEnable()
    {
        // Reset state when enabled
        currentUnitIndex = 0;
        initialized = false;
    }

    /// <summary>
    /// Initialize with battle results data
    /// Called by GameStateManager when entering PostBattle state
    /// </summary>
    public void Initialize(BattleResultsData data)
    {
        resultsData = data;
        initialized = true;

        Debug.Log($"[PostBattleController] Initialized with {data.playerUnits.Length} units");

        // Start post-battle flow
        StartCoroutine(PostBattleFlow());
    }

    #endregion

    #region Post-Battle Flow

    private IEnumerator PostBattleFlow()
    {
        // 1. Wait a moment for battle to clean up
        yield return new WaitForSeconds(resultsDelay);

        // 2. Award EXP and JP to all units
        AwardRewards();

        // 3. Show battle results screen
        ShowBattleResults();
        yield return new WaitUntil(() => !IsResultsScreenShown());

        // 4. Check for level ups
        CheckLevelUps();

        // 5. Check for new job unlocks
        CheckNewJobUnlocks();

        // 6. Auto-show job menu if units have new jobs
        if (autoShowJobMenuOnUnlock && unitsWithNewJobs.Count > 0)
        {
            yield return StartCoroutine(ShowJobMenuFlow());
        }

        // 7. Show post-battle menu (Continue, Job Change, Shop)
        ShowPostBattleMenu();
    }

    #endregion

    #region Rewards

    private void AwardRewards()
    {
        if (resultsData == null || resultsData.playerUnits == null)
            return;

        Debug.Log($"[PostBattleController] Awarding rewards: {resultsData.expGained} EXP, {resultsData.jpGained} JP");

        foreach (var unit in resultsData.playerUnits)
        {
            if (unit == null)
                continue;

            // Award EXP
            var rank = unit.GetComponent<Rank>();
            if (rank != null)
            {
                rank.EXP += resultsData.expGained;
            }

            // Award JP
            var jobManager = unit.GetComponent<JobManager>();
            if (jobManager != null)
            {
                jobManager.AddJobPoints(resultsData.jpGained);
            }
        }

        // Award gold (if you have a currency system)
        // GameData.gold += resultsData.goldGained;

        // Add items to inventory (if you have an inventory system)
        // foreach (var item in resultsData.itemsGained)
        // {
        //     Inventory.AddItem(item);
        // }
    }

    #endregion

    #region Battle Results Screen

    private void ShowBattleResults()
    {
        if (uiManager == null)
            return;

        Debug.Log("[PostBattleController] Showing battle results");

        // Show battle results panel
        uiManager.ShowMenu(MenuType.BattleResults);

        // Populate results panel with data
        // (You'll need to create a BattleResultsPanelController to display this)
        // var resultsPanel = uiManager.GetPanel<BattleResultsPanelController>();
        // if (resultsPanel != null)
        // {
        //     resultsPanel.ShowResults(resultsData);
        // }

        OnResultsShown?.Invoke();
    }

    private bool IsResultsScreenShown()
    {
        return uiManager != null && uiManager.CurrentMenu == MenuType.BattleResults;
    }

    /// <summary>
    /// Called when player closes results screen
    /// (Call this from results panel's "Continue" button)
    /// </summary>
    public void OnResultsClosed()
    {
        if (uiManager != null)
        {
            uiManager.HideMenu();
        }
    }

    #endregion

    #region Level Ups

    private void CheckLevelUps()
    {
        leveledUpUnits.Clear();

        if (resultsData == null || resultsData.playerUnits == null)
            return;

        foreach (var unit in resultsData.playerUnits)
        {
            if (unit == null)
                continue;

            var rank = unit.GetComponent<Rank>();
            if (rank != null && rank.DidLevelUp())
            {
                leveledUpUnits.Add(unit);
                Debug.Log($"[PostBattleController] {unit.name} leveled up!");
            }
        }

        if (leveledUpUnits.Count > 0)
        {
            // Could show level up animations/notifications here
            Debug.Log($"[PostBattleController] {leveledUpUnits.Count} unit(s) leveled up");
        }
    }

    #endregion

    #region Job System

    private void CheckNewJobUnlocks()
    {
        unitsWithNewJobs.Clear();

        if (resultsData == null || resultsData.playerUnits == null)
            return;

        foreach (var unit in resultsData.playerUnits)
        {
            if (unit == null)
                continue;

            var jobManager = unit.GetComponent<JobManager>();
            if (jobManager != null && jobManager.HasUnlockedJobs())
            {
                unitsWithNewJobs.Add(unit);
                Debug.Log($"[PostBattleController] {unit.name} has new jobs available!");
            }
        }

        if (unitsWithNewJobs.Count > 0)
        {
            Debug.Log($"[PostBattleController] {unitsWithNewJobs.Count} unit(s) have new jobs");
        }
    }

    private IEnumerator ShowJobMenuFlow()
    {
        if (jobMenuPanel == null || unitsWithNewJobs.Count == 0)
            yield break;

        Debug.Log("[PostBattleController] Starting job menu flow");

        // Show job menu for each unit with new jobs
        for (int i = 0; i < unitsWithNewJobs.Count; i++)
        {
            currentUnitIndex = i;
            var unit = unitsWithNewJobs[i];
            var jobManager = unit.GetComponent<JobManager>();

            if (jobManager != null)
            {
                // Show job menu
                uiManager?.ShowMenu(MenuType.JobChange);
                jobMenuPanel.Show(jobManager);

                // Wait for player to close job menu
                yield return new WaitUntil(() => !jobMenuPanel.gameObject.activeSelf);
            }
        }

        OnJobChangeCompleted?.Invoke();
        Debug.Log("[PostBattleController] Job menu flow completed");
    }

    /// <summary>
    /// Manually open job menu for a specific unit
    /// Called from post-battle menu
    /// </summary>
    public void OpenJobMenu(Unit unit)
    {
        if (jobMenuPanel == null || unit == null)
            return;

        var jobManager = unit.GetComponent<JobManager>();
        if (jobManager == null)
        {
            Debug.LogWarning($"[PostBattleController] Unit {unit.name} has no JobManager");
            return;
        }

        uiManager?.ShowMenu(MenuType.JobChange);
        jobMenuPanel.Show(jobManager);
    }

    /// <summary>
    /// Open job menu for next unit in list
    /// Called from job menu's "Next" button
    /// </summary>
    public void ShowNextUnitJobMenu()
    {
        if (resultsData == null || resultsData.playerUnits == null)
            return;

        currentUnitIndex++;

        if (currentUnitIndex >= resultsData.playerUnits.Length)
        {
            // No more units, close job menu
            uiManager?.HideMenu();
            return;
        }

        var unit = resultsData.playerUnits[currentUnitIndex];
        OpenJobMenu(unit);
    }

    /// <summary>
    /// Open job menu for previous unit in list
    /// Called from job menu's "Previous" button
    /// </summary>
    public void ShowPreviousUnitJobMenu()
    {
        if (resultsData == null || resultsData.playerUnits == null)
            return;

        currentUnitIndex--;

        if (currentUnitIndex < 0)
        {
            currentUnitIndex = 0;
            return;
        }

        var unit = resultsData.playerUnits[currentUnitIndex];
        OpenJobMenu(unit);
    }

    #endregion

    #region Shop

    /// <summary>
    /// Open shop
    /// Called from post-battle menu's "Shop" button
    /// </summary>
    public void OpenShop()
    {
        if (!allowShopAccess)
        {
            Debug.LogWarning("[PostBattleController] Shop access not allowed");
            return;
        }

        Debug.Log("[PostBattleController] Opening shop");

        OnShopEntered?.Invoke();

        // Transition to shop state
        GameStateManager.Instance?.OpenShop();
    }

    #endregion

    #region Post-Battle Menu

    private void ShowPostBattleMenu()
    {
        Debug.Log("[PostBattleController] Showing post-battle menu");

        // Show a simple menu with options:
        // - Continue
        // - Job Change
        // - Shop (if enabled)
        // - Save Game (optional)

        // For now, we'll just log this
        // You can create a PostBattleMenuPanel prefab later
    }

    /// <summary>
    /// Called when player clicks "Continue" in post-battle menu
    /// </summary>
    public void OnContinue()
    {
        Debug.Log("[PostBattleController] Player continuing from post-battle");

        OnPostBattleCompleted?.Invoke();

        // Notify game state manager that post-battle is complete
        GameStateManager.Instance?.OnPostBattleCompleted();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get current unit being viewed (for UI display)
    /// </summary>
    public Unit GetCurrentUnit()
    {
        if (resultsData == null || resultsData.playerUnits == null)
            return null;

        if (currentUnitIndex >= 0 && currentUnitIndex < resultsData.playerUnits.Length)
        {
            return resultsData.playerUnits[currentUnitIndex];
        }

        return null;
    }

    /// <summary>
    /// Get all player units from battle
    /// </summary>
    public Unit[] GetAllUnits()
    {
        return resultsData?.playerUnits;
    }

    /// <summary>
    /// Get units that leveled up
    /// </summary>
    public List<Unit> GetLeveledUpUnits()
    {
        return leveledUpUnits;
    }

    /// <summary>
    /// Get units with new jobs available
    /// </summary>
    public List<Unit> GetUnitsWithNewJobs()
    {
        return unitsWithNewJobs;
    }

    /// <summary>
    /// Check if initialized
    /// </summary>
    public bool IsInitialized()
    {
        return initialized;
    }

    #endregion
}
