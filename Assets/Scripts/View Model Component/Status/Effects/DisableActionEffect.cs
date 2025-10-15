using UnityEngine;

public class DisableActionEffectBase : MonoBehaviour
{
    public bool blockActions = true;     // blocks using abilities/attacks
    public bool blockMovement = false;   // blocks movement

    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner != null)
        {
            if (blockActions)
                this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
            if (blockMovement)
                this.Subscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            if (blockActions)
                this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
            if (blockMovement)
                this.Unsubscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);
        }
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        // Find the unit that owns this ability and block only if it is the owner.
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (unit != owner)
            return;

        if (e.Exception != null && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }

    private void OnCanMoveCheck(MovementCanMoveCheckEvent e)
    {
        var unit = e.Movement.GetComponentInParent<Unit>();
        if (unit != owner)
            return;

        if (e.Exception != null && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }
}