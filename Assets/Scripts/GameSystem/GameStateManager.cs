using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Top-level game state manager
/// Coordinates state transitions between Battle, PostBattle, Shop, etc.
/// Singleton pattern for global access
/// </summary>
public class GameStateManager : MonoBehaviour
{
    #region Singleton

    public static GameStateManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region State

    /// <summary>
    /// Current game state
    /// </summary>
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    /// <summary>
    /// Previous game state (for returning)
    /// </summary>
    public GameState PreviousState { get; private set; } = GameState.MainMenu;

    /// <summary>
    /// State data passed during transitions
    /// </summary>
    private object currentStateData;

    #endregion

    #region Controller References

    [Header("Controllers")]
    [Tooltip("Battle controller (activated during battles)")]
    public BattleController battleController;

    [Tooltip("Post-battle controller (activated after battles)")]
    public PostBattleController postBattleController;

    [Tooltip("Shop controller (activated during shopping)")]
    public GameObject shopController; // Will be created later

    [Tooltip("Conversation controller (for cutscenes)")]
    public ConversationController conversationController;

    [Header("Managers")]
    [Tooltip("UI Manager reference")]
    public UIManager uiManager;

    [Tooltip("Data persistence manager")]
    public DataPersistenceManager persistenceManager;

    #endregion

    #region Events

    /// <summary>
    /// Fired when game state changes
    /// </summary>
    public event Action<GameState, GameState> OnStateChanged;

    /// <summary>
    /// Fired before state transition (can be used to cleanup)
    /// </summary>
    public event Action<GameState> OnStateExit;

    /// <summary>
    /// Fired after state transition (can be used to initialize)
    /// </summary>
    public event Action<GameState> OnStateEnter;

    #endregion

    #region Initialization

    private void Start()
    {
        // Auto-find managers if not assigned
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
        }

        if (persistenceManager == null)
        {
            persistenceManager = FindObjectOfType<DataPersistenceManager>();
        }

