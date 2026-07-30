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

    // Walks the unit along the chosen path, collects any remains waiting on
    // the destination tile, then returns to the command menu
    private IEnumerator Sequence()
    {
        var m = turn.actor.GetComponent<Movement>();
        yield return StartCoroutine(m.Traverse(owner.currentTile));
        turn.hasUnitMoved = true;

        var pickup = RemainsPickup.FindAt(turn.actor.tile);
        if (pickup != null)
            pickup.Collect(turn.actor);

        board.DeSelectTiles(tiles);
        ShowSelector();
        owner.ChangeState<CommandSelectionState>();
    }
}