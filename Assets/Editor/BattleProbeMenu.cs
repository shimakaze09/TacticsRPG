using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Menu entry for the battle regression suite: opens the Battle scene,
/// flags the run, and enters play mode — BattleProbeRunner does the rest.
/// </summary>
public static class BattleProbeMenu
{
    [MenuItem("Tactics RPG/Run Battle Probes")]
    public static void RunBattleProbes()
    {
        if (EditorApplication.isPlaying)
        {
            UnityEngine.Debug.LogWarning("[Probes] Already in play mode — stop first.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
        SessionState.SetBool(BattleProbeRunner.TriggerFlag, true);
        EditorApplication.isPlaying = true;
    }

    /// <summary>
    /// Batch entry point (CI / headless):
    /// Unity -batchmode -projectPath ... -executeMethod BattleProbeMenu.RunHeadless
    /// — no -quit; the runner exits with 0 when every probe passes.
    /// </summary>
    public static void RunHeadless()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
        SessionState.SetBool(BattleProbeRunner.TriggerFlag, true);
        EditorApplication.isPlaying = true;
    }
}
