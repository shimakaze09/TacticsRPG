public class AbsorbDamageAbilityEffect : BaseAbilityEffect
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
        this.SubscribeToSender<AbilityHitEvent>(OnEffectHit, effect);
        this.SubscribeToSender<AbilityMissedEvent>(OnEffectMiss, effect);
    }

    private void OnDisable()
    {
        if (effect != null)
        {
            this.UnsubscribeFromSender<AbilityHitEvent>(OnEffectHit, effect);
            this.UnsubscribeFromSender<AbilityMissedEvent>(OnEffectMiss, effect);
        }
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

    private void OnEffectHit(AbilityHitEvent e)
    {
        amount = e.Damage * -1;
    }

    private void OnEffectMiss(AbilityMissedEvent e)
    {
        amount = 0;
    }

    #endregion
}