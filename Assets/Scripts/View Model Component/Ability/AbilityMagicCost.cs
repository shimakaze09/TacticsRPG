using UnityEngine;
using System.Collections;

public class AbilityMagicCost : MonoBehaviour
{
    #region Fields

    public int amount;
    private Ability owner;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        owner = GetComponent<Ability>();
    }

    private void OnEnable()
    {
        this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
        this.AddObserver(OnDidPerformNotification, Ability.DidPerformNotification, owner);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
        this.RemoveObserver(OnDidPerformNotification, Ability.DidPerformNotification, owner);
    }

    #endregion

    #region Notification Handlers

    private void OnCanPerformCheck(object sender, object args)
    {
        var s = GetComponentInParent<Stats>();
        if (s[StatTypes.MP] < amount)
        {
            var exc = (BaseException)args;
            exc.FlipToggle();
        }
    }

    private void OnDidPerformNotification(object sender, object args)
    {
        var s = GetComponentInParent<Stats>();
        s[StatTypes.MP] -= amount;
    }

    #endregion
}