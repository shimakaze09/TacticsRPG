using UnityEngine;

/// <summary>
/// The battle reward model (issue #59): one versioned policy that turns a
/// contract's authored ContractRewards — or the bounded writ fallback — plus
/// the battle outcome into a BattleResultsData payload, and commits that
/// payload exactly once. Briefing forecasts, the results screen, and the
/// committed transaction all show the same numbers because they all come from
/// here. Exploit rules are structural: authored pay is flat (reinforcement
/// kills add nothing), only Enemy-alliance units count as kills (guests and
/// neutrals never pay), writ pay is capped by the starting enemy roster, and
/// difficulty never scales any reward (toggling Hard buys nothing).
/// </summary>
public static class RewardPolicy
{
    /// <summary>Bumped whenever settle/commit semantics change.</summary>
    public const int Version = 1;

    /// <summary>Writ fallback: flat completion pay in scrip.</summary>
    public const int WritBasePay = 500;

    /// <summary>Writ fallback: scrip per starting enemy defeated.</summary>
    public const int WritPayPerEnemy = 100;

    /// <summary>Writ fallback: EXP per starting enemy defeated.</summary>
    public const int WritExpPerEnemy = 100;

    /// <summary>Writ fallback: Cert per starting enemy defeated.</summary>
    public const int WritCertPerEnemy = 50;

    /// <summary>Percent of EXP and Cert a KO'd participant still receives.</summary>
    public const int KoSharePercent = 50;

    #region Forecast

    /// <summary>
    /// The best-case payload for an authored contract (victory with full
    /// clear) — what a briefing screen quotes before the battle. Returns the
    /// same shape Settle produces so the quote can never drift from the pay.
    /// </summary>
    public static BattleResultsData Forecast(BattleDefinition definition)
    {
        if (definition == null)
            return null;

        ContractRewards rewards = definition.rewards ?? new ContractRewards();
        return new BattleResultsData
        {
            victory = true,
            policyVersion = Version,
            goldGained = rewards.basePay + rewards.bonusPay,
            expGained = rewards.expAward,
            jpGained = rewards.certAward,
            itemsGained = rewards.salvage != null ? rewards.salvage.ToArray() : new string[0],
            playerUnits = new Unit[0]
        };
    }

    /// <summary>
    /// The best-case writ payload for a given starting enemy count — the
    /// quote for repeatable contracts until #52's encounter rating replaces
    /// the flat per-enemy terms.
    /// </summary>
    public static BattleResultsData ForecastWrit(int enemyCount)
    {
        int enemies = Mathf.Max(0, enemyCount);
        return new BattleResultsData
        {
            victory = true,
            policyVersion = Version,
            goldGained = WritBasePay + enemies * WritPayPerEnemy,
            expGained = enemies * WritExpPerEnemy,
            jpGained = enemies * WritCertPerEnemy,
            itemsGained = new string[0],
            playerUnits = new Unit[0]
        };
    }

    #endregion

    #region Settle

    /// <summary>
    /// Turns the finished battle into its reward payload using the pending
    /// contract (or the scene's test battle) as the authored source.
    /// </summary>
    public static BattleResultsData Settle(BattleController battle, bool victory)
    {
        return Settle(battle, DefinitionFor(battle), victory);
    }

    /// <summary>
    /// Definition-explicit settle: authored contracts pay their flat authored
    /// amounts (plus the full-clear bonus and defeat consolation); writ
    /// battles pay bounded per-enemy amounts. Deterministic — same battle
    /// state, same payload.
    /// </summary>
    public static BattleResultsData Settle(BattleController battle, BattleDefinition definition, bool victory)
    {
        var results = new BattleResultsData
        {
            victory = victory,
            policyVersion = Version,
            itemsGained = new string[0],
            playerUnits = GetPlayerUnits(battle)
        };

        if (definition != null)
        {
            ContractRewards rewards = definition.rewards ?? new ContractRewards();
            if (victory)
            {
                results.goldGained = rewards.basePay + (AllEnemiesDown(battle) ? rewards.bonusPay : 0);
                results.expGained = rewards.expAward;
                results.jpGained = rewards.certAward;
                results.itemsGained = rewards.salvage != null ? rewards.salvage.ToArray() : new string[0];
            }
            else
            {
                results.goldGained = rewards.basePay * rewards.defeatPayPercent / 100;
            }
        }
        else if (victory)
        {
            // Writ fallback: pay per starting enemy defeated. Writs author no
            // waves, so every Enemy-alliance unit is part of the starting
            // roster; the count can never grow past it.
            int defeated = CountDefeatedEnemies(battle);
            results.goldGained = WritBasePay + defeated * WritPayPerEnemy;
            results.expGained = defeated * WritExpPerEnemy;
            results.jpGained = defeated * WritCertPerEnemy;
        }

        return results;
    }

