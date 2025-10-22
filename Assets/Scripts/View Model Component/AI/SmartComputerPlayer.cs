using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Smart AI that thinks like a player, evaluating tactical situations and making optimal decisions
/// Works with the new data-driven job/ability system
/// </summary>
public class SmartComputerPlayer : MonoBehaviour
{
    #region MonoBehaviour

    private void Awake()
    {
        bc = GetComponent<BattleController>();
        tacticalEvaluator = new TacticalEvaluator();
    }

    #endregion

    #region Public

    public PlanOfAttack Evaluate()
    {
        var poa = new PlanOfAttack();
        
        // Get current unit and available abilities
        var unit = actor;
        var availableAbilities = GetAvailableAbilities(unit);
        
        if (availableAbilities.Count == 0)
        {
            Debug.LogWarning($"No abilities available for {unit.name}");
            return poa;
        }

        // Evaluate all possible actions and pick the best one
        var bestAction = EvaluateAllActions(unit, availableAbilities);
        
        if (bestAction != null)
        {
            poa.ability = bestAction.ability;
            poa.target = bestAction.targetType;
            poa.moveLocation = bestAction.moveLocation;
            poa.fireLocation = bestAction.fireLocation;
            poa.attackDirection = bestAction.attackDirection;
        }
        else
        {
            // Fallback: move toward nearest enemy
            MoveTowardNearestEnemy(poa);
        }

        return poa;
    }

    #endregion

    #region Fields

    private BattleController bc;
    private Unit actor => bc.turn.actor;
    private Alliance alliance => actor.GetComponent<Alliance>();
    private TacticalEvaluator tacticalEvaluator;

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets all abilities available to the unit from their job catalog
    /// </summary>
    private List<Ability> GetAvailableAbilities(Unit unit)
    {
        var abilities = new List<Ability>();
        var catalog = unit.GetComponentInChildren<AbilityCatalog>();
        
        if (catalog == null)
        {
            Debug.LogWarning($"No ability catalog found for {unit.name}");
            return abilities;
        }

        // Get all abilities from the catalog
        for (int i = 0; i < catalog.transform.childCount; i++)
        {
            var category = catalog.transform.GetChild(i);
            for (int j = 0; j < category.childCount; j++)
            {
                var abilityObj = category.GetChild(j);
                var ability = abilityObj.GetComponent<Ability>();
                if (ability != null)
                {
                    abilities.Add(ability);
                }
            }
        }

        return abilities;
    }

    /// <summary>
    /// Evaluates all possible actions and returns the best one
    /// </summary>
    private TacticalAction EvaluateAllActions(Unit unit, List<Ability> abilities)
    {
        var allActions = new List<TacticalAction>();
        
        // Evaluate each ability
        foreach (var ability in abilities)
        {
            var actions = EvaluateAbility(unit, ability);
            allActions.AddRange(actions);
        }

        // Sort by tactical score (highest first)
        allActions.Sort((a, b) => b.tacticalScore.CompareTo(a.tacticalScore));

        // Return the best action if it's worth doing
        if (allActions.Count > 0 && allActions[0].tacticalScore > 0)
        {
            return allActions[0];
        }

        return null;
    }

    /// <summary>
    /// Evaluates all possible uses of a specific ability
    /// </summary>
    private List<TacticalAction> EvaluateAbility(Unit unit, Ability ability)
    {
        var actions = new List<TacticalAction>();
        var moveOptions = GetMoveOptions(unit);
        var range = ability.GetComponent<AbilityRange>();
        var area = ability.GetComponent<AbilityArea>();

        if (range == null || area == null)
        {
            Debug.LogWarning($"Ability {ability.name} missing range or area component");
            return actions;
        }

        // Try each possible move position
        foreach (var moveTile in moveOptions)
        {
            // Temporarily place unit to evaluate from this position
            var originalTile = unit.tile;
            unit.Place(moveTile);

            // Get all possible fire locations from this position
            var fireOptions = range.GetTilesInRange(bc.board);
            
            foreach (var fireTile in fireOptions)
            {
                // Try each possible direction
                for (int dir = 0; dir < 4; dir++)
                {
                    unit.dir = (Directions)dir;
                    
                    var action = EvaluateSpecificAction(unit, ability, moveTile, fireTile, (Directions)dir);
                    if (action != null)
                    {
                        actions.Add(action);
                    }
                }
            }

            // Restore original position
            unit.Place(originalTile);
        }

        return actions;
    }

