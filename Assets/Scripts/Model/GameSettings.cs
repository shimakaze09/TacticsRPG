using UnityEngine;

/// <summary>
/// Versioned machine-level preferences (issue #62): volumes, text scale,
/// battle speed, window mode, and resolution. Stored in PlayerPrefs — device
/// state, deliberately separate from campaign saves (Save.game) so wiping or
/// copying a save never touches them. Every property clamps on read AND
/// write, so hand-edited or corrupted prefs recover to valid values instead
/// of propagating garbage. Apply timing is part of the contract:
/// volumes and text scale apply immediately; resolution applies immediately
/// behind a confirm-or-revert countdown (SettingsPanelController); battle
/// speed applies when the next battle starts; difficulty applies to the next
/// battle only (DifficultySettings locks it while one is running).
/// </summary>
public static class GameSettings
{
    /// <summary>Bumped when stored semantics change; mismatch resets to defaults.</summary>
    public const int Version = 1;

    private const string VersionKey = "TacticsRPG.Settings.Version";
    private const string MusicKey = "TacticsRPG.Settings.MusicVolume";
    private const string SfxKey = "TacticsRPG.Settings.SfxVolume";
    private const string TextScaleKey = "TacticsRPG.Settings.TextScale";
    private const string BattleSpeedKey = "TacticsRPG.Settings.BattleSpeed";
    private const string WindowModeKey = "TacticsRPG.Settings.WindowMode";
    private const string ResolutionWidthKey = "TacticsRPG.Settings.ResolutionWidth";
    private const string ResolutionHeightKey = "TacticsRPG.Settings.ResolutionHeight";

    /// <summary>Default music volume percent.</summary>
    public const int DefaultMusicVolume = 80;

    /// <summary>Default SFX volume percent.</summary>
    public const int DefaultSfxVolume = 100;

    /// <summary>Text scale bounds and default (percent).</summary>
    public const int MinTextScale = 80, MaxTextScale = 150, DefaultTextScale = 100;

    /// <summary>Battle speed steps (percent); index 0 is the default.</summary>
    public static readonly int[] BattleSpeedSteps = { 100, 150, 200 };

    /// <summary>Music volume percent (0–100). Applies immediately (#35 routes channels).</summary>
    public static int MusicVolume
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(MusicKey, DefaultMusicVolume), 0, 100);
        set => WriteInt(MusicKey, Mathf.Clamp(value, 0, 100));
    }

    /// <summary>SFX volume percent (0–100). Applies immediately via the audio listener.</summary>
    public static int SfxVolume
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(SfxKey, DefaultSfxVolume), 0, 100);
        set => WriteInt(SfxKey, Mathf.Clamp(value, 0, 100));
    }

    /// <summary>UI text/canvas scale percent (80–150). Applies immediately to registered canvases.</summary>
    public static int TextScalePercent
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(TextScaleKey, DefaultTextScale), MinTextScale, MaxTextScale);
        set => WriteInt(TextScaleKey, Mathf.Clamp(value, MinTextScale, MaxTextScale));
    }

    /// <summary>
    /// Battle animation/AI pacing percent, snapped to BattleSpeedSteps.
    /// Applies when the next battle starts (Time.timeScale for its duration).
    /// </summary>
    public static int BattleSpeedPercent
    {
        get => SnapBattleSpeed(PlayerPrefs.GetInt(BattleSpeedKey, BattleSpeedSteps[0]));
        set => WriteInt(BattleSpeedKey, SnapBattleSpeed(value));
    }

    /// <summary>Fullscreen window (default) or windowed. Applies with the resolution.</summary>
    public static FullScreenMode WindowMode
    {
        get => PlayerPrefs.GetInt(WindowModeKey, 0) == 1 ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        set => WriteInt(WindowModeKey, value == FullScreenMode.Windowed ? 1 : 0);
    }

    /// <summary>
    /// The stored resolution, validated against this machine's supported
    /// list — after a device switch an unsupported saved value falls back to
    /// the current screen resolution instead of failing to apply.
    /// </summary>
    public static Resolution PreferredResolution
    {
        get
        {
            int width = PlayerPrefs.GetInt(ResolutionWidthKey, 0);
            int height = PlayerPrefs.GetInt(ResolutionHeightKey, 0);
            foreach (Resolution supported in Screen.resolutions)
            {
                if (supported.width == width && supported.height == height)
                    return supported;
            }

            return Screen.currentResolution;
        }
        set
        {
            WriteInt(ResolutionWidthKey, value.width);
            WriteInt(ResolutionHeightKey, value.height);
        }
    }

    /// <summary>
    /// Ensures the stored schema matches this build: on first run or after a
    /// version bump, resets every setting to defaults. Call once at startup
    /// (the settings panel also calls it defensively).
    /// </summary>
    public static void MigrateIfNeeded()
    {
        if (PlayerPrefs.GetInt(VersionKey, 0) == Version)
            return;

        ResetToDefaults();
    }

    /// <summary>Restores every setting to its default and stamps the schema version.</summary>
    public static void ResetToDefaults()
    {
        MusicVolume = DefaultMusicVolume;
        SfxVolume = DefaultSfxVolume;
        TextScalePercent = DefaultTextScale;
        BattleSpeedPercent = BattleSpeedSteps[0];
        WindowMode = FullScreenMode.FullScreenWindow;
        PlayerPrefs.DeleteKey(ResolutionWidthKey);
        PlayerPrefs.DeleteKey(ResolutionHeightKey);
        WriteInt(VersionKey, Version);
    }

    /// <summary>Applies the immediate-tier settings (audio; callers apply text scale per canvas).</summary>
    public static void ApplyImmediate()
    {
        // Until #35 adds channel routing, SFX volume drives the master listener
        AudioListener.volume = SfxVolume / 100f;
    }

    /// <summary>Applies the stored window mode + resolution to the display.</summary>
    public static void ApplyResolution()
    {
        Resolution target = PreferredResolution;
        Screen.SetResolution(target.width, target.height, WindowMode);
    }

    /// <summary>Scales a canvas for the text-scale accessibility option.</summary>
    public static void ApplyTextScale(Canvas canvas)
    {
        if (canvas != null)
            canvas.scaleFactor = TextScalePercent / 100f;
    }

    // Snaps an arbitrary stored value to the nearest legal battle-speed step.
    private static int SnapBattleSpeed(int value)
    {
        int best = BattleSpeedSteps[0];
        foreach (int step in BattleSpeedSteps)
        {
            if (Mathf.Abs(value - step) < Mathf.Abs(value - best))
                best = step;
        }

        return best;
    }

    // All writes persist immediately — settings must survive a crash.
    private static void WriteInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }
}
