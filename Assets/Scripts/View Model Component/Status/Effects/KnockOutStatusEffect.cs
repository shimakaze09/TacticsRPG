using UnityEngine;
using System.Collections;

public class KnockOutStatusEffect : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void Awake()
    {
        owner = GetComponentInParent<Unit>();
        stats = owner.GetComponent<Stats>();
    }

    private void OnEnable()
    {
        owner.transform.localScale = new Vector3(0.75f, 0.1f, 0.75f);
        this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
        this.AddObserver(OnStatCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), stats);
    }

    private void OnDisable()
    {
        owner.transform.localScale = Vector3.one;
        this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
        this.RemoveObserver(OnStatCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), stats);
    }

    private void OnTurnCheck(object sender, object args)
    {
        // Dont allow a KO'd unit to take turns
        var exc = args as BaseException;
        if (exc.defaultToggle == true)
            exc.FlipToggle();
    }

    private void OnStatCounterWillChange(object sender, object args)
    {
        // Dont allow a KO'd unit to increment the turn order counter
        var exc = args as ValueChangeException;
        if (exc.toValue > exc.fromValue)
            exc.FlipToggle();
    }
}