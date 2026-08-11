using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Battle Flow State - Manages the entire battle lifecycle as a sub-flow.
/// 
/// RESPONSIBILITIES:
/// - Load the battle scene asynchronously
/// - Wait for BattleController to initialize
/// - Listen for battle end events
/// - Transition to PostBattle state when battle completes
/// - Clean up battle scene on exit
/// 
/// INTEGRATION:
/// - BattleController remains autonomous with its own state machine
/// - This state acts as a wrapper/coordinator
/// - BattleController calls GameFlowController.NotifyBattleEnded() when done
/// 
/// SCENE:
/// - Requires a Battle scene to be loaded
/// </summary>
public class BattleFlowState : BaseGameFlowState
{
    #region Properties

    public override GameFlowState StateType => GameFlowState.Battle;

    // Battle scene name - customize based on your project
    protected override string SceneName => "Battle"; // Change if your battle scene has a different name

    /// <summary>
    /// Reference to the active BattleController in the scene
    /// </summary>
    private BattleController battleController;

    #endregion

    #region State Lifecycle

    protected override void OnSceneReady()
    {
        Debug.Log("[BattleFlowState] Battle scene loaded");

        // Find the BattleController in the loaded scene
        battleController = FindAnyObjectByType<BattleController>();

        if (battleController == null)
        {
            Debug.LogError("[BattleFlowState] BattleController not found in battle scene!");
            // Fallback: return to world
            Controller.EnterWorld();
            return;
        }

        // Pending battle payloads (PendingBattle / PendingBattleLevel) are
        // PULLED by InitBattleState before it builds anything — pushing them
        // here raced BattleController.Start's own init (issue #18)

        // Subscribe to battle end events
        SubscribeToBattleEvents();

        // Show battle UI
        ShowBattleUI();

        // BattleController will handle its own state machine from here
        // It starts with InitBattleState in its Start() method
        Debug.Log("[BattleFlowState] Battle initialized, waiting for completion...");
    }

    protected override void OnStateExit()
    {
        Debug.Log("[BattleFlowState] Exiting battle state");

        // Unsubscribe from battle events
        UnsubscribeFromBattleEvents();

        // Hide battle UI
        HideBattleUI();

        // Clear the battle controller reference
        battleController = null;

        // Both pending payloads are one-battle data: consumed by this
        // battle's init, cleared here so a later battle can never replay them
        Controller.PendingBattleLevel = null;
        Controller.PendingBattle = null;
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Show battle-specific UI
    /// </summary>
    private void ShowBattleUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Set UI context to Battle
            // Controller.UIManager.SetContext(UIContext.Battle);
            // Controller.UIManager.ShowBattleUI();
            Debug.Log("[BattleFlowState] Battle UI shown");
        }
    }

    /// <summary>
    /// Hide battle UI
    /// </summary>
    private void HideBattleUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Hide battle UI
            // Controller.UIManager.HideBattleUI();
            Debug.Log("[BattleFlowState] Battle UI hidden");
        }
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Subscribe to battle events
    /// </summary>
    private void SubscribeToBattleEvents()
    {
        if (Controller != null)
        {
            Controller.OnBattleEnded += HandleBattleEnded;
        }
    }

    /// <summary>
    /// Unsubscribe from battle events
    /// </summary>
    private void UnsubscribeFromBattleEvents()
    {
        if (Controller != null)
        {
            Controller.OnBattleEnded -= HandleBattleEnded;
        }
    }

    /// <summary>
    /// Handle battle ended event
    /// </summary>
    private void HandleBattleEnded()
    {
        Debug.Log("[BattleFlowState] Battle ended, transitioning to PostBattle state");

        // Note: GameFlowController.NotifyBattleEnded() already transitions to PostBattle
        // This is just for additional logging or cleanup if needed
    }

    #endregion

    #region Battle Control (Optional)

    /// <summary>
    /// Pause the battle (optional feature)
    /// </summary>
    public void PauseBattle()
    {
        Debug.Log("[BattleFlowState] Pausing battle");
        Time.timeScale = 0f;

        // TODO: Show pause menu
        // Controller.UIManager.ShowPanel<PauseMenuPanel>();
    }

    /// <summary>
    /// Resume the battle (optional feature)
    /// </summary>
    public void ResumeBattle()
    {
        Debug.Log("[BattleFlowState] Resuming battle");
        // Resume to the pacing THIS battle started with — a preference
        // changed mid-battle waits for the next one (issue #62)
        Time.timeScale = (BattleSessionGuard.ActiveBattleSpeedPercent ?? GameSettings.BattleSpeedPercent) / 100f;

        // TODO: Hide pause menu
        // Controller.UIManager.HidePanel<PauseMenuPanel>();
    }

    /// <summary>
    /// Forfeit/retreat from battle (optional feature)
    /// </summary>
    public void ForfeitBattle()
    {
        Debug.Log("[BattleFlowState] Forfeiting battle");

        // TODO: Show confirmation dialog
        // If confirmed:
        // - Mark battle as lost/forfeited
        // - Return to world without rewards
        Controller.EnterWorld();
    }

    #endregion

    #region Debug/Testing

    /// <summary>
    /// For testing: Force end the battle
    /// </summary>
    [ContextMenu("Debug: Force End Battle")]
    private void DebugForceEndBattle()
    {
        Debug.Log("[BattleFlowState] DEBUG: Force ending battle");
        Controller.NotifyBattleEnded(new BattleResultsData
        {
            victory = true,
            itemsGained = new string[0],
            playerUnits = new Unit[0]
        });
    }

    #endregion
}
