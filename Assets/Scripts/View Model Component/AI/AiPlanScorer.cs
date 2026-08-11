using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The tactical AI's valuation policy: pure scoring rules that turn one
/// (ability, stand tile, aim tile, facing) combination into a point value
/// using the game's own Predict() and hit-chance math. Also home to the
/// shared role heuristics (healer detection, target-priority multipliers,
/// focus-target nomination, healer MP reserve) so the whole AI prices units
/// consistently. Holds no mutable state — identical inputs always score
/// identically, which is what makes the AI's choices deterministic.
/// </summary>
public sealed class AiPlanScorer
{
    #region Tuning

    /// <summary>Flat bonus for a plan whose predicted damage finishes a foe.</summary>
    public const float KillBonus = 60f;

    /// <summary>Extra value per fraction of HP the target is already missing (focus fire).</summary>
    public const float LowHealthFocusWeight = 12f;

    /// <summary>Multiplier on the penalty for catching own allies in a hostile area.</summary>
    public const float AllyHitPenaltyFactor = 1.25f;

    /// <summary>Value per point of HP actually restored by a heal.</summary>
    public const float HealWeight = 1.1f;

    /// <summary>Value of bringing a downed ally back.</summary>
    public const float ReviveValue = 80f;

    /// <summary>Value per removable status when cleansing an ally.</summary>
    public const float CleansePerStatus = 15f;

    /// <summary>Score discount per point of MP an ability costs.</summary>
    public const float MpCostWeight = 0.25f;

    /// <summary>How strongly destination danger discounts every plan.</summary>
    public const float ThreatPositionWeight = 0.15f;

    /// <summary>Healers weigh destination danger this many times harder than others.</summary>
    public const float HealerThreatMultiplier = 3f;

    /// <summary>Flat bonus for damaging the team's agreed focus target.</summary>
    public const float FocusAssistBonus = 18f;

    /// <summary>Tiny per-tile pull toward the focus target while picking firing positions.</summary>
    public const float FocusConvergeWeight = 0.3f;

    /// <summary>Target-value multiplier for enemy healers: support dies first.</summary>
    public const float HealerTargetMultiplier = 1.35f;

    /// <summary>Target-value multiplier for enemy casters.</summary>
    public const float CasterTargetMultiplier = 1.2f;

    /// <summary>Target-value multiplier for armored targets: tanks die last.</summary>
    public const float TankTargetMultiplier = 0.9f;

    /// <summary>Allies below this HP fraction count as "hurt" for the MP reserve rule.</summary>
    public const float HurtAllyFraction = 0.7f;

    private static readonly HashSet<string> BuffStatuses = new HashSet<string>
    {
        "Bulwark", "Firewall", "Knit", "Overclock", "Failsafe", "Nullgrav", "Ghosted"
    };

    private static readonly Dictionary<string, float> StatusValues = new Dictionary<string, float>
    {
        // hostile control / debuffs
        { "Graycast", 34f }, { "Swayed", 32f }, { "Blackout", 30f }, { "FreezeFrame", 30f },
        { "Deadline", 26f }, { "Scrambled", 22f }, { "DeadAir", 20f },
        { "Pinned", 16f }, { "Static", 16f }, { "Throttle", 14f },
        { "Sepsis", 12f }, { "Doused", 12f }, { "Redline", 12f },
        { "Desync", 10f }, { "Thirst", 10f },
        // friendly buffs
        { "Bulwark", 16f }, { "Firewall", 14f }, { "Knit", 14f },
        { "Overclock", 22f }, { "Failsafe", 18f }, { "Nullgrav", 6f }, { "Ghosted", 10f }
    };

    #endregion

    private readonly AiTurnContext context;

    /// <summary>Binds the rules to one turn's snapshot; the scorer itself stays stateless.</summary>
    public AiPlanScorer(AiTurnContext context)
    {
        this.context = context;
    }

    #region Candidate scoring

