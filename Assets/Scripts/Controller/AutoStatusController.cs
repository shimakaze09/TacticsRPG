using UnityEngine;

public class AutoStatusController : MonoBehaviour
{
    private void OnEnable()
    {
        this.AddObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
    }

    private void OnHPDidChangeNotification(object sender, object args)
    {
        var stats = sender as Stats;
        if (stats[StatTypes.HP] == 0)
        {
            var status = stats.GetComponentInChildren<Status>();
            var c = status.Add<KnockOutStatusEffect, StatComparisonCondition>();
            c.Init(StatTypes.HP, 0, c.EqualTo);
        }
    }
}