using UnityEngine;

/// <summary>
/// Runs a worn item's defensive traits: while equipped, shapes damage the
/// wearer takes via the TweakDamageEvent stage (same hook statuses use).
/// PhysicalResist/PhysicalWeakness are live; element-tagged traits join when
/// damage carries an element (BATTLE_PLAN 1.10).
/// </summary>
public class GearDefenseFeature : Feature
{
    private Unit owner;

    protected override void OnApply()
    {
        owner = _target.GetComponentInParent<Unit>();
        this.Subscribe<TweakDamageEvent>(OnTweakDamage);
    }

    protected override void OnRemove()
    {
        this.Unsubscribe<TweakDamageEvent>(OnTweakDamage);
        owner = null;
    }

    private void OnTweakDamage(TweakDamageEvent e)
    {
        if (owner == null || e.Target != owner)
            return;

        var tag = GetComponent<GearTag>();
        var gear = tag != null ? GearCatalog.Get(tag.gearId) : null;
        if (gear == null || gear.traits == null)
            return;

        foreach (var trait in gear.traits)
        {
            // High sortOrder: resists multiply after additive tweaks
            switch (trait.type)
            {
                case GearTraitType.PhysicalResist when e.IsPhysical:
                    e.Modifiers.Add(new MultValueModifier(100, 1f - trait.value / 100f));
                    break;
                case GearTraitType.PhysicalWeakness when e.IsPhysical:
                    e.Modifiers.Add(new MultValueModifier(100, 1f + trait.value / 100f));
                    break;
                case GearTraitType.ElementResist when MatchesElement(e, trait):
                    e.Modifiers.Add(new MultValueModifier(100, 1f - trait.value / 100f));
                    break;
                case GearTraitType.ElementWeakness when MatchesElement(e, trait):
                    e.Modifiers.Add(new MultValueModifier(100, 1f + trait.value / 100f));
                    break;
            }
        }
    }

    // Does the incoming damage carry the element this trait names?
    private static bool MatchesElement(TweakDamageEvent e, GearTraitData trait)
    {
        return e.HasElement &&
               System.Enum.TryParse(trait.tag, out ElementTypes element) &&
               e.Element == element;
    }
}