        // Subscribe to battle events
        if (battleController != null)
        {
            // We'll subscribe to battle end events here later
        }
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Change to a new game state
    /// </summary>
    public void ChangeState(GameState newState, object data = null)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning($"[GameStateManager] Already in state {newState}");
            return;
        }

        Debug.Log($"[GameStateManager] State transition: {CurrentState} → {newState}");

        // Store previous state
        PreviousState = CurrentState;

        // Exit current state
        OnStateExit?.Invoke(CurrentState);
        ExitState(CurrentState);

        // Update state
        CurrentState = newState;
        currentStateData = data;

        // Enter new state
        EnterState(newState, data);
        OnStateEnter?.Invoke(newState);

        // Notify listeners
        OnStateChanged?.Invoke(PreviousState, CurrentState);
    }

    /// <summary>
    /// Return to previous state
    /// </summary>
    public void ReturnToPreviousState()
    {
        ChangeState(PreviousState);
    }

    #endregion

    #region State Enter/Exit

    private void EnterState(GameState state, object data)
    {
        switch (state)
        {
            case GameState.Battle:
                EnterBattleState(data);
                break;

            case GameState.PostBattle:
                EnterPostBattleState(data);
                break;

            case GameState.Shop:
                EnterShopState(data);
                break;

            case GameState.Cutscene:
                EnterCutsceneState(data);
                break;

            case GameState.MainMenu:
                EnterMainMenuState();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Battle:
                ExitBattleState();
                break;

            case GameState.PostBattle:
                ExitPostBattleState();
                break;

            case GameState.Shop:
                ExitShopState();
                break;

            case GameState.Cutscene:
                ExitCutsceneState();
                break;
        }
    }

    #endregion

    #region Battle State

    private void EnterBattleState(object data)
    {
        // Activate battle controller
        if (battleController != null)
        {
            battleController.enabled = true;
        }

        // Show battle UI
        if (uiManager != null)
        {
            uiManager.SetContext(UIContext.Battle);
        }

        Debug.Log("[GameStateManager] Entered Battle state");
    }

    private void ExitBattleState()
    {
        // Hide battle UI
        if (uiManager != null)
        {
            uiManager.HideBattleUI();
        }

        Debug.Log("[GameStateManager] Exited Battle state");
    }

    /// <summary>
    /// Called by BattleController when battle ends
    /// </summary>
    public void OnBattleEnded(bool victory, BattleResultsData resultsData)
    {
        Debug.Log($"[GameStateManager] Battle ended. Victory: {victory}");

        if (victory)
        {
            // Transition to post-battle state
            ChangeState(GameState.PostBattle, resultsData);
        }
        else
        {
            // Handle defeat (game over or retry)
            HandleBattleDefeat();
        }
    }

    private void HandleBattleDefeat()
    {
        // For now, return to main menu
        // Later: could show game over screen, allow retry, etc.
        Debug.Log("[GameStateManager] Battle defeat - returning to main menu");
        ChangeState(GameState.MainMenu);
    }

    #endregion

    #region Post-Battle State

    private void EnterPostBattleState(object data)
    {
        // Activate post-battle controller
        if (postBattleController != null)
        {
            postBattleController.enabled = true;

            // Pass battle results data
            if (data is BattleResultsData resultsData)
            {
                postBattleController.Initialize(resultsData);
            }
        }

        // Show post-battle UI
        if (uiManager != null)
        {
            uiManager.SetContext(UIContext.PostBattle);
        }

        Debug.Log("[GameStateManager] Entered PostBattle state");
    }

    private void ExitPostBattleState()
    {
        // Deactivate post-battle controller
        if (postBattleController != null)
        {
            postBattleController.enabled = false;
        }

        // Hide post-battle UI
        if (uiManager != null)
        {
            uiManager.HidePostBattleUI();
        }

        Debug.Log("[GameStateManager] Exited PostBattle state");
    }

    /// <summary>
    /// Called by PostBattleController when player is done with post-battle activities
    /// </summary>
    public void OnPostBattleCompleted()
    {
        Debug.Log("[GameStateManager] Post-battle completed");

        // Return to main menu (or world map when implemented)
        ChangeState(GameState.MainMenu);
    }

    #endregion

    #region Shop State

    private void EnterShopState(object data)
    {
        // Activate shop controller
        if (shopController != null)
        {
            shopController.SetActive(true);
        }

        // Show shop UI
        if (uiManager != null)
        {
            uiManager.SetContext(UIContext.Shop);
        }

        Debug.Log("[GameStateManager] Entered Shop state");
    }

    private void ExitShopState()
    {
        // Deactivate shop controller
        if (shopController != null)
        {
            shopController.SetActive(false);
        }

        // Hide shop UI
        if (uiManager != null)
        {
            uiManager.HideShopUI();
        }

        Debug.Log("[GameStateManager] Exited Shop state");
    }

    /// <summary>
    /// Called when player exits shop
    /// </summary>
    public void OnShopExited()
    {
        // Return to post-battle state (shop only accessible after battles)
        ChangeState(GameState.PostBattle);
    }

    #endregion

    #region Cutscene State

    private void EnterCutsceneState(object data)
    {
        // Activate conversation controller
        if (conversationController != null)
        {
            conversationController.enabled = true;
        }

        // Show global UI (dialog)
        if (uiManager != null)
        {
            uiManager.SetContext(UIContext.Global);
        }

        Debug.Log("[GameStateManager] Entered Cutscene state");
    }

    private void ExitCutsceneState()
    {
        // Deactivate conversation controller
        if (conversationController != null)
        {
            conversationController.enabled = false;
        }

        Debug.Log("[GameStateManager] Exited Cutscene state");
    }

    #endregion

    #region Main Menu State

    private void EnterMainMenuState()
    {
        // Hide all UI
        if (uiManager != null)
        {
            uiManager.HideAllUI();
        }

        Debug.Log("[GameStateManager] Entered MainMenu state");

        // Could load main menu scene here
        // SceneManager.LoadScene("MainMenu");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Start a new battle
    /// </summary>
    public void StartBattle(object battleData = null)
    {
        ChangeState(GameState.Battle, battleData);
    }

    /// <summary>
    /// Open shop from post-battle
    /// </summary>
    public void OpenShop()
    {
        if (CurrentState != GameState.PostBattle)
        {
            Debug.LogWarning("[GameStateManager] Shop can only be opened from PostBattle state");
            return;
        }

        ChangeState(GameState.Shop);
    }

    /// <summary>
    /// Check if state allows certain actions
    /// </summary>
    public bool CanOpenMenu()
    {
        return CurrentState == GameState.PostBattle || CurrentState == GameState.World;
    }

    /// <summary>
    /// Check if currently in battle
    /// </summary>
    public bool IsInBattle()
    {
        return CurrentState == GameState.Battle;
    }

    /// <summary>
    /// Check if in post-battle phase
    /// </summary>
    public bool IsPostBattle()
    {
        return CurrentState == GameState.PostBattle;
    }

    #endregion

    #region Debug

    /// <summary>
    /// Get debug information about current state
    /// </summary>
    public string GetDebugInfo()
    {
        return $"State: {CurrentState}, Previous: {PreviousState}, Data: {currentStateData?.GetType().Name ?? "None"}";
    }

    #endregion
}

/// <summary>
/// Data passed from Battle to PostBattle state
/// </summary>
[Serializable]
public class BattleResultsData
{
    public bool victory;
    public int expGained;
    public int jpGained;
    public int goldGained;
    public string[] itemsGained;
    public Unit[] playerUnits; // For level up checks and job changes
}
