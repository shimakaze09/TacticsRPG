using UnityEngine;

/// <summary>
/// Battle-wide elevation rules: attacking from meaningfully higher ground
/// deals more damage and lands more easily; attacking uphill suffers the
/// reverse. Hooks the TweakDamage and hit-rate stages, so the tactical AI
/// weighs elevation automatically. One instance lives on the
/// BattleController (added by InitBattleState).
/// </summary>
public class ElevationRules : MonoBehaviour
{
    /// <summary>Height difference (in tile heights) that counts as high ground.</summary>
    public const int HighGroundAdvantage = 2;

    [Tooltip("Outgoing damage multiplier when attacking from high ground")]
    public float highGroundDamageMultiplier = 1.15f;

    [Tooltip("Outgoing damage multiplier when attacking from low ground")]
    public float lowGroundDamageMultiplier = 0.9f;

    [Tooltip("Evade/resist shift for elevation (positive = easier to hit downhill targets)")]
    public int highGroundHitBonus = 10;

    // Subscribes the elevation hooks for the battle's lifetime
    private void OnEnable()
    {
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
        this.Subscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    // Symmetric unsubscribe
    private void OnDisable()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
        this.Unsubscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    // Scales damage by the attacker's elevation relative to the target
    private void OnTweakDamage(TweakDamageEvent e)
    {
        var rise = HeightDifference(e.Attacker, e.Target);
        if (rise >= HighGroundAdvantage)
            e.Modifiers.Add(new MultValueModifier(96, highGroundDamageMultiplier));
        else if (rise <= -HighGroundAdvantage)
            e.Modifiers.Add(new MultValueModifier(96, lowGroundDamageMultiplier));
    }

    // Shifts effective evade/resist by elevation (lower = easier to hit)
    private void OnHitRateCheck(HitRateStatusCheckEvent e)
    {
        var rise = HeightDifference(e.Attacker, e.Target);
        if (rise >= HighGroundAdvantage)
            e.Args.HitRate -= highGroundHitBonus;
        else if (rise <= -HighGroundAdvantage)
            e.Args.HitRate += highGroundHitBonus;
    }

    // Attacker height minus target height; 0 when either is off-board
    private static int HeightDifference(Unit attacker, Unit target)
    {
        if (attacker == null || target == null || attacker.tile == null || target.tile == null)
            return 0;
        return attacker.tile.height - target.tile.height;
    }
}
