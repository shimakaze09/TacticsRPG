using UnityEngine;

/// <summary>
/// LEGACY JOB COMPONENT - DEPRECATED
/// 
/// This is the old job system from Liquid Fire tutorials.
/// It has been superseded by the new FFT-style JobManager system.
/// 
/// MIGRATION GUIDE:
/// - Replace Job component with JobManager component
/// - Use JobDefinition ScriptableObjects instead of prefabs
/// - Use JobProgressData for multi-job stat calculation
/// 
/// This component is kept for backwards compatibility only.
/// New units should use JobManager instead.
/// </summary>
[System.Obsolete("Use JobManager instead. This legacy Job component will be removed in future versions.", false)]
public class Job : MonoBehaviour
{
    #region MonoBehaviour

    private void OnDestroy()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnLvlChangeNotification, stats);
    }

    #endregion

    #region Event Handlers

    protected virtual void OnLvlChangeNotification(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.LVL)
            return;

        var oldValue = e.OldValue;
        var newValue = e.NewValue;

        for (var i = oldValue; i < newValue; i++)
            LevelUp();
    }

    #endregion

    #region Private

    private void LevelUp()
    {
        for (var i = 0; i < statOrder.Length; i++)
        {
            var type = statOrder[i];
            var whole = Mathf.FloorToInt(growStats[i]);
            var fraction = growStats[i] - whole;

            var value = stats[type];
            value += whole;
            if (Random.value > 1f - fraction)
                value++;

            stats.SetValue(type, value, false);
        }

        stats.SetValue(StatTypes.HP, stats[StatTypes.MHP], false);
        stats.SetValue(StatTypes.MP, stats[StatTypes.MMP], false);
    }

    #endregion

    #region Fields / Properties

    public static readonly StatTypes[] statOrder =
    {
        StatTypes.MHP,
        StatTypes.MMP,
        StatTypes.ATK,
        StatTypes.DEF,
        StatTypes.MAT,
        StatTypes.MDF,
        StatTypes.SPD
    };

    public int[] baseStats = new int[statOrder.Length];
    public float[] growStats = new float[statOrder.Length];
    private Stats stats;

    #endregion

    #region Public

    public void Employ()
    {
        stats = gameObject.GetComponentInParent<Stats>();
        this.SubscribeToSender<StatDidChangeEvent>(OnLvlChangeNotification, stats);

        var features = GetComponentsInChildren<Feature>();
        for (var i = 0; i < features.Length; i++)
            features[i].Activate(gameObject);
    }

    public void UnEmploy()
    {
        var features = GetComponentsInChildren<Feature>();
        for (var i = 0; i < features.Length; i++)
            features[i].Deactivate();

        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnLvlChangeNotification, stats);
        stats = null;
    }

    public void LoadDefaultStats()
    {
        for (var i = 0; i < statOrder.Length; i++)
        {
            var type = statOrder[i];
            stats.SetValue(type, baseStats[i], false);
        }

        stats.SetValue(StatTypes.HP, stats[StatTypes.MHP], false);
        stats.SetValue(StatTypes.MP, stats[StatTypes.MMP], false);
    }

    #endregion
}