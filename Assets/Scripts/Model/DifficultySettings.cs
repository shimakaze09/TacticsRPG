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

    public static Difficulty Current
    {
        get => (Difficulty)PlayerPrefs.GetInt(PrefsKey, (int)Difficulty.Easy);
        set
        {
            PlayerPrefs.SetInt(PrefsKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Applied to enemy MHP during stat recalculation.</summary>
    public static float EnemyHpMultiplier => Current == Difficulty.Hard ? 1.3f : 1f;

    /// <summary>Applied to enemy outgoing damage at the TweakDamage stage.</summary>
    public static float EnemyDamageMultiplier => Current == Difficulty.Hard ? 1.2f : 1f;
}
