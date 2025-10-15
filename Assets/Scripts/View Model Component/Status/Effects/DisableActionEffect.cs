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
                this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck);
            if (blockMovement)
                this.AddObserver(OnCanMoveCheck, Movement.CanMoveCheck);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            if (blockActions)
                this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck);
            // if (blockMovement)
            //     this.RemoveObserver(OnCanMoveCheck, Movement.CanMoveCheck);
        }
    }

    private void OnCanPerformCheck(object sender, object args)
    {
        // sender is the Ability component; find its unit and block only if it is the owner.
        var ability = sender as Ability ?? (sender as Component)?.GetComponent<Ability>();
        var unit = ability?.GetComponentInParent<Unit>();
        if (unit != owner)
            return;

        var exc = args as BaseException;
        if (exc != null && exc.defaultToggle)
            exc.FlipToggle();
    }

    private void OnCanMoveCheck(object sender, object args)
    {
        var movement = sender as Movement ?? (sender as Component)?.GetComponent<Movement>();
        var unit = movement?.GetComponentInParent<Unit>();
        if (unit != owner)
            return;

        var exc = args as BaseException;
        if (exc != null && exc.defaultToggle)
            exc.FlipToggle();
    }
}