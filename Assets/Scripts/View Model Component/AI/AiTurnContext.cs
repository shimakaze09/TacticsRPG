using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The tactical AI's per-turn snapshot: everything Evaluate needs that is
/// fixed for the duration of one planning pass — the acting unit, its
/// reachable tiles, the threat map, the team's focus target, and the healer
/// discipline state (MP reserve). Built once per turn while the actor is on
/// its real tile, then treated as read-only by the generator, scorer, and
/// selector, so every stage sees the identical world.
/// </summary>
public sealed class AiTurnContext
{
    /// <summary>The battle this plan is for.</summary>
    public BattleController Bc { get; }

    /// <summary>The unit whose turn is being planned.</summary>
    public Unit Actor { get; }

    /// <summary>The actor's side, for foe/ally matching.</summary>
    public Alliance Alliance { get; }

    /// <summary>The actor's real tile at planning time.</summary>
    public Tile StartTile { get; }

    /// <summary>The actor's real facing at planning time.</summary>
    public Directions StartDir { get; }

    /// <summary>Every tile the actor may end its move on (its own tile included).</summary>
    public IReadOnlyList<Tile> MoveOptions { get; }

    /// <summary>Per-tile expected incoming damage, from the actor's point of view.</summary>
    public ThreatMap Threat { get; }

    /// <summary>How strongly destination danger discounts a plan (amplified for healers).</summary>
    public float ThreatWeight { get; }

    /// <summary>The team's agreed kill-first target, or null when no foe stands.</summary>
    public Unit FocusTarget { get; }

    /// <summary>True when the actor carries any heal or revive ability.</summary>
    public bool IsActorHealer { get; }

    /// <summary>The actor's current MP, read once for the MP-reserve rule.</summary>
    public int ActorMp { get; }

    /// <summary>MP a healer must keep free for its emergency support cast.</summary>
    public int HealReserveMp { get; }

    /// <summary>True while allies are hurt or down, activating the MP reserve.</summary>
    public bool EnforceHealReserve { get; }

    // Snapshot construction goes through Build so every field is captured
    // consistently from the same board state
    private AiTurnContext(BattleController bc, Unit actor, Alliance alliance, List<Tile> moveOptions,
        ThreatMap threat, float threatWeight, Unit focusTarget, bool isActorHealer,
        int actorMp, int healReserveMp, bool enforceHealReserve)
    {
        Bc = bc;
        Actor = actor;
        Alliance = alliance;
        StartTile = actor.tile;
        StartDir = actor.dir;
        MoveOptions = moveOptions;
        Threat = threat;
        ThreatWeight = threatWeight;
        FocusTarget = focusTarget;
        IsActorHealer = isActorHealer;
        ActorMp = actorMp;
        HealReserveMp = healReserveMp;
        EnforceHealReserve = enforceHealReserve;
    }

    /// <summary>
    /// Captures the planning snapshot for one unit's turn: threat map, focus
    /// nomination, healer MP reserve, and move options, all computed with the
    /// actor standing on its real tile.
    /// </summary>
    public static AiTurnContext Build(BattleController bc, Unit actor)
    {
        var alliance = actor.GetComponent<Alliance>();
        var threat = ThreatMap.Build(bc, actor);
        var isHealer = AiPlanScorer.IsHealer(actor);
        var threatWeight = isHealer
            ? AiPlanScorer.ThreatPositionWeight * AiPlanScorer.HealerThreatMultiplier
            : AiPlanScorer.ThreatPositionWeight;
        var focusTarget = AiPlanScorer.NominateFocusTarget(bc, actor, alliance);
        AiPlanScorer.ComputeHealReserve(bc, actor, alliance, isHealer,
            out var actorMp, out var healReserveMp, out var enforceHealReserve);
        var moveOptions = ComputeMoveOptions(bc, actor);
        return new AiTurnContext(bc, actor, alliance, moveOptions, threat, threatWeight,
            focusTarget, isHealer, actorMp, healReserveMp, enforceHealReserve);
    }

    /// <summary>
    /// Every tile a unit may end its move on: its movement range plus its own
    /// tile (an immobilized unit still "moves" to where it stands). Shared
    /// with the Easy AI so both brains agree on reachability.
    /// </summary>
    public static List<Tile> ComputeMoveOptions(BattleController bc, Unit actor)
    {
        var movement = actor.GetComponent<Movement>();
        if (movement == null || !movement.CanMove())
            return new List<Tile> { actor.tile };

        var options = movement.GetTilesInRange(bc.board);
        options.Add(actor.tile);
        return options;
    }

    /// <summary>Expected next-turn damage on a tile (0 when unthreatened).</summary>
    public float GetThreat(Tile tile)
    {
        return Threat != null ? Threat.GetThreat(tile) : 0f;
    }

    /// <summary>
    /// Manhattan distance from a tile to the closest living foe, considering
    /// ALL foes — retreating from one enemy must not walk into another.
    /// </summary>
    public int DistanceToNearestFoe(Tile tile)
    {
        if (tile == null)
            return int.MaxValue;

        var closest = int.MaxValue;
        foreach (var other in Bc.units)
        {
            if (other == null || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !Alliance.IsMatch(otherAlliance, Targets.Foe))
                continue;

            var stats = other.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var distance = Mathf.Abs(tile.pos.x - other.tile.pos.x) +
                           Mathf.Abs(tile.pos.y - other.tile.pos.y);
            closest = Mathf.Min(closest, distance);
        }

        return closest;
    }

    /// <summary>
    /// Reachable tile with the lowest expected incoming damage; ties broken
    /// by max-min distance to all living foes.
    /// </summary>
    public Tile SafestMoveTile()
    {
        var bestTile = Actor.tile;
        var bestThreat = float.MaxValue;
        var bestDistance = int.MinValue;
        foreach (var tile in MoveOptions)
        {
            var tileThreat = GetThreat(tile);
            var distance = DistanceToNearestFoe(tile);
            if (tileThreat < bestThreat ||
                (Mathf.Approximately(tileThreat, bestThreat) && distance > bestDistance))
            {
                bestThreat = tileThreat;
                bestDistance = distance;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    /// <summary>
    /// True when moving to <paramref name="candidate"/> is genuinely safer
    /// than staying on <paramref name="current"/>: lower expected damage,
    /// or equal damage but farther from every foe.
    /// </summary>
    public bool IsSafer(Tile candidate, Tile current)
    {
        var candidateThreat = GetThreat(candidate);
        var currentThreat = GetThreat(current);

        if (candidateThreat < currentThreat)
            return true;
        return Mathf.Approximately(candidateThreat, currentThreat) &&
               DistanceToNearestFoe(candidate) > DistanceToNearestFoe(current);
    }
}
