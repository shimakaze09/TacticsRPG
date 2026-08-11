using UnityEngine;

public enum Difficulty
{
    Easy = 0,
    Hard = 1
}

/// <summary>
/// Global difficulty selection and its combat modifiers.
///
/// Easy: the classic pattern-driven AI (ComputerPlayer), no stat scaling.
/// Hard: the tactical AI (TacticalComputerPlayer) plus enemies with +30% HP
/// and +20% outgoing damage — noticeably tougher, deliberately NOT doubled,
/// so a well-played battle stays winnable.
///
/// Stored in PlayerPrefs (a machine-level option, like audio volume — not
/// part of the save file). Selectable in-editor via Tactics RPG → Difficulty.
/// </summary>
public static class DifficultySettings
{
    private const string PrefsKey = "TacticsRPG.Difficulty";

    // While a battle is being resolved, Current answers with this snapshot so
    // mid-battle preference changes cannot swing HP multipliers or AI choice
    private static Difficulty? battleLock;

    /// <summary>
    /// The difficulty every combat system reads. While a battle is locked
    /// (issue #62), this is the value snapshotted at battle start — changing
    /// the stored preference mid-battle only affects the NEXT battle.
    /// </summary>
    public static Difficulty Current
    {
        get => battleLock ?? (Difficulty)PlayerPrefs.GetInt(PrefsKey, (int)Difficulty.Easy);
        set
        {
            PlayerPrefs.SetInt(PrefsKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>True while a battle holds the difficulty snapshot.</summary>
    public static bool IsLockedForBattle => battleLock.HasValue;

    /// <summary>
    /// Snapshots the current difficulty for the duration of a battle —
    /// called by battle init before any difficulty-dependent stat or AI work.
    /// </summary>
    public static void LockForBattle()
    {
        battleLock = Current;
    }

    /// <summary>Releases the battle snapshot; the stored preference rules again.</summary>
    public static void ReleaseBattleLock()
    {
        battleLock = null;
    }

    /// <summary>Applied to enemy MHP during stat recalculation.</summary>
    public static float EnemyHpMultiplier => Current == Difficulty.Hard ? 1.3f : 1f;

    /// <summary>Applied to enemy outgoing damage at the TweakDamage stage.</summary>
    public static float EnemyDamageMultiplier => Current == Difficulty.Hard ? 1.2f : 1f;
}
