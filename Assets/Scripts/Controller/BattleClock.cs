using UnityEngine;

/// <summary>
/// Tracks the battle's "round" in a designer-friendly way: with a CTR
/// scheduler there is no natural round boundary, so a round is defined as
/// (completed unit turns / units at battle start). Reinforcement waves and
/// survive-N-rounds victory both read this. Lives on the BattleController.
/// </summary>
public class BattleClock : MonoBehaviour
{
    /// <summary>Completed unit activations since battle start.</summary>
    public int TurnsCompleted { get; private set; }

    /// <summary>Current battle round, starting at 1.</summary>
    public int CurrentRound => 1 + TurnsCompleted / Mathf.Max(1, startingUnitCount);

    private int startingUnitCount = 1;

    /// <summary>Snapshot the unit count that defines a round's length.</summary>
    public void Configure(int unitsAtStart)
    {
        startingUnitCount = Mathf.Max(1, unitsAtStart);
    }

    // Counts every completed activation battle-wide
    private void OnEnable()
    {
        this.Subscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    private void OnTurnCompleted(TurnCompletedEvent e)
    {
        TurnsCompleted++;
    }
}
