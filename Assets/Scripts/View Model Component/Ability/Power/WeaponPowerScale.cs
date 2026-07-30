using UnityEngine;

/// <summary>
/// Scales the basic attack's power by the equipped weapon's damage profile
/// (GearData.damagePercent): precision weapons strike above 100%, wide
/// footprints below it. Lives on the Attack ability beside its power source
/// and multiplies after the base power is contributed.
/// </summary>
public class WeaponPowerScale : MonoBehaviour
{
    private void OnEnable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
            this.SubscribeToSender<GetPowerEvent>(OnGetPower, effect);
    }

    private void OnDisable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
            this.UnsubscribeFromSender<GetPowerEvent>(OnGetPower, effect);
    }

    private void OnGetPower(GetPowerEvent e)
    {
        if (GetComponentInParent<Unit>() != e.Attacker)
            return;

        var gear = GearCatalog.EquippedWeapon(this);
        if (gear != null && gear.damagePercent != 100)
            e.Modifiers.Add(new MultValueModifier(1, gear.damagePercent / 100f));
    }
}