    /// <summary>
    /// Evaluates a specific action (ability + move + fire + direction)
    /// </summary>
    private TacticalAction EvaluateSpecificAction(Unit unit, Ability ability, Tile moveTile, Tile fireTile, Directions direction)
    {
        var action = new TacticalAction
        {
            ability = ability,
            moveLocation = new Point(moveTile.pos.x, moveTile.pos.y),
            fireLocation = new Point(fireTile.pos.x, fireTile.pos.y),
            attackDirection = direction
        };

        var area = ability.GetComponent<AbilityArea>();
        var affectedTiles = area.GetTilesInArea(bc.board, fireTile.pos);
        
        // Calculate tactical score based on multiple factors
        action.tacticalScore = CalculateTacticalScore(unit, ability, affectedTiles, moveTile);

        // Determine target type
        action.targetType = DetermineTargetType(ability, affectedTiles);

        return action;
    }

    /// <summary>
    /// Calculates the tactical value of an action
    /// </summary>
    private float CalculateTacticalScore(Unit unit, Ability ability, List<Tile> affectedTiles, Tile moveTile)
    {
        float score = 0f;
        
        // Get unit stats for calculations
        var stats = unit.GetComponent<Stats>();
        var unitHP = stats[StatTypes.HP];
        var unitMP = stats[StatTypes.MP];
        
        // Check if we can afford the MP cost
        var magicCost = ability.GetComponent<AbilityMagicCost>();
        if (magicCost != null && magicCost.amount > unitMP)
        {
            return -1000f; // Can't afford this ability
        }

        // Evaluate each affected tile
        foreach (var tile in affectedTiles)
        {
            if (tile.content == null) continue;

            var targetUnit = tile.content.GetComponent<Unit>();
            if (targetUnit == null) continue;

            var targetStats = targetUnit.GetComponent<Stats>();
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            
            // Determine if this is an enemy or ally
            bool isEnemy = alliance.IsMatch(targetAlliance, Targets.Foe);
            bool isAlly = alliance.IsMatch(targetAlliance, Targets.Ally);
            
            // Calculate damage/healing potential
            var damageScore = CalculateDamageScore(ability, targetStats, isEnemy);
            var healingScore = CalculateHealingScore(ability, targetStats, isAlly);
            var statusScore = CalculateStatusScore(ability, targetStats, isEnemy);
            
            score += damageScore + healingScore + statusScore;
            
            // Bonus for finishing off enemies
            if (isEnemy && targetStats[StatTypes.HP] <= CalculateExpectedDamage(ability, targetStats))
            {
                score += 50f; // High bonus for killing enemies
            }
            
            // Penalty for friendly fire
            if (isAlly && damageScore > 0)
            {
                score -= damageScore * 2f; // Heavy penalty for damaging allies
            }
        }

        // Positional bonuses/penalties
        score += CalculatePositionalScore(unit, moveTile, affectedTiles);
        
        // MP efficiency bonus
        if (magicCost != null)
        {
            score += CalculateMPEfficiencyScore(ability, magicCost.amount, score);
        }

        return score;
    }

    /// <summary>
    /// Calculates damage score for offensive abilities
    /// </summary>
    private float CalculateDamageScore(Ability ability, Stats targetStats, bool isEnemy)
    {
        if (!isEnemy) return 0f;

        var physicalPower = ability.GetComponent<PhysicalAbilityPower>();
        var magicalPower = ability.GetComponent<MagicalAbilityPower>();
        
        float basePower = 0f;
        if (physicalPower != null) basePower = physicalPower.level;
        if (magicalPower != null) basePower = magicalPower.level;
        
        if (basePower <= 0) return 0f;
        
        // Calculate expected damage (simplified)
        float expectedDamage = basePower * 0.8f; // Rough estimate
        float targetHP = targetStats[StatTypes.HP];
        
        // Score based on damage percentage
        float damagePercentage = expectedDamage / targetHP;
        
        return damagePercentage * 100f; // Scale up for scoring
    }

    /// <summary>
    /// Calculates healing score for support abilities
    /// </summary>
    private float CalculateHealingScore(Ability ability, Stats targetStats, bool isAlly)
    {
        if (!isAlly) return 0f;

        var healEffect = ability.GetComponent<HealAbilityEffect>();
        if (healEffect == null) return 0f;
        
        float targetHP = targetStats[StatTypes.HP];
        float targetMaxHP = targetStats[StatTypes.MHP];
        float missingHP = targetMaxHP - targetHP;
        
        if (missingHP <= 0) return 0f; // No healing needed
        
        // Score based on how much healing is needed
        float healingPercentage = missingHP / targetMaxHP;
        return healingPercentage * 50f; // Scale for scoring
    }

