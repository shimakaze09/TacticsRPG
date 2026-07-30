using UnityEngine;

/// <summary>
/// Battle-wide elemental law: damage carrying an element hits +25% into the
/// element it beats and -25% into the element that beats it (the
/// ElementRelationship table). Lives on the BattleController, like
/// ElevationRules, so the rule applies exactly once per calculation.
/// </summary>
public class ElementRules : MonoBehaviour
{
    [Tooltip("Multiplier when the damage's element beats the target's")]
    public float advantageMultiplier = 1.25f;

    [Tooltip("Multiplier when the target's element beats the damage's")]
    public float restraintMultiplier = 0.75f;

    private void OnEnable()
    {
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
    }

    private void OnTweakDamage(TweakDamageEvent e)
    {
        if (!e.HasElement || e.Target == null)
            return;

        var targetAffinity = e.Target.GetComponent<Elements>();
        if (targetAffinity == null)
            return;

        var (advantaged, restrained) = ElementRelationship.elementRestriction[e.Element];

        // High sortOrder: elemental scaling applies after additive tweaks
        if (targetAffinity.types == advantaged)
            e.Modifiers.Add(new MultValueModifier(100, advantageMultiplier));
        else if (targetAffinity.types == restrained)
            e.Modifiers.Add(new MultValueModifier(100, restraintMultiplier));
    }
}
