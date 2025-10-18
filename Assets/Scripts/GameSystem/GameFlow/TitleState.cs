using UnityEngine;

/// <summary>
/// Title Screen State - Handles the main menu and game initialization.
/// 
/// RESPONSIBILITIES:
/// - Show title screen UI (main menu)
/// - Handle "New Game" button (starts new game and transitions to World)
/// - Handle "Load Game" button (loads save data and transitions to World)
/// - Handle "Options" button (opens settings menu)
/// - Handle "Quit" button (exits application)
/// 
/// SCENE:
/// - Can work with or without a dedicated Title scene
/// - If SceneName is set, will load that scene
/// - Otherwise, operates in the current scene
/// </summary>
public class TitleState : BaseGameFlowState
{
    #region Properties

    public override GameFlowState StateType => GameFlowState.Title;

    // Optional: Set this if you have a dedicated Title scene
    // protected override string SceneName => "TitleScene";

    #endregion

    #region State Lifecycle

    protected override void OnSceneReady()
    {
        Debug.Log("[TitleState] Title screen ready");

        // Show title UI
        ShowTitleUI();

        // Subscribe to UI events
        SubscribeToTitleUIEvents();
    }

    protected override void OnStateExit()
    {
        Debug.Log("[TitleState] Exiting title screen");

        // Unsubscribe from UI events
        UnsubscribeFromTitleUIEvents();

        // Hide title UI
        HideTitleUI();
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Show the title screen UI
    /// </summary>
    private void ShowTitleUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Show title menu panel
            // Controller.UIManager.ShowPanel<TitleMenuPanel>();
            Debug.Log("[TitleState] Title UI shown");
        }
        else
        {
            Debug.LogWarning("[TitleState] UIManager not found");
        }
    }

    /// <summary>
    /// Hide the title screen UI
    /// </summary>
    private void HideTitleUI()
    {
        if (Controller.UIManager != null)
        {
            // TODO: Hide title menu panel
            // Controller.UIManager.HidePanel<TitleMenuPanel>();
            Debug.Log("[TitleState] Title UI hidden");
        }
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Subscribe to title UI button events
    /// </summary>
    private void SubscribeToTitleUIEvents()
    {
        // TODO: Subscribe to title menu button events
        // Example:
        // titleMenuPanel.OnNewGameClicked += HandleNewGame;
        // titleMenuPanel.OnLoadGameClicked += HandleLoadGame;
        // titleMenuPanel.OnOptionsClicked += HandleOptions;
        // titleMenuPanel.OnQuitClicked += HandleQuit;
    }

    /// <summary>
    /// Unsubscribe from title UI button events
    /// </summary>
    private void UnsubscribeFromTitleUIEvents()
    {
        // TODO: Unsubscribe from title menu button events
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// Handle "New Game" button click
    /// </summary>
    public void HandleNewGame()
    {
        Debug.Log("[TitleState] New Game clicked");
        Controller.NewGame();
    }

    /// <summary>
    /// Handle "Load Game" button click
    /// </summary>
    public void HandleLoadGame()
    {
        Debug.Log("[TitleState] Load Game clicked");
        Controller.LoadGame();
        Controller.EnterWorld();
    }

    /// <summary>
    /// Handle "Options" button click
    /// </summary>
    public void HandleOptions()
    {
        Debug.Log("[TitleState] Options clicked");
        // TODO: Show options/settings panel
        // Controller.UIManager.ShowPanel<SettingsPanel>();
    }

    /// <summary>
    /// Handle "Quit" button click
    /// </summary>
    public void HandleQuit()
    {
        Debug.Log("[TitleState] Quit clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Debug/Testing

    /// <summary>
    /// For testing: Immediately start a new game
    /// </summary>
    [ContextMenu("Debug: Start New Game")]
    private void DebugStartNewGame()
    {
        HandleNewGame();
    }

    #endregion
}