    /// <summary>
    /// Values one candidate combination. The actor must already be
    /// hypothetically placed on <paramref name="moveTile"/> (and facing
    /// <paramref name="direction"/> for directional abilities) so that
    /// range/hit/Predict math sees the position being evaluated. Returns the
    /// finished immutable candidate, or null when the combination affects
    /// nothing, helps the enemy, or violates the healer's MP reserve.
    /// </summary>
    public AiPlanCandidate Score(Ability ability, AbilityArea area, Tile moveTile, Tile fireTile, Directions direction)
    {
        var areaTiles = area.GetTilesInArea(context.Bc.board, fireTile.pos);

        // Cheap early-out: nothing to affect
        var hasContent = false;
        foreach (var t in areaTiles)
        {
            if (t.content != null)
            {
                hasContent = true;
                break;
            }
        }

        if (!hasContent)
            return null;

        var score = 0f;
        var kills = false;
        var focusHit = false;
        foreach (var tile in areaTiles)
            score += ScoreTile(ability, tile, ref kills, ref focusHit);

        if (score <= 0f)
            return null;

        var mpCost = ability.GetComponent<AbilityMagicCost>();
        if (mpCost != null)
            score -= mpCost.amount * MpCostWeight;

        // MP reserve: while allies are hurt (or down), a healer refuses any
        // non-support cast that would leave it unable to afford the
        // emergency heal/revive
        if (mpCost != null && context.IsActorHealer && context.EnforceHealReserve && context.HealReserveMp > 0 &&
            !IsSupportAbility(ability) && context.ActorMp - mpCost.amount < context.HealReserveMp)
            return null;

        // Prefer safer ground when plans are otherwise equal (healers weigh
        // danger several times harder — see ThreatWeight)
        score -= context.GetThreat(moveTile) * context.ThreatWeight;

        // Gentle pull toward the focus target: among near-equal firing
        // positions, end up closer to the team's kill-first pick
        if (context.FocusTarget != null && context.FocusTarget.tile != null)
        {
            var focusDistance = Mathf.Abs(moveTile.pos.x - context.FocusTarget.tile.pos.x) +
                                Mathf.Abs(moveTile.pos.y - context.FocusTarget.tile.pos.y);
            score -= focusDistance * FocusConvergeWeight;
        }

        return new AiPlanCandidate(ability, moveTile, fireTile, direction, score, kills, focusHit,
            moveTile == context.StartTile);
    }

    // Sums the value of every effect this ability lands on one tile's
    // occupant, each weighted by its hit chance
    private float ScoreTile(Ability ability, Tile tile, ref bool kills, ref bool focusHit)
    {
        if (tile.content == null)
            return 0f;

        var defender = tile.content.GetComponentInChildren<Unit>();
        if (defender == null)
            return 0f;

        var defStats = defender.GetComponent<Stats>();
        var defAlliance = defender.GetComponent<Alliance>();
        if (defStats == null || defAlliance == null)
            return 0f;

        var isFoe = context.Alliance.IsMatch(defAlliance, Targets.Foe);
        var isDown = defStats[StatTypes.HP] <= 0;

        var total = 0f;
        for (var i = 0; i < ability.transform.childCount; i++)
        {
            var child = ability.transform.GetChild(i);
            var effect = child.GetComponent<BaseAbilityEffect>();
            var targeter = child.GetComponent<AbilityEffectTarget>();
            var hitRate = child.GetComponent<HitRate>();
            if (effect == null || targeter == null || hitRate == null)
                continue;

            if (!targeter.IsTarget(tile))
                continue;

            var chance = Mathf.Clamp(hitRate.Calculate(tile), 0, 100) / 100f;
            if (chance <= 0f)
                continue;

            total += ScoreEffect(effect, tile, defender, defStats, isFoe, isDown, ref kills, ref focusHit) * chance;
        }

        return total;
    }

