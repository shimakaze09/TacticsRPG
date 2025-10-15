public class AttackedStatusCondition : StatusCondition
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats)
            this.SubscribeToSender<StatDidChangeEvent>(OnHitNotification, stats);
    }

    private void OnDisable()
    {
        if (stats)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnHitNotification, stats);
    }

    private void OnHitNotification(StatDidChangeEvent e)
    {
        if (e.StatType == StatTypes.HP && e.NewValue < e.OldValue)
        {
            Remove();
        }
    }
}