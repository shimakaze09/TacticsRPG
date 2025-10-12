using UnityEngine;

public class Job : MonoBehaviour
{
    #region MonoBehaviour

    private void OnDestroy()
    {
        this.RemoveObserver(OnLvlChangeNotification, Stats.DidChangeNotification(StatTypes.LVL), stats);
    }

    #endregion

    #region Event Handlers

    protected virtual void OnLvlChangeNotification(object sender, object args)
    {
        var oldValue = (int)args;
        var newValue = stats[StatTypes.LVL];

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
        this.AddObserver(OnLvlChangeNotification, Stats.DidChangeNotification(StatTypes.LVL), stats);

        var features = GetComponentsInChildren<Feature>();
        for (var i = 0; i < features.Length; i++)
            features[i].Activate(gameObject);
    }

    public void UnEmploy()
    {
        var features = GetComponentsInChildren<Feature>();
        for (var i = 0; i < features.Length; i++)
            features[i].Deactivate();

        this.RemoveObserver(OnLvlChangeNotification, Stats.DidChangeNotification(StatTypes.LVL), stats);
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