    // Values a single effect against a single unit: damage by HP actually
    // removed (kills and focus fire bonused), heals by HP restored scaled by
    // urgency, statuses by tactical worth
    private float ScoreEffect(BaseAbilityEffect effect, Tile tile, Unit defender, Stats defStats, bool isFoe,
        bool isDown, ref bool kills, ref bool focusHit)
    {
        switch (effect)
        {
            case DamageAbilityEffect _ when isDown:
                return 0f;

            case DamageAbilityEffect _:
            {
                var damage = -effect.Predict(tile);
                if (!isFoe)
                    return -damage * AllyHitPenaltyFactor;

                var hp = defStats[StatTypes.HP];
                if (damage >= hp)
                    kills = true;
                float value = Mathf.Min(damage, hp); // overkill isn't worth extra
                if (damage >= hp)
                    value += KillBonus;
                value += (1f - hp / (float)Mathf.Max(1, defStats[StatTypes.MHP])) * LowHealthFocusWeight;

                // Support dies first; damage on the team's focus target earns
                // the assist bonus that makes the team converge
                value *= RoleMultiplier(defender);
                if (defender == context.FocusTarget)
                {
                    value += FocusAssistBonus;
                    focusHit = true;
                }

                return value;
            }

            case HealAbilityEffect _ when isDown:
                return 0f;

            case HealAbilityEffect _:
            {
                var missing = defStats[StatTypes.MHP] - defStats[StatTypes.HP];
                var healed = Mathf.Min(effect.Predict(tile), missing);
                if (isFoe)
                    return -healed;

                // Triage: the closer an ally is to death, the more the same
                // heal is worth — stabilize the critical before topping off
                var urgency = 2f - defStats[StatTypes.HP] / (float)Mathf.Max(1, defStats[StatTypes.MHP]);
                return healed * HealWeight * urgency;
            }

            case ReviveAbilityEffect _:
                return isDown && !isFoe ? ReviveValue : 0f;

            case CleanseAbilityEffect _ when !isFoe && !isDown:
                return CountRemovableStatuses(defender) * CleansePerStatus;

            case InflictAbilityEffect inflict when !isDown:
                return ScoreStatus(inflict.statusName, defender, defStats, isFoe);

            default:
                return 0f;
        }
    }

    // Tactical worth of landing a status: only statuses that exist in the
    // StatusRegistry are valued (an unregistered name would silently no-op
    // when inflicted, so it must score zero), re-applying an active status is
    // worthless, and silencing is amplified against real casters
    private float ScoreStatus(string statusName, Unit defender, Stats defStats, bool isFoe)
    {
        var type = StatusRegistry.Resolve(statusName);
        if (type == null)
        {
            Debug.LogWarning($"[TacticalAI] Ability inflicts unknown status '{statusName}' — scoring it 0");
            return 0f;
        }

        if (defender.GetComponentInChildren(type) != null)
            return 0f;

        if (!StatusValues.TryGetValue(statusName, out var value))
            value = 10f;

        if (statusName == "DeadAir" && defStats[StatTypes.MMP] >= 20)
            value *= 1.6f;

        var isBuff = BuffStatuses.Contains(statusName);
        if (isBuff)
            return isFoe ? 0f : value;

        return isFoe ? value : -value;
    }

    // How many duration-based statuses a cleanse could strip from a unit
    private int CountRemovableStatuses(Unit unit)
    {
        var status = unit.GetComponentInChildren<Status>();
        return status != null ? status.GetComponentsInChildren<DurationStatusCondition>().Length : 0;
    }

    #endregion

    #region Shared role policy

    /// <summary>True when the unit carries any heal or revive ability.</summary>
    public static bool IsHealer(Unit unit)
    {
        foreach (var ability in unit.GetComponentsInChildren<Ability>())
        {
            if (ability.GetComponentInChildren<HealAbilityEffect>() != null ||
                ability.GetComponentInChildren<ReviveAbilityEffect>() != null)
                return true;
        }

        return false;
    }

    /// <summary>True for abilities that heal, revive, or cleanse — the casts the MP reserve protects.</summary>
    public static bool IsSupportAbility(Ability ability)
    {
        return ability.GetComponentInChildren<HealAbilityEffect>() != null ||
               ability.GetComponentInChildren<ReviveAbilityEffect>() != null ||
               ability.GetComponentInChildren<CleanseAbilityEffect>() != null;
    }

    /// <summary>Target priority by role: support first, armor last.</summary>
    public static float RoleMultiplier(Unit unit)
    {
        if (IsHealer(unit))
            return HealerTargetMultiplier;

        var stats = unit.GetComponent<Stats>();
        if (stats == null)
            return 1f;
        if (stats[StatTypes.MAT] > stats[StatTypes.ATK])
            return CasterTargetMultiplier;
        if (stats[StatTypes.DEF] >= stats[StatTypes.ATK])
            return TankTargetMultiplier;
        return 1f;
    }

