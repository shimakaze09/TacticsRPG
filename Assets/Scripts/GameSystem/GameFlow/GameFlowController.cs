using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global GameFlow Controller - Manages high-level game state transitions for the FFT-style tactics game.
/// 
/// RESPONSIBILITIES:
/// - Manages global game states (Title, World, Battle, PostBattle, Shop)
/// - Owns and coordinates global singleton managers (UIManager, JobManager, DataPersistence)
/// - Provides async scene loading and transitions
/// - Persists across all scenes (DontDestroyOnLoad)
/// - Publishes state change events for UI and gameplay systems
/// 
/// ARCHITECTURE:
/// - Uses the existing StateMachine base class
/// - Each state is a BaseGameFlowState component
/// - BattleController is integrated as a sub-flow within BattleFlowState
/// 
/// USAGE:
/// GameFlowController.Instance.StartBattle(levelData);
/// GameFlowController.Instance.EnterWorld();
/// GameFlowController.Instance.EnterShop();
/// </summary>
public class GameFlowController : StateMachine
{
    #region Singleton

    public static GameFlowController Instance { get; private set; }

    #endregion

    #region Events

    /// <summary>
    /// Fired when the game flow state changes
    /// </summary>
    public event Action<GameFlowState, GameFlowState> OnFlowStateChanged;

    /// <summary>
    /// Fired when a battle ends (from BattleController)
    /// </summary>
    public event Action OnBattleEnded;

    /// <summary>
    /// Fired when game is saved
    /// </summary>
    public event Action OnGameSaved;

    /// <summary>
    /// Fired when game is loaded
    /// </summary>
    public event Action OnGameLoaded;

    #endregion

    #region State

    /// <summary>
    /// Current game flow state
    /// </summary>
    public GameFlowState CurrentFlowState { get; private set; } = GameFlowState.None;

    /// <summary>
    /// Previous game flow state
    /// </summary>
    public GameFlowState PreviousFlowState { get; private set; } = GameFlowState.None;

    /// <summary>
    /// Level data for the next battle to start
    /// </summary>
    public LevelData PendingBattleLevel { get; set; }

    /// <summary>
    /// Whether we're currently in a transition
    /// </summary>
    public bool IsTransitioning => _inTransition;

    #endregion

    #region Manager References

    /// <summary>
    /// Global UI Manager reference
    /// </summary>
    public UIManager UIManager => UIManager.Instance;

    /// <summary>
    /// Global Data Persistence Manager reference
    /// </summary>
    public DataPersistenceManager DataManager => DataPersistenceManager.Instance;

    #endregion

    #region Loading Screen

    [Header("Loading Screen")]
    [Tooltip("Loading screen UI (optional)")]
    public GameObject loadingScreen;

    [Tooltip("Loading progress bar (optional)")]
    public UnityEngine.UI.Slider loadingProgressBar;

    #endregion

    #region Event Listeners

    /// <summary>
    /// Registered event listeners
    /// </summary>
    private List<IGameFlowEventListener> eventListeners = new List<IGameFlowEventListener>();

    #endregion

