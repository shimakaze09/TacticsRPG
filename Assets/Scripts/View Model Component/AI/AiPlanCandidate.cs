/// <summary>
/// One immutable, fully-scored option for the tactical AI's turn: use this
/// ability, standing on this tile, aimed at this tile/direction, worth this
/// many points. Produced by AiCandidateGenerator, valued by AiPlanScorer,
/// chosen between by AiPlanSelector — candidates never change after creation,
/// so selection can never disturb generation results.
/// </summary>
public sealed class AiPlanCandidate
{
    /// <summary>The ability this candidate performs.</summary>
    public Ability Ability { get; }

    /// <summary>Where the actor stands to perform it.</summary>
    public Tile MoveTile { get; }

    /// <summary>The tile the ability is aimed at.</summary>
    public Tile FireTile { get; }

    /// <summary>Facing while performing (matters for directional abilities).</summary>
    public Directions Direction { get; }

    /// <summary>Total tactical value after every scoring adjustment.</summary>
    public float Score { get; }

    /// <summary>True when the predicted damage finishes a foe.</summary>
    public bool KillsTarget { get; }

    /// <summary>True when this candidate damages the team's focus target.</summary>
    public bool HitsFocus { get; }

    /// <summary>True when the actor fires from its current tile (the hit-and-run pool).</summary>
    public bool IsStationary { get; }

    /// <summary>Captures a scored option; all fields are fixed for the candidate's lifetime.</summary>
    public AiPlanCandidate(Ability ability, Tile moveTile, Tile fireTile, Directions direction,
        float score, bool killsTarget, bool hitsFocus, bool isStationary)
    {
        Ability = ability;
        MoveTile = moveTile;
        FireTile = fireTile;
        Direction = direction;
        Score = score;
        KillsTarget = killsTarget;
        HitsFocus = hitsFocus;
        IsStationary = isStationary;
    }
}
