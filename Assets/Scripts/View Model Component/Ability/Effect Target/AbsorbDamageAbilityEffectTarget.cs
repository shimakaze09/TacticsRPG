public class AbsorbDamageAbilityEffectTarget : BaseAbilityEffect
{
    #region Private

    private BaseAbilityEffect GetTrackedEffect()
    {
        var owner = GetComponentInParent<Ability>().transform;
        if (trackedSiblingIndex >= 0 && trackedSiblingIndex < owner.childCount)
        {
            var sibling = owner.GetChild(trackedSiblingIndex);
            return sibling.GetComponent<BaseAbilityEffect>();
        }

        return null;
    }

    #endregion

    #region Fields

    public int trackedSiblingIndex;
    private BaseAbilityEffect effect;
    private int amount;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        effect = GetTrackedEffect();
    }

    private void OnEnable()
    {
        this.AddObserver(OnEffectHit, HitNotification, effect);
        this.AddObserver(OnEffectMiss, MissedNotification, effect);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnEffectHit, HitNotification, effect);
        this.RemoveObserver(OnEffectMiss, MissedNotification, effect);
    }

    #endregion

    #region Base Ability Effect

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        var s = GetComponentInParent<Stats>();
        s[StatTypes.HP] += amount;
        return amount;
    }

    #endregion

    #region Event Handlers

    private void OnEffectHit(object sender, object args)
    {
        amount = (int)args * -1;
    }

    private void OnEffectMiss(object sender, object args)
    {
        amount = 0;
    }

    #endregion
}