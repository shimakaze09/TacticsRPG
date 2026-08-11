/// <summary>
/// Hard-difficulty AI, as a thin coordinator over three focused stages: a
/// per-turn snapshot (AiTurnContext) feeds the occupancy-safe candidate
/// enumeration (AiCandidateGenerator), each candidate is valued by the pure
/// scoring policy (AiPlanScorer), and the decision stage (AiPlanSelector)
/// turns the scored list into the committed plan — kills, focus fire,
/// hit-and-run, panic retreats, and healer discipline all live in those
/// stages. Where the Easy brain (ComputerPlayer) follows a scripted attack
/// pattern, this pipeline scores every usable (ability × move tile × fire
/// tile/direction) combination using the game's own Predict() and hit-chance
/// math, then executes the best plan.
/// </summary>
public class TacticalComputerPlayer : ComputerPlayer
{
    /// <summary>
    /// Builds this turn's plan through the snapshot → generate → score →
    /// select pipeline; when no stage produces a plan, advances on the
    /// nearest foe like the Easy brain.
    /// </summary>
    public override PlanOfAttack Evaluate()
    {
        var context = AiTurnContext.Build(bc, actor);
        var candidates = AiCandidateGenerator.Generate(context);
        var poa = new AiPlanSelector(context).Select(candidates);

        if (poa == null)
        {
            poa = new PlanOfAttack { actFirst = false };
            MoveTowardOpponent(poa);
        }

        return poa;
    }
}
