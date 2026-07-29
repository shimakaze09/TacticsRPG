using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Battle state: animates the unit walking the path chosen in MoveTargetState,
/// then returns to the command menu.
/// </summary>
public class MoveSequenceState : BattleState
{
    private readonly List<Tile> tiles = new();

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