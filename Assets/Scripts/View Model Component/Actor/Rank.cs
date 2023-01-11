using UnityEngine;

public class Rank : MonoBehaviour
{
    #region Consts

    public const int minLevel = 1;
    public const int maxLevel = 99;
    public const int maxExperience = 999999;

    #endregion

    #region Fields / Properties

    public int LVL => stats[StatTypes.LVL];

    public int EXP
    {
        get => stats[StatTypes.EXP];
        set => stats[StatTypes.EXP] = value;
    }

    public float LevelPercent => (float)(LVL - minLevel) / (float)(maxLevel - minLevel);

    private Stats stats;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        stats = GetComponent<Stats>();
    }

    private void OnEnable()
    {
        this.AddObserver(OnExpWillChange, Stats.WillChangeNotification(StatTypes.EXP), stats);
        this.AddObserver(OnExpDidChange, Stats.DidChangeNotification(StatTypes.EXP), stats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnExpWillChange, Stats.WillChangeNotification(StatTypes.EXP), stats);
        this.RemoveObserver(OnExpDidChange, Stats.DidChangeNotification(StatTypes.EXP), stats);
    }

    #endregion

    #region Event Handlers

    private void OnExpWillChange(object sender, object args)
    {
        var vce = args as ValueChangeException;
        vce.AddModifier(new ClampValueModifier(int.MaxValue, EXP, maxExperience));
    }

    private void OnExpDidChange(object sender, object args)
    {
        stats.SetValue(StatTypes.LVL, LevelForExperience(EXP), false);
    }

    #endregion

    #region Public

    public static int ExperienceForLevel(int level)
    {
        var levelPercent = Mathf.Clamp01((float)(level - minLevel) / (float)(maxLevel - minLevel));
        return (int)EasingEquations.EaseInQuad(0, maxExperience, levelPercent);
    }

    public static int LevelForExperience(int exp)
    {
        var lvl = maxLevel;
        for (; lvl >= minLevel; lvl--)
            if (exp >= ExperienceForLevel(lvl))
                break;
        return lvl;
    }

    public void Init(int level)
    {
        stats.SetValue(StatTypes.LVL, level, false);
        stats.SetValue(StatTypes.EXP, ExperienceForLevel(level), false);
    }

    #endregion
}