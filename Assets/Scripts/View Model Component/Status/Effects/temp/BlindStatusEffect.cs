public class BlindStatusEffect : StatusEffect
{
    private void OnEnable()
    {
        this.Subscribe<HitRateStatusCheckEvent>(OnHitRateStatusCheck);
    }

    private void OnDisable()
    {
        this.Unsubscribe<HitRateStatusCheckEvent>(OnHitRateStatusCheck);
    }

    private void OnHitRateStatusCheck(HitRateStatusCheckEvent e)
    {
        var owner = GetComponentInParent<Unit>();
        if (owner == e.Attacker)
            // The attacker is blind
            e.Args.HitRate += 50;
        else if (owner == e.Target)
            // The defender is blind
            e.Args.HitRate -= 20;
    }
}