    /// <summary>
    /// Calculates status effect score
    /// </summary>
    private float CalculateStatusScore(Ability ability, Stats targetStats, bool isEnemy)
    {
        var inflictEffect = ability.GetComponent<InflictAbilityEffect>();
        if (inflictEffect == null) return 0f;
        
        // High value status effects
        var highValueStatuses = new[] { "Sleep", "Petrify", "Charm", "Confuse", "Silence" };
        var mediumValueStatuses = new[] { "Poison", "Slow", "Blind", "Paralyze" };
        
        if (highValueStatuses.Contains(inflictEffect.statusName))
        {
            return isEnemy ? 30f : -30f; // Good on enemies, bad on allies
        }
        else if (mediumValueStatuses.Contains(inflictEffect.statusName))
        {
            return isEnemy ? 15f : -15f;
        }
        
        return 0f;
    }

    /// <summary>
    /// Calculates expected damage for an ability
    /// </summary>
    private float CalculateExpectedDamage(Ability ability, Stats targetStats)
    {
        var physicalPower = ability.GetComponent<PhysicalAbilityPower>();
        var magicalPower = ability.GetComponent<MagicalAbilityPower>();
        
        float basePower = 0f;
        if (physicalPower != null) basePower = physicalPower.level;
        if (magicalPower != null) basePower = magicalPower.level;
        
        return basePower * 0.8f; // Simplified calculation
    }

    /// <summary>
    /// Calculates positional bonuses/penalties
    /// </summary>
    private float CalculatePositionalScore(Unit unit, Tile moveTile, List<Tile> affectedTiles)
    {
        float score = 0f;
        
        // Bonus for staying in range of allies
        var allies = GetAlliesInRange(unit, 3);
        if (allies.Count > 0)
        {
            score += 5f;
        }
        
        // Penalty for moving too close to enemies
        var enemies = GetEnemiesInRange(moveTile, 1);
        if (enemies.Count > 0)
        {
            score -= 10f;
        }
        
        // Bonus for positioning that affects multiple enemies
        var affectedEnemies = affectedTiles.Count(t => 
        {
            var targetUnit = t.content?.GetComponent<Unit>();
            if (targetUnit == null) return false;
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            return alliance.IsMatch(targetAlliance, Targets.Foe);
        });
        
        if (affectedEnemies > 1)
        {
            score += affectedEnemies * 10f; // Bonus for multi-target
        }
        
        return score;
    }

    /// <summary>
    /// Calculates MP efficiency score
    /// </summary>
    private float CalculateMPEfficiencyScore(Ability ability, int mpCost, float baseScore)
    {
        if (mpCost <= 0) return 0f;
        
        // Higher efficiency for abilities that give good value per MP
        float efficiency = baseScore / mpCost;
        return efficiency * 2f;
    }

    /// <summary>
    /// Determines the target type for an ability
    /// </summary>
    private Targets DetermineTargetType(Ability ability, List<Tile> affectedTiles)
    {
        // Check if ability affects enemies
        bool affectsEnemies = affectedTiles.Any(tile =>
        {
            var targetUnit = tile.content?.GetComponent<Unit>();
            if (targetUnit == null) return false;
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            return alliance.IsMatch(targetAlliance, Targets.Foe);
        });
        
        // Check if ability affects allies
        bool affectsAllies = affectedTiles.Any(tile =>
        {
            var targetUnit = tile.content?.GetComponent<Unit>();
            if (targetUnit == null) return false;
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            return alliance.IsMatch(targetAlliance, Targets.Ally);
        });
        
        if (affectsEnemies && !affectsAllies) return Targets.Foe;
        if (affectsAllies && !affectsEnemies) return Targets.Ally;
        if (affectsEnemies && affectsAllies) return Targets.Tile;
        
        return Targets.Tile;
    }

    /// <summary>
    /// Gets all allies within range
    /// </summary>
    private List<Unit> GetAlliesInRange(Unit unit, int range)
    {
        var allies = new List<Unit>();
        var tiles = bc.board.Search(unit.tile, (t1, t2) => Vector3.Distance(new Vector3(t1.pos.x, t1.pos.y, 0), new Vector3(t2.pos.x, t2.pos.y, 0)) <= range);
        
        foreach (var tile in tiles)
        {
            if (tile.content == null) continue;
            var targetUnit = tile.content.GetComponent<Unit>();
            if (targetUnit == null) continue;
            
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            if (alliance.IsMatch(targetAlliance, Targets.Ally))
            {
                allies.Add(targetUnit);
            }
        }
        
        return allies;
    }

