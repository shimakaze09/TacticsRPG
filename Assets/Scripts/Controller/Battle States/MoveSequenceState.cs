using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveSequenceState : BattleState
{
    private List<Tile> tiles = new();

    public override void Enter()
    {
        base.Enter();
        tiles.Clear();
        tiles.Add(board.GetTile(pos));
        board.ConfirmTiles(tiles);
        HideSelector();
        StartCoroutine(nameof(Sequence));
    }

    private IEnumerator Sequence()
    {
        var m = turn.actor.GetComponent<Movement>();
        yield return StartCoroutine(m.Traverse(owner.currentTile));
        turn.hasUnitMoved = true;
        board.DeSelectTiles(tiles);
        ShowSelector();
        owner.ChangeState<CommandSelectionState>();
    }
}