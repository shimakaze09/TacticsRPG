using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Post-Battle State - Handles battle results, rewards, and character progression.
/// 
/// RESPONSIBILITIES:
/// - Display battle results (victory/defeat)
/// - Award experience points (EXP) to units
/// - Award job points (JP) to units
/// - Process level-ups and job level-ups
/// - Distribute item/equipment rewards
/// - Show battle statistics
/// - Transition back to World state
/// 
/// INTEGRATION:
/// - Works with ExperienceManager for EXP distribution
/// - Works with JobManager for JP distribution
/// - Uses UIManager to show results panel
/// </summary>
public class PostBattleState : BaseGameFlowState
{
    #region Properties

    public override GameFlowState StateType => GameFlowState.PostBattle;

    // Post-battle typically doesn't need a separate scene
    // It can overlay on top of the battle scene or world scene
    protected override string SceneName => null;

    /// <summary>
    /// Results from the completed battle
    /// </summary>
    private BattleResults battleResults;

    #endregion

    #region State Lifecycle

    protected override void OnSceneReady()
    {
        Debug.Log("[PostBattleState] Post-battle state ready");

        // Gather battle results
        GatherBattleResults();

        // Show results UI
        ShowResultsUI();

        // Process rewards
        ProcessRewards();
    }

    protected override void OnStateExit()
    {
        Debug.Log("[PostBattleState] Exiting post-battle state");

        // Hide results UI
        HideResultsUI();

        // Clear battle results
        battleResults = null;
    }

    #endregion

    #region Battle Results

    /// <summary>
    /// Gather results from the completed battle
    /// </summary>
    private void GatherBattleResults()
    {
        Debug.Log("[PostBattleState] Gathering battle results");

        // TODO: Get results from BattleController or a results manager
        // For now, create a placeholder
        battleResults = new BattleResults
        {
            victory = true, // Placeholder
            expGained = 500, // Placeholder
            jpGained = 100, // Placeholder
            goldGained = 1000, // Placeholder
            itemsGained = new List<string>() // Placeholder
        };

        Debug.Log($"[PostBattleState] Results: Victory={battleResults.victory}, EXP={battleResults.expGained}, JP={battleResults.jpGained}");
    }

    /// <summary>
    /// Process rewards (EXP, JP, items, gold)
    /// </summary>
    private void ProcessRewards()
    {
        if (battleResults == null)
        {
            Debug.LogWarning("[PostBattleState] No battle results to process!");
            return;
        }

        Debug.Log("[PostBattleState] Processing rewards...");

        // Award EXP to surviving units
        AwardExperience();

        // Award JP to surviving units
        AwardJobPoints();

        // Add items to inventory
        AwardItems();

        // Add gold to player
        AwardGold();

        Debug.Log("[PostBattleState] Rewards processed");
    }

    /// <summary>
    /// Award experience points to units
    /// </summary>
    private void AwardExperience()
    {
        if (battleResults.expGained <= 0)
            return;

        Debug.Log($"[PostBattleState] Awarding {battleResults.expGained} EXP to units");

        // TODO: Find all player units that participated in battle
        // For each unit:
        //   - unit.GetComponent<Rank>().AddExperience(battleResults.expGained);
        //   - Check for level-ups and show level-up UI
    }

    /// <summary>
    /// Award job points to units
    /// </summary>
    private void AwardJobPoints()
    {
        if (battleResults.jpGained <= 0)
            return;

        Debug.Log($"[PostBattleState] Awarding {battleResults.jpGained} JP to units");

        // TODO: Find all player units that participated in battle
        // For each unit:
        //   - unit.GetComponent<JobManager>()?.AddJobPoints(battleResults.jpGained);
        //   - Check for job level-ups and show notifications
    }

    /// <summary>
    /// Award items to the player's inventory
    /// </summary>
    private void AwardItems()
    {
        if (battleResults.itemsGained == null || battleResults.itemsGained.Count == 0)
            return;

        Debug.Log($"[PostBattleState] Awarding {battleResults.itemsGained.Count} items");

        // TODO: Add items to inventory system
        foreach (var item in battleResults.itemsGained)
        {
            Debug.Log($"  - Received: {item}");
        }
    }

    /// <summary>
    /// Award gold to the player
    /// </summary>
    private void AwardGold()
    {
        if (battleResults.goldGained <= 0)
            return;

        Debug.Log($"[PostBattleState] Awarding {battleResults.goldGained} gold");

        // TODO: Add gold to player's currency
        // PlayerData.Instance.AddGold(battleResults.goldGained);
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Show the battle results UI
    /// </summary>
    private void ShowResultsUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Show battle results panel
            // Controller.UIManager.ShowPanel<BattleResultsPanel>();
            // resultsPanel.DisplayResults(battleResults);
            Debug.Log("[PostBattleState] Results UI shown");
        }
        else
        {
            Debug.LogWarning("[PostBattleState] UIManager not found");
        }
    }

    /// <summary>
    /// Hide the battle results UI
    /// </summary>
    private void HideResultsUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Hide battle results panel
            // Controller.UIManager.HidePanel<BattleResultsPanel>();
            Debug.Log("[PostBattleState] Results UI hidden");
        }
    }

    #endregion

    #region User Actions

    /// <summary>
    /// Continue to the next screen (called by UI button)
    /// </summary>
    public void ContinueToWorld()
    {
        Debug.Log("[PostBattleState] Continuing to world");

        // Save game before returning to world
        Controller.SaveGame();

        // Return to world
        Controller.EnterWorld();
    }

    /// <summary>
    /// Retry the battle (if defeat)
    /// </summary>
    public void RetryBattle()
    {
        Debug.Log("[PostBattleState] Retrying battle");

        // TODO: Reload the same battle
        // Controller.StartBattle(previousLevelData);
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Data structure for battle results
    /// </summary>
    private class BattleResults
    {
        public bool victory;
        public int expGained;
        public int jpGained;
        public int goldGained;
        public List<string> itemsGained;
    }

    #endregion

    #region Debug/Testing

    /// <summary>
    /// For testing: Skip to world
    /// </summary>
    [ContextMenu("Debug: Skip To World")]
    private void DebugSkipToWorld()
    {
        ContinueToWorld();
    }

    #endregion
}