    #endregion

    #region Commit

    /// <summary>
    /// Applies a settled payload exactly once: EXP/Cert per participant (KO'd
    /// units keep KoSharePercent), scrip into the Bank, salvage into the party
    /// inventory. Re-invoking with the same payload is a no-op, so a repeated
    /// post-battle flow cannot double-pay.
    /// </summary>
    public static void Commit(BattleResultsData results)
    {
        if (results == null || results.committed)
            return;

        results.committed = true;

        if (results.playerUnits != null)
        {
            foreach (Unit unit in results.playerUnits)
            {
                if (unit == null)
                    continue;

                int share = IsKnockedOut(unit) ? KoSharePercent : 100;

                var rank = unit.GetComponent<Rank>();
                if (rank != null)
                    rank.EXP += results.expGained * share / 100;

                var jobManager = unit.GetComponent<JobManager>();
                if (jobManager != null)
                    jobManager.AddJobPoints(results.jpGained * share / 100);
            }
        }

        if (results.goldGained != 0)
            Bank.Instance.gold += results.goldGained;

        if (results.itemsGained != null)
        {
            foreach (string gearId in results.itemsGained)
                PartyInventory.Instance.Add(gearId);
        }
    }

    #endregion

    #region Battle-state queries

    // The authored contract for this battle: the flow's pending contract
    // first, the scene's dev/test override second (mirrors InitBattleState).
    private static BattleDefinition DefinitionFor(BattleController battle)
    {
        if (GameFlowController.Instance != null && GameFlowController.Instance.PendingBattle != null)
            return GameFlowController.Instance.PendingBattle;
        return battle != null ? battle.testBattle : null;
    }

    // Participants are the Hero-alliance units; guests/neutrals are never in
    // the payload and enemies never receive player rewards.
    private static Unit[] GetPlayerUnits(BattleController battle)
    {
        var players = new System.Collections.Generic.List<Unit>();
        if (battle != null)
        {
            foreach (Unit unit in battle.units)
            {
                if (unit == null)
                    continue;
                var alliance = unit.GetComponent<Alliance>();
                if (alliance != null && alliance.type == Alliances.Hero)
                    players.Add(unit);
            }
        }

        return players.ToArray();
    }

    // Kill counting: only Enemy-alliance units — a dead guest or neutral
    // bystander is not a payable kill.
    private static int CountDefeatedEnemies(BattleController battle)
    {
        int count = 0;
        if (battle != null)
        {
            foreach (Unit unit in battle.units)
            {
                if (unit == null)
                    continue;
                var alliance = unit.GetComponent<Alliance>();
                if (alliance != null && alliance.type == Alliances.Enemy && IsKnockedOut(unit))
                    count++;
            }
        }

        return count;
    }

    // Full clear = no Enemy-alliance unit left standing (waves included).
    private static bool AllEnemiesDown(BattleController battle)
    {
        if (battle == null)
            return false;

        foreach (Unit unit in battle.units)
        {
            if (unit == null)
                continue;
            var alliance = unit.GetComponent<Alliance>();
            if (alliance != null && alliance.type == Alliances.Enemy && !IsKnockedOut(unit))
                return false;
        }

        return true;
    }

    // A unit is down when HP sits at its floor (MinHP can be > 0 for
    // DefeatTarget victory conditions).
    private static bool IsKnockedOut(Unit unit)
    {
        var health = unit.GetComponent<Health>();
        return health == null || health.HP <= health.MinHP;
    }

    #endregion
}
