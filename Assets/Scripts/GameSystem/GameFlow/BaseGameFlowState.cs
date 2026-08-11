using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base class for all game flow states: scene loading, async operations, and
/// GameFlowController access, integrated with the State machine pattern.
/// Scene ownership policy (issue #18): flow scenes load in **single mode
/// only** — a state with a SceneName owns the active scene outright and
/// replaces whatever was loaded; additive loading is not part of the flow
/// contract. States without a SceneName deliberately inherit the scene left
/// by the previous owner (their dedicated scenes do not exist yet). Async
/// loads capture the controller's SceneGeneration; a load that finishes
/// after a newer transition still activates (Unity cannot abandon an async
/// load) but its OnSceneReady and side effects are suppressed.
/// </summary>
public abstract class BaseGameFlowState : State
{
    #region Properties

    /// <summary>
    /// Reference to the GameFlowController managing these states
    /// </summary>
    protected GameFlowController Controller { get; private set; }

    /// <summary>
    /// The GameFlowState enum value this state represents
    /// </summary>
    public abstract GameFlowState StateType { get; }

    /// <summary>
    /// Name of the scene to load when entering this state (if any)
    /// </summary>
    protected virtual string SceneName => null;

    /// <summary>
    /// Whether this state requires a scene to be loaded
    /// </summary>
    protected virtual bool RequiresSceneLoad => !string.IsNullOrEmpty(SceneName);

    // In-flight scene load, stopped on Exit so a dead state never runs
    // completion code
    private Coroutine _loadRoutine;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize this state with a reference to the controller
    /// </summary>
    public virtual void Initialize(GameFlowController controller)
    {
        Controller = controller;
    }

    #endregion

    #region State Lifecycle

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"[GameFlow] Entering {StateType} state");

        if (RequiresSceneLoad)
        {
            _loadRoutine = Controller.StartCoroutine(LoadSceneAsync());
        }
        else
        {
            OnSceneReady();
        }
    }

    public override void Exit()
    {
        Debug.Log($"[GameFlow] Exiting {StateType} state");

        if (_loadRoutine != null)
        {
            Controller.StopCoroutine(_loadRoutine);
            _loadRoutine = null;
            Controller.ShowLoadingScreen(false);
        }

        OnStateExit();
        base.Exit();
    }

    #endregion

    #region Scene Loading

    /// <summary>
    /// Asynchronously loads the state's scene in single mode. Captures the
    /// flow generation up front: when a newer transition supersedes this
    /// load, the scene still activates (an async load cannot be abandoned
    /// without wedging Unity's load queue) but OnSceneReady is skipped —
    /// the new owner's transition decides what happens next.
    /// </summary>
    protected virtual IEnumerator LoadSceneAsync()
    {
        var myGeneration = Controller.SceneGeneration;
        Debug.Log($"[GameFlow] Loading scene: {SceneName} (generation {myGeneration})");

        // Show loading UI if available
        Controller.ShowLoadingScreen(true);

        // Load the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until scene is almost loaded (0.9 = ready but not activated)
        while (asyncLoad.progress < 0.9f)
        {
            Controller.UpdateLoadingProgress(asyncLoad.progress);
            yield return null;
        }

        // Activate the scene — even when stale, or the load queue wedges
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to fully load
        yield return asyncLoad;

        // Hide loading UI
        Controller.ShowLoadingScreen(false);
        _loadRoutine = null;

        if (Controller.SceneGeneration != myGeneration)
        {
            Debug.LogWarning($"[GameFlow] Stale scene load of '{SceneName}' (generation {myGeneration} vs {Controller.SceneGeneration}) — OnSceneReady suppressed.");
            yield break;
        }

        // Scene is ready
        OnSceneReady();
    }

    /// <summary>
    /// Asynchronously unloads a scene
    /// </summary>
    protected virtual IEnumerator UnloadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            yield break;

        Debug.Log($"[GameFlow] Unloading scene: {sceneName}");

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        yield return asyncUnload;

        Debug.Log($"[GameFlow] Scene unloaded: {sceneName}");
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Called when the scene is fully loaded and ready (or immediately if no scene is needed)
    /// Override this to implement state-specific initialization logic
    /// </summary>
    protected virtual void OnSceneReady()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when exiting this state
    /// Override this to implement state-specific cleanup logic
    /// </summary>
    protected virtual void OnStateExit()
    {
        // Override in derived classes
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Transition to a specific state type. Always routes through the
    /// controller so flow bookkeeping stays atomic — never swap the
    /// underlying machine state directly (issue #18).
    /// </summary>
    protected void TransitionToState(GameFlowState targetState)
    {
        Controller.TransitionToState(targetState);
    }

    #endregion
}