    #region MonoBehaviour Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameFlow] Duplicate GameFlowController found, destroying...");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GameFlow] GameFlowController initialized");
    }

    private void Start()
    {
        // Initialize all flow states
        InitializeStates();

        // Start at Title screen
        TransitionToState(GameFlowState.Title);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize all game flow states
    /// </summary>
    private void InitializeStates()
    {
        Debug.Log("[GameFlow] Initializing game flow states...");

        // Get or add all state components
        var titleState = GetState<TitleState>();
        var worldState = GetState<WorldState>();
        var battleFlowState = GetState<BattleFlowState>();
        var postBattleState = GetState<PostBattleState>();
        var shopState = GetState<ShopState>();

        // Initialize each state with controller reference
        titleState.Initialize(this);
        worldState.Initialize(this);
        battleFlowState.Initialize(this);
        postBattleState.Initialize(this);
        shopState.Initialize(this);

        Debug.Log("[GameFlow] All states initialized");
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Transition to a specific game flow state
    /// </summary>
    public void TransitionToState(GameFlowState targetState)
    {
        BaseGameFlowState newState = targetState switch
        {
            GameFlowState.Title => GetState<TitleState>(),
            GameFlowState.World => GetState<WorldState>(),
            GameFlowState.Battle => GetState<BattleFlowState>(),
            GameFlowState.PostBattle => GetState<PostBattleState>(),
            GameFlowState.Shop => GetState<ShopState>(),
            _ => null
        };

        if (newState != null)
        {
            PreviousFlowState = CurrentFlowState;
            CurrentFlowState = targetState;

            // Notify listeners BEFORE transition
            NotifyStateChanging(PreviousFlowState, CurrentFlowState);

            // Change the state
            CurrentState = newState;

            // Notify listeners AFTER transition
            OnFlowStateChanged?.Invoke(PreviousFlowState, CurrentFlowState);
            NotifyStateChanged(CurrentFlowState);
        }
        else
        {
            Debug.LogError($"[GameFlow] Cannot transition to state: {targetState}");
        }
    }

    #endregion

    #region Public API - State Transitions

    /// <summary>
    /// Enter the world/overworld state
    /// </summary>
    public void EnterWorld()
    {
        Debug.Log("[GameFlow] Entering World state");
        TransitionToState(GameFlowState.World);
    }

    /// <summary>
    /// Start a battle with the given level data
    /// </summary>
    public void StartBattle(LevelData level)
    {
        if (level == null)
        {
            Debug.LogError("[GameFlow] Cannot start battle: LevelData is null");
            return;
        }

        Debug.Log($"[GameFlow] Starting battle with level: {level.name}");
        PendingBattleLevel = level;
        TransitionToState(GameFlowState.Battle);
    }

    /// <summary>
    /// Return to post-battle state (called when battle ends)
    /// </summary>
    public void ReturnToPostBattle()
    {
        Debug.Log("[GameFlow] Returning to PostBattle state");
        TransitionToState(GameFlowState.PostBattle);
    }

    /// <summary>
    /// Enter the shop state
    /// </summary>
    public void EnterShop()
    {
        Debug.Log("[GameFlow] Entering Shop state");
        TransitionToState(GameFlowState.Shop);
    }

    /// <summary>
    /// Return to title screen
    /// </summary>
    public void ReturnToTitle()
    {
        Debug.Log("[GameFlow] Returning to Title state");
        TransitionToState(GameFlowState.Title);
    }

    #endregion

    #region Public API - Data Management

    /// <summary>
    /// Save the game
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("[GameFlow] Saving game...");

        if (DataManager != null)
        {
            DataManager.SaveGame();
            OnGameSaved?.Invoke();
            Debug.Log("[GameFlow] Game saved successfully");
        }
        else
        {
            Debug.LogWarning("[GameFlow] Cannot save: DataPersistenceManager not found");
        }
    }

    /// <summary>
    /// Load the game
    /// </summary>
    public void LoadGame()
    {
        Debug.Log("[GameFlow] Loading game...");

        if (DataManager != null)
        {
            DataManager.LoadGame();
            OnGameLoaded?.Invoke();
            Debug.Log("[GameFlow] Game loaded successfully");
        }
        else
        {
            Debug.LogWarning("[GameFlow] Cannot load: DataPersistenceManager not found");
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void NewGame()
    {
        Debug.Log("[GameFlow] Starting new game...");

        if (DataManager != null)
        {
            DataManager.NewGame();
            Debug.Log("[GameFlow] New game initialized");
        }
        else
        {
            Debug.LogWarning("[GameFlow] Cannot start new game: DataPersistenceManager not found");
        }

        // Transition to world
        EnterWorld();
    }

    #endregion

    #region Loading Screen

    /// <summary>
    /// Show or hide the loading screen
    /// </summary>
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);
        }
    }

    /// <summary>
    /// Update loading progress (0-1)
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = progress;
        }
    }

    #endregion

    #region Event Listener Management

    /// <summary>
    /// Register an event listener
    /// </summary>
    public void RegisterListener(IGameFlowEventListener listener)
    {
        if (listener != null && !eventListeners.Contains(listener))
        {
            eventListeners.Add(listener);
        }
    }

    /// <summary>
    /// Unregister an event listener
    /// </summary>
    public void UnregisterListener(IGameFlowEventListener listener)
    {
        if (listener != null && eventListeners.Contains(listener))
        {
            eventListeners.Remove(listener);
        }
    }

    /// <summary>
    /// Notify all listeners that state is changing
    /// </summary>
    private void NotifyStateChanging(GameFlowState fromState, GameFlowState toState)
    {
        foreach (var listener in eventListeners)
        {
            try
            {
                listener.OnFlowStateChanging(fromState, toState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameFlow] Error notifying listener: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Notify all listeners that state has changed
    /// </summary>
    private void NotifyStateChanged(GameFlowState newState)
    {
        foreach (var listener in eventListeners)
        {
            try
            {
                listener.OnFlowStateChanged(newState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameFlow] Error notifying listener: {ex.Message}");
            }
        }
    }

    #endregion

    #region Battle Integration

    /// <summary>
    /// Called by BattleController when battle ends
    /// </summary>
    public void NotifyBattleEnded()
    {
        Debug.Log("[GameFlow] Battle ended notification received");
        OnBattleEnded?.Invoke();
        ReturnToPostBattle();
    }

    #endregion
}
