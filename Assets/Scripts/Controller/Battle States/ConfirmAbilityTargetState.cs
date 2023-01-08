using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConfirmAbilityTargetState : BattleState
{
    private List<Tile> tiles;
    private AbilityArea aa;
    private int index = 0;

    public override void Enter()
    {
        base.Enter();
        aa = turn.ability.GetComponent<AbilityArea>();
        tiles = aa.GetTilesInArea(board, pos);
        board.SelectTiles(tiles);
        FindTargets();
        RefreshPrimaryStatPanel(turn.actor.tile.pos);

        if (turn.targets.Count > 0)
        {
            hitSuccessIndicator.Show();
            SetTarget(0);
        }
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
        var targeters = turn.ability.GetComponentsInChildren<AbilityEffectTarget>();
        foreach (var tile in tiles.Where(tile => IsTarget(tile, targeters)))
            turn.targets.Add(tile);
    }

    private bool IsTarget(Tile tile, AbilityEffectTarget[] list)
    {
        return list.Any(t => t.IsTarget(tile));
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
            UpdateHitSuccessIndicator();
        }
    }

    private void UpdateHitSuccessIndicator()
    {
        var chance = CalculateHitRate();
        var amount = EstimateDamage();
        hitSuccessIndicator.SetStats(chance, amount);
    }

    private int CalculateHitRate()
    {
        var target = turn.targets[index].content.GetComponent<Unit>();
        var hr = turn.ability.GetComponentInChildren<HitRate>();
        return hr.Calculate(turn.actor, target);
    }

    private int EstimateDamage()
    {
        return 50;
    }
}