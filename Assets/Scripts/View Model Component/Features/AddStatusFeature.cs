public abstract class AddStatusFeature<T> : Feature where T : StatusEffect
{
    #region Fields

    private StatusCondition statusCondition;

    #endregion

    #region Protected

    protected override void OnApply()
    {
        var status = GetComponentInParent<Status>();
        statusCondition = status.Add<T, StatusCondition>();
    }

    protected override void OnRemove()
    {
        if (statusCondition != null)
            statusCondition.Remove();
    }

    #endregion
}