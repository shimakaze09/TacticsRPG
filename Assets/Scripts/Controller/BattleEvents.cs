using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs a battle definition's scripted events — currently reinforcement
/// waves that arrive when the BattleClock reaches their round. The hook
/// point for future mid-battle story triggers. Lives on the BattleController.
/// </summary>
public class BattleEvents : MonoBehaviour
{
    private BattleController bc;
    private BattleClock clock;
    private Transform unitContainer;
    private readonly List<ReinforcementWave> pending = new List<ReinforcementWave>();

    /// <summary>Arms the component with the definition's wave list.</summary>
    public void Configure(BattleController controller, BattleDefinition definition, Transform container)
    {
        bc = controller;
        clock = controller.GetComponent<BattleClock>();
        unitContainer = container;
        pending.Clear();
        if (definition != null && definition.waves != null)
            pending.AddRange(definition.waves);
    }

    // Waves are checked after every completed activation
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
        if (bc == null || clock == null || pending.Count == 0)
            return;

        for (var i = pending.Count - 1; i >= 0; i--)
        {
            if (clock.CurrentRound < pending[i].round)
                continue;

            foreach (var entry in pending[i].spawns)
                BattleSpawner.Spawn(bc, entry, unitContainer);

            Debug.Log($"[BattleEvents] Reinforcements arrived (round {clock.CurrentRound}): {pending[i].spawns.Count} unit(s)");
            pending.RemoveAt(i);
        }
    }
}
