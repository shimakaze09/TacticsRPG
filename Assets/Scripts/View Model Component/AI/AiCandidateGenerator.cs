using System.Collections.Generic;

/// <summary>
/// Enumerates every plan the tactical AI could take this turn: for each
/// usable ability, each reachable stand tile, and each aim tile (or facing,
/// for directional abilities), asks the scorer to value the combination and
/// collects the survivors as immutable candidates. Hypothetical positions are
/// applied through an AiPlacementScope, so the board's real occupancy is
/// guaranteed to be restored even if evaluation throws. Enumeration order is
/// deterministic (ability order, then move-option order, then fire order),
/// which is what keeps plan selection stable across identical inputs.
/// </summary>
public static class AiCandidateGenerator
{
    /// <summary>
    /// Produces the scored candidate list for the context's actor. The actor
    /// finishes exactly where it started — generation never leaves a mark on
    /// the board.
    /// </summary>
    public static List<AiPlanCandidate> Generate(AiTurnContext context)
    {
        var scorer = new AiPlanScorer(context);
        var candidates = new List<AiPlanCandidate>();
        var actor = context.Actor;

        using (var scope = new AiPlacementScope(actor))
        {
            foreach (var ability in CollectUsableAbilities(actor))
            {
                var range = ability.GetComponent<AbilityRange>();
                var area = ability.GetComponent<AbilityArea>();
                if (range == null || area == null)
                    continue;

                if (!range.positionOriented)
                {
                    // Infinite-style ranges: fire options don't depend on
                    // where we stand, but the scorer's live-placement
                    // precondition still does — hit rates and predictions
                    // read the caster's actual tile/facing, so stand on the
                    // committed tile before scoring (PR #95 review)
                    var safeTile = context.SafestMoveTile();
                    scope.MoveTo(safeTile);
                    var fireTiles = range.GetTilesInRange(context.Bc.board);
                    foreach (var fireTile in fireTiles)
                        Collect(candidates, scorer.Score(ability, area, safeTile, fireTile, context.StartDir));

                    scope.Restore();
                }
                else if (range.directionOriented)
                {
                    // Line/cone style: the fire "location" is our own tile; the
                    // direction is what matters.
                    foreach (var moveTile in context.MoveOptions)
                    {
                        scope.MoveTo(moveTile);
                        for (var d = 0; d < 4; ++d)
                        {
                            scope.Face((Directions)d);
                            Collect(candidates, scorer.Score(ability, area, moveTile, moveTile, (Directions)d));
                        }
                    }

                    scope.Restore();
                }
                else
                {
                    foreach (var moveTile in context.MoveOptions)
                    {
                        scope.MoveTo(moveTile);
                        var fireTiles = range.GetTilesInRange(context.Bc.board);
                        foreach (var fireTile in fireTiles)
                            Collect(candidates, scorer.Score(ability, area, moveTile, fireTile, actor.dir));
                    }

                    scope.Restore();
                }
            }
        }

        return candidates;
    }

    // Every ability on the actor that is currently performable
    private static List<Ability> CollectUsableAbilities(Unit actor)
    {
        var result = new List<Ability>();
        foreach (var ability in actor.GetComponentsInChildren<Ability>())
        {
            if (ability.CanPerform())
                result.Add(ability);
        }

        return result;
    }

    // Keeps only combinations the scorer found worth anything
    private static void Collect(List<AiPlanCandidate> candidates, AiPlanCandidate candidate)
    {
        if (candidate != null)
            candidates.Add(candidate);
    }
}
