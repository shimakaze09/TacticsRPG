using UnityEngine;

public class AutoStatusController : MonoBehaviour
{
    private void OnEnable()
    {
        this.Subscribe<StatDidChangeEvent>(OnHPDidChangeNotification);
    }

    private void OnDisable()
    {
        this.Unsubscribe<StatDidChangeEvent>(OnHPDidChangeNotification);
    }

    private void OnHPDidChangeNotification(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        if (e.NewValue == 0)
        {
            var status = e.Stats.GetComponentInChildren<Status>();
            var c = status.Add<KnockOutStatusEffect, StatComparisonCondition>();
            c.Init(StatTypes.HP, 0, c.EqualTo);
        }
    }
}