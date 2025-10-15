using System;

public class StatComparisonCondition : StatusCondition
{
    #region Notification Handlers

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != type)
            return;

        if (condition != null && !condition())
            Remove();
    }

    #endregion

    #region Fields

    public StatTypes type { get; private set; }
    public int value { get; private set; }
    public Func<bool> condition { get; private set; }
    private Stats stats;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        stats = GetComponentInParent<Stats>();
    }

    private void OnDisable()
    {
        if (stats)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    #endregion

    #region Public

    public void Init(StatTypes type, int value, Func<bool> condition)
    {
        this.type = type;
        this.value = value;
        this.condition = condition;
        this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    public bool EqualTo()
    {
        return stats[type] == value;
    }

    public bool LessThan()
    {
        return stats[type] < value;
    }

    public bool LessThanOrEqualTo()
    {
        return stats[type] <= value;
    }

    public bool GreaterThan()
    {
        return stats[type] > value;
    }

    public bool GreaterThanOrEqualTo()
    {
        return stats[type] >= value;
    }

    #endregion
}