    /// <summary>
    /// Gets all enemies within range
    /// </summary>
    private List<Unit> GetEnemiesInRange(Tile centerTile, int range)
    {
        var enemies = new List<Unit>();
        var tiles = bc.board.Search(centerTile, (t1, t2) => Vector3.Distance(new Vector3(t1.pos.x, t1.pos.y, 0), new Vector3(t2.pos.x, t2.pos.y, 0)) <= range);
        
        foreach (var tile in tiles)
        {
            if (tile.content == null) continue;
            var targetUnit = tile.content.GetComponent<Unit>();
            if (targetUnit == null) continue;
            
            var targetAlliance = targetUnit.GetComponent<Alliance>();
            if (alliance.IsMatch(targetAlliance, Targets.Foe))
            {
                enemies.Add(targetUnit);
            }
        }
        
        return enemies;
    }

    /// <summary>
    /// Gets available move options for a unit
    /// </summary>
    private List<Tile> GetMoveOptions(Unit unit)
    {
        var status = unit.GetComponent<Status>();
        if (status != null && !unit.GetComponent<Movement>().CanMove())
            return new List<Tile> { unit.tile };

        var options = unit.GetComponent<Movement>().GetTilesInRange(bc.board);
        options.Add(unit.tile);
        return options;
    }

    /// <summary>
    /// Fallback: move toward nearest enemy
    /// </summary>
    private void MoveTowardNearestEnemy(PlanOfAttack poa)
    {
        var moveOptions = GetMoveOptions(actor);
        var nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            var path = FindPathToTarget(actor.tile, nearestEnemy.tile, moveOptions);
            if (path.Count > 0)
            {
                poa.moveLocation = path[0].pos;
                return;
            }
        }
        
        poa.moveLocation = actor.tile.pos;
    }

    /// <summary>
    /// Finds the nearest enemy unit
    /// </summary>
    private Unit FindNearestEnemy()
    {
        Unit nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        var allUnits = FindObjectsOfType<Unit>();
        foreach (var unit in allUnits)
        {
            var unitAlliance = unit.GetComponent<Alliance>();
            if (unitAlliance != null && alliance.IsMatch(unitAlliance, Targets.Foe))
            {
                var stats = unit.GetComponent<Stats>();
                if (stats[StatTypes.HP] > 0) // Only living enemies
                {
                    float distance = Vector3.Distance(actor.transform.position, unit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = unit;
                    }
                }
            }
        }
        
        return nearestEnemy;
    }

    /// <summary>
    /// Finds path to target (simplified pathfinding)
    /// </summary>
    private List<Tile> FindPathToTarget(Tile start, Tile target, List<Tile> availableMoves)
    {
        // Simple pathfinding: move toward target
        var path = new List<Tile>();
        var startPos = new Vector3(start.pos.x, start.pos.y, 0);
        var targetPos = new Vector3(target.pos.x, target.pos.y, 0);
        var direction = (targetPos - startPos);
        direction.Normalize();
        
        // Find the move option closest to the target direction
        Tile bestMove = null;
        float bestScore = float.MaxValue;
        
        foreach (var move in availableMoves)
        {
            var movePos = new Vector3(move.pos.x, move.pos.y, 0);
            var moveDirection = (movePos - startPos);
            moveDirection.Normalize();
            float score = Vector3.Distance(moveDirection, direction);
            
            if (score < bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        if (bestMove != null)
        {
            path.Add(bestMove);
        }
        
        return path;
    }

    /// <summary>
    /// Determines the best facing direction after an action
    /// </summary>
    public Directions DetermineEndFacingDirection()
    {
        var nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            var start = actor.dir;
            for (var i = 0; i < 4; i++)
            {
                actor.dir = (Directions)i;
                if (nearestEnemy.GetFacing(actor) == Facings.Front)
                {
                    var bestDir = actor.dir;
                    actor.dir = start; // Restore original direction
                    return bestDir;
                }
            }
            actor.dir = start; // Restore original direction
        }

        return (Directions)Random.Range(0, 4);
    }

    #endregion
}

/// <summary>
/// Represents a tactical action the AI can take
/// </summary>
public class TacticalAction
{
    public Ability ability;
    public Targets targetType;
    public Point moveLocation;
    public Point fireLocation;
    public Directions attackDirection;
    public float tacticalScore;
}

/// <summary>
/// Helper class for tactical evaluation
/// </summary>
public class TacticalEvaluator
{
    // This class can be expanded with more sophisticated evaluation methods
    // For now, it's a placeholder for future tactical AI improvements
}
