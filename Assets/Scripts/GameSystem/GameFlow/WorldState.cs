using UnityEngine;

/// <summary>
/// World/Overworld State - Represents the player's time outside of battles.
/// 
/// RESPONSIBILITIES:
/// - Display world map or overworld UI
/// - Allow player to:
///   - Navigate the world
///   - Open party menu (job changes, equipment, etc.)
///   - Start battles (select mission/location)
///   - Enter shops
///   - Save/load game
/// - Manage world exploration and events
/// 
/// SCENE:
/// - Can work with or without a dedicated World scene
/// - Typically the main hub between battles
/// </summary>
public class WorldState : BaseGameFlowState
{
    #region Properties

    public override GameFlowState StateType => GameFlowState.World;

    // Optional: Set this if you have a dedicated World scene
    // protected override string SceneName => "WorldScene";

    #endregion

    #region State Lifecycle

    protected override void OnSceneReady()
    {
        Debug.Log("[WorldState] World scene ready");

        // Show world UI
        ShowWorldUI();

        // Subscribe to input and UI events
        SubscribeToEvents();

        // Auto-save on entering world (optional)
        AutoSaveIfNeeded();
    }

    protected override void OnStateExit()
    {
        Debug.Log("[WorldState] Exiting world state");

        // Unsubscribe from events
        UnsubscribeFromEvents();

        // Hide world UI
        HideWorldUI();
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Show the world/overworld UI
    /// </summary>
    private void ShowWorldUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Show world menu/HUD
            // Controller.UIManager.ShowPanel<WorldMenuPanel>();
            // Controller.UIManager.SetContext(UIContext.World);
            Debug.Log("[WorldState] World UI shown");
        }
        else
        {
            Debug.LogWarning("[WorldState] UIManager not found");
        }
    }

    /// <summary>
    /// Hide the world UI
    /// </summary>
    private void HideWorldUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Hide world menu/HUD
            // Controller.UIManager.HidePanel<WorldMenuPanel>();
            Debug.Log("[WorldState] World UI hidden");
        }
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Subscribe to world input and UI events
    /// </summary>
    private void SubscribeToEvents()
    {
        // TODO: Subscribe to world menu events
        // Example:
        // worldMenuPanel.OnPartyMenuClicked += OpenPartyMenu;
        // worldMenuPanel.OnStartBattleClicked += OpenBattleSelection;
        // worldMenuPanel.OnShopClicked += EnterShop;
        // worldMenuPanel.OnSaveGameClicked += SaveGame;
    }

    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // TODO: Unsubscribe from world menu events
    }

    #endregion

    #region Menu Handlers

    /// <summary>
    /// Open the party menu (job changes, equipment, etc.)
    /// </summary>
    public void OpenPartyMenu()
    {
        Debug.Log("[WorldState] Opening party menu");

        if (Controller.UIManager != null)
        {
            // TODO: Show party/job menu panel
            // Controller.UIManager.ShowPanel<PartyMenuPanel>();
            // Controller.UIManager.ShowMenu(MenuType.JobMenu);
        }
    }

    /// <summary>
    /// Open battle selection UI
    /// </summary>
    public void OpenBattleSelection()
    {
        Debug.Log("[WorldState] Opening battle selection");

        // TODO: Show battle selection panel
        // This would show available missions/battles
        // When user selects one, call StartBattle(levelData)
    }

    /// <summary>
    /// Start a battle with the given level data
    /// </summary>
    public void StartBattle(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("[WorldState] Cannot start battle: LevelData is null");
            return;
        }

        Debug.Log($"[WorldState] Starting battle: {levelData.name}");
        Controller.StartBattle(levelData);
    }

    /// <summary>
    /// Enter the shop
    /// </summary>
    public void EnterShop()
    {
        Debug.Log("[WorldState] Entering shop");
        Controller.EnterShop();
    }

    /// <summary>
    /// Save the game
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("[WorldState] Saving game");
        Controller.SaveGame();

        // TODO: Show save confirmation UI
        // Controller.UIManager.ShowMessage("Game saved!");
    }

    /// <summary>
    /// Return to title screen
    /// </summary>
    public void ReturnToTitle()
    {
        Debug.Log("[WorldState] Returning to title");

        // TODO: Show confirmation dialog
        // "Are you sure? Unsaved progress will be lost."
        // If confirmed:
        Controller.ReturnToTitle();
    }

    #endregion

    #region Auto-Save

    /// <summary>
    /// Auto-save when entering world (if enabled)
    /// </summary>
    private void AutoSaveIfNeeded()
    {
        // TODO: Check if auto-save is enabled in settings
        bool autoSaveEnabled = true; // Placeholder

        if (autoSaveEnabled)
        {
            Debug.Log("[WorldState] Auto-saving...");
            Controller.SaveGame();
        }
    }

    #endregion

    #region Debug/Testing

    /// <summary>
    /// For testing: Start a test battle
    /// </summary>
    [ContextMenu("Debug: Start Test Battle")]
    private void DebugStartTestBattle()
    {
        // TODO: Load a test level data
        // LevelData testLevel = Resources.Load<LevelData>("Levels/TestBattle");
        // StartBattle(testLevel);
        Debug.Log("[WorldState] Test battle would start here (need LevelData)");
    }

    #endregion
}
