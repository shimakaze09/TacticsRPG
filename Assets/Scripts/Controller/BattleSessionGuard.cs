using UnityEngine;

/// <summary>
/// Ensures a battle's session-scoped settings never outlive it (issue #62):
/// arming locks the difficulty snapshot and applies the configured pacing;
/// OnDestroy releases both. Because destruction is the one path every battle
/// exit shares — victory, defeat, forfeit to world, a flow transition
/// unloading the scene, an aborted battle — an abnormal exit can no longer
/// strand the difficulty lock or a non-1× Time.timeScale. Also remembers the
/// pacing the battle started with so pause-resume restores that, not a
/// preference changed mid-battle. Added by InitBattleState.
/// </summary>
public class BattleSessionGuard : MonoBehaviour
{
    /// <summary>
    /// The battle-speed percent the running battle applied at init; null when
    /// no battle is running. Pause-resume reads this so a mid-battle
    /// preference change waits for the next battle.
    /// </summary>
    public static int? ActiveBattleSpeedPercent { get; private set; }

    /// <summary>Locks difficulty and applies the configured pacing for this battle.</summary>
    public void Arm()
    {
        DifficultySettings.LockForBattle();
        ActiveBattleSpeedPercent = GameSettings.BattleSpeedPercent;
        Time.timeScale = ActiveBattleSpeedPercent.Value / 100f;
    }

    // Destruction is the shared exit path — normal or abnormal
    private void OnDestroy()
    {
        Release();
    }

    /// <summary>Releases the lock and pacing; idempotent, so early release plus destroy is safe.</summary>
    public static void Release()
    {
        DifficultySettings.ReleaseBattleLock();
        ActiveBattleSpeedPercent = null;
        Time.timeScale = 1f;
    }
}
