using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConfirmAbilityTargetState : BattleState
{
    private List<Tile> tiles;
    private AbilityArea aa;
    private int index;

    public override void Enter()
    {
        base.Enter();
        aa = turn.ability.GetComponent<AbilityArea>();
        tiles = aa.GetTilesInArea(board, pos);
        board.ConfirmTiles(tiles);
        FindTargets();
        RefreshPrimaryStatPanel(turn.actor.tile.pos);
        if (turn.targets.Count > 0)
        {
            if (driver.Current == Drivers.Human)
                hitSuccessIndicator.Show();
            SetTarget(0);
        }

        if (driver.Current == Drivers.Computer)
            StartCoroutine(ComputerDisplayAbilitySelection());
    }

    public override void Exit()
    {
        base.Exit();
        board.DeSelectTiles(tiles);
        statPanelController.HidePrimary();
        statPanelController.HideSecondary();
        hitSuccessIndicator.Hide();
    }

    protected override void OnMove(object sender, InfoEventArgs<Point> e)
    {
        if (e.info.y > 0 || e.info.x > 0)
            SetTarget(index + 1);
        else
            SetTarget(index - 1);
    }

    protected override void OnFire(object sender, InfoEventArgs<int> e)
    {
        if (e.info == 0)
        {
            if (turn.targets.Count > 0) owner.ChangeState<PerformAbilityState>();
        }
        else
        {
            owner.ChangeState<AbilityTargetState>();
        }
    }

    private void FindTargets()
    {
        turn.targets = new List<Tile>();
        foreach (var tile in tiles.Where(t => turn.ability.IsTarget(t)))
            turn.targets.Add(tile);
    }

    private void SetTarget(int target)
    {
        index = target;
        if (index < 0)
            index = turn.targets.Count - 1;
        if (index >= turn.targets.Count)
            index = 0;

        if (turn.targets.Count > 0)
        {
            RefreshSecondaryStatPanel(turn.targets[index].pos);
            SelectTile(turn.targets[index].pos);
            UpdateHitSuccessIndicator();
        }
    }

    private void UpdateHitSuccessIndicator()
    {
        var chance = 0;
        var amount = 0;
        var target = turn.targets[index];

        var obj = turn.ability.transform;
        for (var i = 0; i < obj.childCount; i++)
        {
            var targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
            if (targeter.IsTarget(target))
            {
                var hitRate = targeter.GetComponent<HitRate>();
                chance = hitRate.Calculate(target);

                var effect = targeter.GetComponent<BaseAbilityEffect>();
                amount = effect.Predict(target);
                break;
            }
        }

        hitSuccessIndicator.SetStats(chance, amount);
    }

    private IEnumerator ComputerDisplayAbilitySelection()
    {
        owner.battleMessageController.Display(turn.ability.name);
        yield return new WaitForSeconds(2f);
        owner.ChangeState<PerformAbilityState>();
    }
}