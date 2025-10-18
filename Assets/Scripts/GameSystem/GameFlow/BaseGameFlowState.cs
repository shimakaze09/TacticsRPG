using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base class for all game flow states.
/// Provides common functionality for scene loading, async operations, and GameFlowController access.
/// Inherits from State to integrate with the existing state machine pattern.
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
            Controller.StartCoroutine(LoadSceneAsync());
        }
        else
        {
            OnSceneReady();
        }
    }

    public override void Exit()
    {
        Debug.Log($"[GameFlow] Exiting {StateType} state");
        OnStateExit();
        base.Exit();
    }

    #endregion

    #region Scene Loading

    /// <summary>
    /// Asynchronously loads the scene for this state
    /// </summary>
    protected virtual IEnumerator LoadSceneAsync()
    {
        Debug.Log($"[GameFlow] Loading scene: {SceneName}");

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

        // Activate the scene
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to fully load
        yield return asyncLoad;

        // Hide loading UI
        Controller.ShowLoadingScreen(false);

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
    /// Transition to another game flow state
    /// </summary>
    protected void TransitionToState<T>() where T : BaseGameFlowState
    {
        Controller.ChangeState<T>();
    }

    /// <summary>
    /// Transition to a specific state type
    /// </summary>
    protected void TransitionToState(GameFlowState targetState)
    {
        Controller.TransitionToState(targetState);
    }

    #endregion
}
