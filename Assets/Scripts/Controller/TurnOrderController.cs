using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnOrderController : MonoBehaviour
{
    #region Constants

    private const int turnActivation = 1000;
    private const int turnCost = 500;
    private const int moveCost = 300;
    private const int actionCost = 200;

    #endregion

    #region Notifications

    public const string RoundBeganNotification = "TurnOrderController.roundBegan";
    public const string TurnCheckNotification = "TurnOrderController.turnCheck";
    public const string TurnBeganNotification = "TurnOrderController.TurnBeganNotification";
    public const string TurnCompletedNotification = "TurnOrderController.turnCompleted";
    public const string RoundEndedNotification = "TurnOrderController.roundEnded";

    #endregion

    #region Public

    public IEnumerator Round()
    {
        var bc = GetComponent<BattleController>();

        while (true)
        {
            this.PostNotification(RoundBeganNotification);

            var units = new List<Unit>(bc.units);
            foreach (var stats in units.Select(unit => unit.GetComponent<Stats>()))
                stats[StatTypes.CTR] += stats[StatTypes.SPD];

            units.Sort((a, b) => GetCounter(a).CompareTo(GetCounter(b)));

            for (var i = units.Count - 1; i >= 0; i--)
                if (CanTakeTurn(units[i]))
                {
                    bc.turn.Change(units[i]);
                    units[i].PostNotification(TurnBeganNotification);

                    yield return units[i];

                    var cost = turnCost;
                    if (bc.turn.hasUnitMoved)
                        cost += moveCost;
                    if (bc.turn.hasUnitActed)
                        cost += actionCost;

                    var s = units[i].GetComponent<Stats>();
                    s.SetValue(StatTypes.CTR, s[StatTypes.CTR] - cost, false);

                    units[i].PostNotification(TurnCompletedNotification);
                }

            this.PostNotification(RoundEndedNotification);
        }
    }

    #endregion

    #region Private

    private bool CanTakeTurn(Unit target)
    {
        var exc = new BaseException(GetCounter(target) >= turnActivation);
        target.PostNotification(TurnCheckNotification, exc);
        return exc.toggle;
    }

    private int GetCounter(Unit target)
    {
        return target.GetComponent<Stats>()[StatTypes.CTR];
    }

    #endregion
}