    /// <summary>
    /// The team's kill-first pick: highest target value among living foes,
    /// where value = role weight (healer > caster > striker > tank) scaled by
    /// wounded-ness and by whether this team can realistically finish them.
    /// Deterministic from battle state, so every teammate agrees. Never
    /// forces reach — units out of range simply attack who they can and
    /// converge with their movement instead.
    /// </summary>
    public static Unit NominateFocusTarget(BattleController bc, Unit actor, Alliance alliance)
    {
        Unit best = null;
        var bestValue = 0f;

        var teamHit = Mathf.Max(1f, ThreatMap.EstimateDamage(actor));
        foreach (var mate in bc.units)
        {
            if (mate == null || mate == actor)
                continue;
            var mateAlliance = mate.GetComponent<Alliance>();
            if (mateAlliance == null || !alliance.IsMatch(mateAlliance, Targets.Ally))
                continue;
            teamHit = Mathf.Max(teamHit, ThreatMap.EstimateDamage(mate));
        }

        foreach (var foe in bc.units)
        {
            if (foe == null || foe.tile == null)
                continue;

            var foeAlliance = foe.GetComponent<Alliance>();
            if (foeAlliance == null || !alliance.IsMatch(foeAlliance, Targets.Foe))
                continue;

            var stats = foe.GetComponent<Stats>();
            if (stats == null || stats[StatTypes.HP] <= 0)
                continue;

            var woundedness = 1f - stats[StatTypes.HP] / (float)Mathf.Max(1, stats[StatTypes.MHP]);
            var feasibility = Mathf.Clamp(teamHit * 2f / Mathf.Max(1, stats[StatTypes.HP]), 0.3f, 1.5f);
            var value = RoleMultiplier(foe) * (0.6f + woundedness) * feasibility;

            if (value > bestValue)
            {
                bestValue = value;
                best = foe;
            }
        }

        return best;
    }

    /// <summary>
    /// Computes the healer MP-reserve for one turn: while any ally is hurt
    /// (or a revivable ally is down), the healer must keep enough MP for its
    /// emergency support cast — revive cost when someone is down, otherwise
    /// the cheapest heal.
    /// </summary>
    public static void ComputeHealReserve(BattleController bc, Unit actor, Alliance alliance, bool isActorHealer,
        out int actorMp, out int healReserveMp, out bool enforceHealReserve)
    {
        healReserveMp = 0;
        enforceHealReserve = false;
        var stats = actor.GetComponent<Stats>();
        actorMp = stats != null ? stats[StatTypes.MP] : 0;

        if (!isActorHealer)
            return;

        var anyHurt = false;
        var anyDown = false;
        foreach (var other in bc.units)
        {
            if (other == null || other.tile == null)
                continue;

            var otherAlliance = other.GetComponent<Alliance>();
            if (otherAlliance == null || !alliance.IsMatch(otherAlliance, Targets.Ally))
                continue;

            var otherStats = other.GetComponent<Stats>();
            if (otherStats == null)
                continue;

            if (otherStats[StatTypes.HP] <= 0)
                anyDown = true;
            else if (otherStats[StatTypes.HP] < otherStats[StatTypes.MHP] * HurtAllyFraction)
                anyHurt = true;
        }

        if (!anyHurt && !anyDown)
            return;

        var reserve = int.MaxValue;
        foreach (var ability in actor.GetComponentsInChildren<Ability>())
        {
            var isRevive = ability.GetComponentInChildren<ReviveAbilityEffect>() != null;
            var isHeal = ability.GetComponentInChildren<HealAbilityEffect>() != null;
            if (anyDown ? !isRevive : !isHeal)
                continue;

            var cost = ability.GetComponent<AbilityMagicCost>();
            reserve = Mathf.Min(reserve, cost != null ? cost.amount : 0);
        }

        if (reserve == int.MaxValue)
            return;

        healReserveMp = reserve;
        enforceHealReserve = true;
    }

    #endregion
}
