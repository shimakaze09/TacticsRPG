using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnOrderController : MonoBehaviour
{
    #region Public

    public IEnumerator Round()
    {
        var bc = GetComponent<BattleController>();

        while (true)
        {
            // Publish round began event
            this.Publish(new RoundBeganEvent());

            var units = new List<Unit>(bc.units);
            foreach (var stats in units.Select(unit => unit.GetComponent<Stats>()))
                stats[StatTypes.CTR] += stats[StatTypes.SPD];

            units.Sort((a, b) => GetCounter(a).CompareTo(GetCounter(b)));

            for (var i = units.Count - 1; i >= 0; i--)
                if (CanTakeTurn(units[i]))
                {
                    bc.turn.Change(units[i]);

                    // Publish turn began event from the unit
                    units[i].Publish(new TurnBeganEvent(units[i]));

                    yield return units[i];

                    var cost = turnCost;
                    if (bc.turn.hasUnitMoved)
                        cost += moveCost;
                    if (bc.turn.hasUnitActed)
                        cost += actionCost;

                    var s = units[i].GetComponent<Stats>();
                    s.SetValue(StatTypes.CTR, s[StatTypes.CTR] - cost, false);

                    // Publish turn completed event from the unit
                    units[i].Publish(new TurnCompletedEvent(units[i]));
                }

            // Publish round ended event
            this.Publish(new RoundEndedEvent());
        }
    }

    #endregion

    #region Constants

    private const int turnActivation = 1000;
    private const int turnCost = 500;
    private const int moveCost = 300;
    private const int actionCost = 200;

    #endregion



    #region Private

    private bool CanTakeTurn(Unit target)
    {
        var exc = new BaseException(GetCounter(target) >= turnActivation);
        // Publish turn check event from the unit
        target.Publish(new TurnCheckEvent(target, exc));
        return exc.toggle;
    }

    private int GetCounter(Unit target)
    {
        return target.GetComponent<Stats>()[StatTypes.CTR];
    }

    #endregion
}