#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor convenience for switching difficulty until the title-screen options
/// UI exists. The setting lives in PlayerPrefs (see DifficultySettings).
/// </summary>
public static class DifficultyMenu
{
    private const string EasyPath = "Tactics RPG/Difficulty/Easy";
    private const string HardPath = "Tactics RPG/Difficulty/Hard";

    [MenuItem(EasyPath)]
    private static void SetEasy() => DifficultySettings.Current = Difficulty.Easy;

    [MenuItem(HardPath)]
    private static void SetHard() => DifficultySettings.Current = Difficulty.Hard;

    [MenuItem(EasyPath, true)]
    private static bool ValidateEasy()
    {
        Menu.SetChecked(EasyPath, DifficultySettings.Current == Difficulty.Easy);
        return true;
    }

    [MenuItem(HardPath, true)]
    private static bool ValidateHard()
    {
        Menu.SetChecked(HardPath, DifficultySettings.Current == Difficulty.Hard);
        return true;
    }
}
#endif
