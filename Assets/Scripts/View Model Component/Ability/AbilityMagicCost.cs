using UnityEngine;

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
        this.SubscribeToSender<AbilityCanPerformCheckEvent>(OnCanPerformCheck, owner);
        this.SubscribeToSender<AbilityDidPerformEvent>(OnDidPerformNotification, owner);
    }

    private void OnDisable()
    {
        this.UnsubscribeFromSender<AbilityCanPerformCheckEvent>(OnCanPerformCheck, owner);
        this.UnsubscribeFromSender<AbilityDidPerformEvent>(OnDidPerformNotification, owner);
    }

    #endregion

    #region Event Handlers

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var s = GetComponentInParent<Stats>();
        if (s[StatTypes.MP] < amount)
        {
            e.Exception.FlipToggle();
        }
    }

    private void OnDidPerformNotification(AbilityDidPerformEvent e)
    {
        var s = GetComponentInParent<Stats>();
        s[StatTypes.MP] -= amount;
    }

    #endregion
}