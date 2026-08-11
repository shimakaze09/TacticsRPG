using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The reusable Settings panel (issue #62), reachable from the title screen
/// and anywhere else via Open(). Builds its whole UI in code on an overlay
/// canvas — no prefab or scene wiring needed, so it works identically in
/// Title, World, and paused Battle. Rows are stepper-based (label, &lt;, value,
/// &gt;) which keyboard/mouse can drive today and action maps can navigate
/// later (#36/#48). Edits write straight into GameSettings /
/// DifficultySettings: volumes and text scale apply immediately, battle
/// speed and difficulty are labeled next-battle, and resolution applies
/// behind a RevertCountdown so an unconfirmed display change rolls back.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    private static SettingsPanelController instance;

    private Canvas canvas;
    private TMP_Text difficultyValue, difficultyDescription;
    private TMP_Text masterValue, textScaleValue, speedValue, windowValue, resolutionValue;
    private TMP_Text countdownLabel;
    private GameObject confirmRow;

    private List<Resolution> resolutionOptions;
    private int resolutionIndex;
    private Resolution revertResolution;
    private FullScreenMode revertWindowMode;
    private readonly RevertCountdown countdown = new RevertCountdown();

    /// <summary>
    /// Shows the settings panel (creating it on first use). The single
    /// entry point for the title screen, pause menu, and UIManager.
    /// </summary>
    public static SettingsPanelController Open()
    {
        if (instance == null)
        {
            var root = new GameObject("SettingsPanel");
            instance = root.AddComponent<SettingsPanelController>();
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    /// <summary>True while a settings panel is open (for input gating).</summary>
    public static bool IsOpen => instance != null && instance.gameObject.activeSelf;

    // Build once; GameSettings migration runs before any value is displayed
    private void Awake()
    {
        GameSettings.MigrateIfNeeded();
        BuildUi();
        RefreshAll();
    }

    private void OnEnable()
    {
        GameSettings.ApplyImmediate();
        GameSettings.ApplyTextScale(canvas);
        RefreshAll();
    }

    // Drives the resolution revert deadline and the Escape shortcut
    private void Update()
    {
        if (countdown.Armed)
        {
            if (countdownLabel != null)
                countdownLabel.text = $"Keep this display mode? Reverting in {countdown.RemainingSeconds(Time.unscaledTime)}s";

            if (countdown.HasExpired(Time.unscaledTime))
                RevertDisplayChange();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    /// <summary>Hides the panel (state stays for the next Open).</summary>
    public void Close()
    {
        if (countdown.Armed)
            RevertDisplayChange();
        gameObject.SetActive(false);
    }

    #region Row actions

    // Difficulty flips the STORED preference — during a battle, Current
    // answers with the locked snapshot, so the preference is the edit target
    private void StepDifficulty(int direction)
    {
        DifficultySettings.Current = DifficultySettings.StoredPreference == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy;
        RefreshAll();
    }

    // One volume control that genuinely works today (the global listener);
    // per-channel music/SFX rows arrive with #35's routing
    private void StepMaster(int direction)
    {
        GameSettings.MasterVolume += direction * 10;
        GameSettings.ApplyImmediate();
        RefreshAll();
    }

    private void StepTextScale(int direction)
    {
        GameSettings.TextScalePercent += direction * 10;
        GameSettings.ApplyTextScale(canvas);
        RefreshAll();
    }

    // Battle speed cycles the legal steps; takes effect when a battle starts
    private void StepBattleSpeed(int direction)
    {
        int[] steps = GameSettings.BattleSpeedSteps;
        int index = System.Array.IndexOf(steps, GameSettings.BattleSpeedPercent);
        index = (index + direction + steps.Length) % steps.Length;
        GameSettings.BattleSpeedPercent = steps[index];
        RefreshAll();
    }

    private void StepWindowMode(int direction)
    {
        GameSettings.WindowMode = GameSettings.WindowMode == FullScreenMode.Windowed
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
        RefreshAll();
    }

    private void StepResolution(int direction)
    {
        if (resolutionOptions.Count == 0)
            return;
        resolutionIndex = (resolutionIndex + direction + resolutionOptions.Count) % resolutionOptions.Count;
        RefreshAll();
    }

    // Applies the selected display mode and arms the revert window
    private void ApplyDisplayChange()
    {
        if (resolutionOptions.Count == 0)
            return;

        revertResolution = GameSettings.PreferredResolution;
        revertWindowMode = Screen.fullScreenMode;

        GameSettings.PreferredResolution = resolutionOptions[resolutionIndex];
        GameSettings.ApplyResolution();
        countdown.Arm(Time.unscaledTime);
        RefreshAll();
    }

    // The player accepted the new display mode inside the countdown
    private void ConfirmDisplayChange()
    {
        countdown.Confirm();
        RefreshAll();
    }

    // Deadline passed (or the panel closed): restore the previous mode
    private void RevertDisplayChange()
    {
        countdown.Disarm();
        GameSettings.PreferredResolution = revertResolution;
        GameSettings.WindowMode = revertWindowMode;
        GameSettings.ApplyResolution();
        SyncResolutionIndex();
        RefreshAll();
    }

    private void ResetDefaults()
    {
        GameSettings.ResetToDefaults();
        GameSettings.ApplyImmediate();
        GameSettings.ApplyTextScale(canvas);
        SyncResolutionIndex();
        RefreshAll();
    }

    #endregion

    #region Display refresh

    // Repaints every value label from the stored state
    private void RefreshAll()
    {
        if (difficultyValue == null)
            return;

        // Display the stored preference, not the battle-locked snapshot —
        // otherwise a mid-battle change looks like it did nothing
        Difficulty preference = DifficultySettings.StoredPreference;
        difficultyValue.text = preference.ToString();
        difficultyDescription.text = preference == Difficulty.Hard
            ? "Tactical AI; enemies gain +30% HP and +20% damage. Rewards are identical."
            : "Classic patterned AI; enemies fight at their listed stats.";
        if (DifficultySettings.IsLockedForBattle && preference != DifficultySettings.Current)
            difficultyDescription.text += " Change takes effect next battle.";

        masterValue.text = GameSettings.MasterVolume + "%";
        textScaleValue.text = GameSettings.TextScalePercent + "%";
        speedValue.text = (GameSettings.BattleSpeedPercent / 100f).ToString("0.#") + "x (next battle)";
        windowValue.text = GameSettings.WindowMode == FullScreenMode.Windowed ? "Windowed" : "Fullscreen";

        if (resolutionOptions.Count > 0)
        {
            Resolution r = resolutionOptions[resolutionIndex];
            resolutionValue.text = $"{r.width}×{r.height}";
        }
        else
        {
            resolutionValue.text = "n/a";
        }

        confirmRow.SetActive(countdown.Armed);
    }

    // Points the resolution stepper at the currently stored resolution
    private void SyncResolutionIndex()
    {
        Resolution current = GameSettings.PreferredResolution;
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].width == current.width && resolutionOptions[i].height == current.height)
            {
                resolutionIndex = i;
                return;
            }
        }

        resolutionIndex = Mathf.Max(0, resolutionOptions.Count - 1);
    }

    #endregion

    #region UI construction

    // Builds the overlay canvas and all rows; runtime-only, no prefab
    private void BuildUi()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        gameObject.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(transform, false);
        }

        // Dim layer over whatever screen opened the panel
        Image dim = MakeImage(transform, "Dim", new Color(0f, 0f, 0f, 0.6f));
        Stretch(dim.rectTransform);

        // Center card
        Image card = MakeImage(transform, "Card", new Color(0.09f, 0.1f, 0.12f, 0.97f));
        var cardRect = card.rectTransform;
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(640f, 560f);

        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        MakeText(card.transform, "Title", "Settings", 26, FontStyles.Bold);

        difficultyValue = MakeStepperRow(card.transform, "Difficulty", StepDifficulty);
        difficultyDescription = MakeText(card.transform, "DifficultyDescription", "", 14, FontStyles.Italic);

        masterValue = MakeStepperRow(card.transform, "Master volume", StepMaster);
        textScaleValue = MakeStepperRow(card.transform, "Text scale", StepTextScale);
        speedValue = MakeStepperRow(card.transform, "Battle speed", StepBattleSpeed);
        windowValue = MakeStepperRow(card.transform, "Window mode", StepWindowMode);
        resolutionValue = MakeStepperRow(card.transform, "Resolution", StepResolution);

        // Display apply + confirm/revert countdown
        var applyRow = MakeRow(card.transform, "ApplyRow");
        MakeButton(applyRow.transform, "Apply display mode", ApplyDisplayChange, 220f);

        confirmRow = MakeRow(card.transform, "ConfirmRow").gameObject;
        countdownLabel = MakeText(confirmRow.transform, "Countdown", "", 14, FontStyles.Normal);
        countdownLabel.GetComponent<LayoutElement>().flexibleWidth = 1f;
        MakeButton(confirmRow.transform, "Keep", ConfirmDisplayChange, 100f);
        MakeButton(confirmRow.transform, "Revert", RevertDisplayChange, 100f);

        var footer = MakeRow(card.transform, "Footer");
        MakeButton(footer.transform, "Reset to defaults", ResetDefaults, 200f);
        MakeButton(footer.transform, "Close", Close, 120f);

        resolutionOptions = DistinctResolutions();
        SyncResolutionIndex();
    }

    // This machine's supported resolutions, one entry per width×height
    private static List<Resolution> DistinctResolutions()
    {
        var seen = new HashSet<long>();
        var options = new List<Resolution>();
        foreach (Resolution r in Screen.resolutions)
        {
            long key = (long)r.width << 32 | (uint)r.height;
            if (seen.Add(key))
                options.Add(r);
        }

        return options;
    }

    // One "Label   <  value  >" row
    private TMP_Text MakeStepperRow(Transform parent, string label, System.Action<int> step)
    {
        var row = MakeRow(parent, label + "Row");

        TMP_Text caption = MakeText(row.transform, "Label", label, 16, FontStyles.Normal);
        caption.GetComponent<LayoutElement>().flexibleWidth = 1f;

        MakeButton(row.transform, "<", () => step(-1), 44f);
        TMP_Text value = MakeText(row.transform, "Value", "", 16, FontStyles.Bold);
        var valueLayout = value.GetComponent<LayoutElement>();
        valueLayout.preferredWidth = 190f;
        value.alignment = TextAlignmentOptions.Center;
        MakeButton(row.transform, ">", () => step(1), 44f);

        return value;
    }

    // Horizontal container row with layout defaults
    private static HorizontalLayoutGroup MakeRow(Transform parent, string name)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        var element = row.AddComponent<LayoutElement>();
        element.preferredHeight = 40f;
        return layout;
    }

    // Solid-color image (no sprite needed)
    private static Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    // TMP label with a LayoutElement for row sizing
    private static TMP_Text MakeText(Transform parent, string name, string text, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        go.AddComponent<LayoutElement>();
        return tmp;
    }

    // Clickable button with label; keyboard-navigable via UI Selectable
    private static Button MakeButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, float width)
    {
        Image background = MakeImage(parent, label + "Button", new Color(0.2f, 0.22f, 0.27f, 1f));
        var button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        var element = background.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = 34f;

        TMP_Text text = MakeText(background.transform, "Label", label, 15, FontStyles.Normal);
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.rectTransform);
        return button;
    }

    // Anchor a rect to fill its parent
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion
}
