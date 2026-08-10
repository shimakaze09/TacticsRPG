#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The committed regression suite: every battle-system invariant proven
/// during development, runnable in one pass. Trigger via
/// Tactics RPG → Run Battle Probes (enters play mode on the Battle scene,
/// waits for init, asserts, prints a PASS/FAIL summary, exits play).
/// Every new system must add its probes here (ARCHITECTURE.md
/// "Verification workflow").
/// </summary>
public class BattleProbeRunner : MonoBehaviour
{
    public const string TriggerFlag = "TacticsRPG.RunBattleProbes";

    private readonly List<string> failures = new List<string>();
    private int checksRun;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SpawnIfRequested()
    {
        if (!SessionState.GetBool(TriggerFlag, false))
            return;

        SessionState.SetBool(TriggerFlag, false);
        new GameObject("BattleProbeRunner").AddComponent<BattleProbeRunner>();
    }

    private void Start()
    {
        // The suite must finish even when the editor window loses focus
        Application.runInBackground = true;
        StartCoroutine(Run());
    }

    private void Check(string name, bool condition, string details = "")
    {
        checksRun++;
        if (!condition)
            failures.Add(name + (string.IsNullOrEmpty(details) ? "" : " — " + details));
    }

    // Roughly-equal helper for damage math that crosses float/floor chains
    private static bool Near(int actual, float expected, int tolerance = 1)
    {
        return Mathf.Abs(actual - expected) <= tolerance;
    }

    private IEnumerator Run()
    {
        // Wait for the battle to finish initializing
        BattleController bc = null;
        var deadline = Time.realtimeSinceStartup + 30f;
        while (Time.realtimeSinceStartup < deadline)
        {
            bc = FindAnyObjectByType<BattleController>();
            if (bc != null && bc.units.Count > 0 && bc.GetComponent<BattleClock>() != null)
                break;
            yield return null;
        }

        if (bc == null || bc.units.Count == 0)
        {
            Debug.LogError("[Probes] Battle never initialized — aborting");
            Finish(false);
            yield break;
        }

        yield return null; // one settle frame

        try
        {
            ProbeBattleSetup(bc);
            ProbeGearAndStats(bc);
            ProbeAbilityMemory(bc);
            ProbeWeaponBehavior(bc);
            ProbeTraits(bc);
            ProbeElementsAndCrits(bc);
            ProbeTargetFilters(bc);
            // Status removal destroys deferred, so this block yields frames
            yield return StartCoroutine(ProbeControlStatuses(bc));
            ProbeTerrain(bc);
            ProbeLevelScaling();
            ProbeGrowthModel();
            ProbeJobThresholds();
            ProbeControlBudget(bc);
            // Tempo statuses add/remove effects, which destroy deferred
            yield return StartCoroutine(ProbeTempoStatuses(bc));
            ProbeClockAndWaves(bc); // mutates turn count
            // Publishes whole rounds of turn events — keep dead last
            yield return StartCoroutine(ProbeControlExpiry(bc));
        }
        finally
        {
            if (failures.Count == 0)
            {
                Debug.Log($"[Probes] PASSED {checksRun}/{checksRun}");
            }
            else
            {
                foreach (var failure in failures)
                    Debug.LogError("[Probes] FAIL: " + failure);
                Debug.LogError($"[Probes] {checksRun - failures.Count}/{checksRun} passed, {failures.Count} FAILED");
            }
        }

        yield return null;
        Finish(failures.Count == 0);
    }

    // Interactive runs drop back to edit mode; headless runs exit with a
    // CI-friendly code (0 = all passed)
    private static void Finish(bool passed)
    {
        if (Application.isBatchMode)
            EditorApplication.Exit(passed ? 0 : 1);
        else
            EditorApplication.isPlaying = false;
    }

    private static Unit Find(BattleController bc, string name)
    {
        foreach (var u in bc.units)
            if (u.name == name)
                return u;
        return null;
    }

    private static Ability AttackOf(Unit unit)
    {
        foreach (var a in unit.GetComponentsInChildren<Ability>())
            if (a.name == "Attack")
                return a;
        return null;
    }

    // Swaps the unit's weapon to a catalog id (play-mode only, not restored)
    private static void EquipWeapon(Unit unit, string gearId)
    {
        var item = ItemFactory.Create(gearId);
        item.transform.SetParent(unit.transform);
        var equippable = item.GetComponent<Equippable>();
        unit.GetComponent<Equipment>().Equip(equippable, equippable.defaultSlots);
    }

    // ---- 1.8: authored battles ------------------------------------------

    private void ProbeBattleSetup(BattleController bc)
    {
        var def = bc.testBattle;
        Check("battle definition wired", def != null);
        if (def == null)
            return;

        Check("authored unit count", bc.units.Count == def.heroes.Count + def.enemies.Count,
            $"{bc.units.Count} vs {def.heroes.Count + def.enemies.Count}");
        Check("battle clock present", bc.GetComponent<BattleClock>() != null);
        Check("element rules present", bc.GetComponent<ElementRules>() != null);
        Check("elevation rules present", bc.GetComponent<ElevationRules>() != null);
        Check("status expiry rules present", bc.GetComponent<StatusExpiryRules>() != null);
        Check("victory condition present", bc.GetComponent<BaseVictoryCondition>() != null);
        Check("wave hook when waves authored",
            def.waves.Count == 0 || bc.GetComponent<BattleEvents>() != null);

        foreach (var u in bc.units)
            Check("unit tile link " + u.name, u.tile != null && u.tile.content == u.gameObject);
    }

    // ---- 1.9: equipment --------------------------------------------------

    private void ProbeGearAndStats(BattleController bc)
    {
        foreach (var u in bc.units)
        {
            var job = u.GetComponent<JobManager>().CurrentJob;
            var expected = GearCatalog.StartingGear(job.id);
            Check("loadout " + u.name, u.GetComponent<Equipment>().items.Count == expected.Length,
                $"{u.GetComponent<Equipment>().items.Count} items vs {expected.Length}");
        }

        var alaois = Find(bc, "Alaois");
        if (alaois == null)
        {
            Check("Alaois present", false);
            return;
        }

        var stats = alaois.GetComponent<Stats>();
        var equipment = alaois.GetComponent<Equipment>();
        var weapon = equipment.GetItem(EquipSlots.Primary);
        var feature = weapon.GetComponent<StatModifierFeature>();

        var before = stats[feature.type];
        equipment.UnEquip(weapon);
        Check("unequip drops stat", stats[feature.type] == before - feature.amount);
        equipment.Equip(weapon, weapon.defaultSlots);
        Check("re-equip restores stat", stats[feature.type] == before);

        var atk = stats[StatTypes.ATK];
        var def = stats[StatTypes.DEF];
        alaois.GetComponent<JobManager>().RecalculateStats();
        Check("recalc preserves gear ATK", stats[StatTypes.ATK] == atk, $"{atk} -> {stats[StatTypes.ATK]}");
        Check("recalc preserves gear DEF", stats[StatTypes.DEF] == def, $"{def} -> {stats[StatTypes.DEF]}");
    }

    // ---- issue #51: ability memory integrity ------------------------------

    // Locked and character-exclusive jobs must not leak abilities into
    // permanent ability memory; repair must scrub leaked saves.
    private void ProbeAbilityMemory(BattleController bc)
    {
        foreach (var u in bc.units)
        {
            var jm = u.GetComponent<JobManager>();
            if (jm == null)
                continue;

            var progress = jm.ProgressData;
            var memory = jm.AbilityMemory;

            // The set of abilities the unit's actual job progress justifies
            var justified = new HashSet<string>();
            JobDefinition lockedJob = null;
            foreach (var job in jm.allJobs)
            {
                if (job == null)
                    continue;
                if (!progress.IsJobUnlocked(job))
                {
                    Check("no-entry job reads level 0 " + u.name + "/" + job.jobName,
                        progress.GetJobLevel(job) == 0,
                        "got " + progress.GetJobLevel(job));
                    lockedJob = job;
                    continue;
                }

                foreach (var name in job.GetUnlockedAbilities(progress.GetJobLevel(job)))
                    justified.Add(name);
            }

            foreach (var learned in memory.learnedAbilities)
                Check("learned ability justified " + u.name, justified.Contains(learned),
                    learned + " has no backing job progress");

            // Re-sync must not add anything new for unchanged progress
            var before = memory.GetLearnedAbilityCount();
            memory.SyncLearnedAbilities(progress, jm.allJobs);
            Check("re-sync adds nothing " + u.name, memory.GetLearnedAbilityCount() == before,
                $"{before} -> {memory.GetLearnedAbilityCount()}");

            // A locked job's Grade-1 ability must not be in memory (unless an
            // unlocked job also grants an ability of the same name)
            if (lockedJob != null)
            {
                foreach (var name in lockedJob.GetUnlockedAbilities(1))
                {
                    if (!justified.Contains(name))
                        Check("locked job leaks nothing " + u.name,
                            !memory.HasLearnedAbility(name), name);
                }
            }

            // Save repair scrubs exactly what the leak taught (a locked job's
            // Grade-1 id) while keeping ids it cannot attribute to the leak —
            // a legitimately learned ability must survive repair even though
            // no purchase-provenance data exists yet
            string leakedId = null;
            if (lockedJob != null)
            {
                foreach (var name in lockedJob.GetUnlockedAbilities(1))
                {
                    if (!justified.Contains(name))
                    {
                        leakedId = name;
                        break;
                    }
                }
            }

            memory.learnedAbilities.Add("__probe_unattributable_ability__");
            if (leakedId != null)
                memory.learnedAbilities.Add(leakedId);

            var removed = memory.RepairLearnedAbilities(progress, jm.allJobs);
            if (leakedId != null)
                Check("repair scrubs leaked id " + u.name,
                    removed == 1 && !memory.HasLearnedAbility(leakedId),
                    $"removed {removed} for {leakedId}");
            Check("repair keeps unattributable id " + u.name,
                memory.HasLearnedAbility("__probe_unattributable_ability__"));

            memory.learnedAbilities.Remove("__probe_unattributable_ability__");
            Check("repair keeps justified " + u.name, memory.GetLearnedAbilityCount() == before,
                $"{before} -> {memory.GetLearnedAbilityCount()}");
        }
    }

    // ---- 1.9b/1.9c: weapon behavior --------------------------------------

    private void ProbeWeaponBehavior(BattleController bc)
    {
        var board = bc.board;
        var alaois = Find(bc, "Alaois");
        var attack = AttackOf(alaois);
        var range = attack.GetComponent<AbilityRange>();
        var area = attack.GetComponent<AbilityArea>();
        Check("weapon range component", range is WeaponAbilityRange);
        Check("weapon area component", area is WeaponAbilityArea);

        // Reach + dead zone (rifle)
        EquipWeapon(alaois, "slug_thrower");
        var tiles = range.GetTilesInRange(board);
        var adjacentTargetable = false;
        var maxDist = 0;
        foreach (var t in tiles)
        {
            var d = Mathf.Abs(t.pos.x - alaois.tile.pos.x) + Mathf.Abs(t.pos.y - alaois.tile.pos.y);
            if (d == 1) adjacentTargetable = true;
            if (d > maxDist) maxDist = d;
        }

        Check("rifle reach 5", maxDist == 5, "max " + maxDist);
        Check("rifle dead zone", !adjacentTargetable);

        // Direct vs arcing fire past a standing unit
        var blocker = Find(bc, "Hania");
        var mPos = alaois.tile.pos;
        var blockTile = board.GetTile(new Point(mPos.x + 2, mPos.y));
        var farTile = board.GetTile(new Point(mPos.x + 4, mPos.y));
        if (blockTile != null && farTile != null && blockTile.content == null && farTile.content == null)
        {
            var home = blocker.tile;
            blocker.Place(blockTile);
            blocker.Match();
            Check("direct fire blocked by unit", !LineOfSight.Clear(board, alaois.tile, farTile, true));
            Check("arcing fire clears unit", LineOfSight.Clear(board, alaois.tile, farTile, false));
            blocker.Place(home);
            blocker.Match();
        }

        // Sweep and line footprints
        var east = new Point(alaois.tile.pos.x + 1, alaois.tile.pos.y);
        EquipWeapon(alaois, "grief_edge");
        Check("sweep hits 3", area.GetTilesInArea(board, east).Count == 3);
        EquipWeapon(alaois, "drip_torch");
        var ray = area.GetTilesInArea(board, east);
        Check("line sprays full reach", ray.Count == 3 && ray[2].pos.x == alaois.tile.pos.x + 3);

        // Damage profile: exact engine math mace (100%) vs two-head (120%)
        var rogue = Find(bc, "Enemy Rogue");
        var effect = attack.GetComponentInChildren<DamageAbilityEffect>();
        EquipWeapon(alaois, "linebreaker_mace");
        var predictMace = -effect.Predict(rogue.tile);
        EquipWeapon(alaois, "twohead_blade");
        var predictTwohead = -effect.Predict(rogue.tile);
        Check("damage profile scales power", predictTwohead > predictMace,
            $"{predictMace} vs {predictTwohead}");
    }

    // ---- 1.9b/1.9c: gear traits -------------------------------------------

    private void ProbeTraits(BattleController bc)
    {
        var alaois = Find(bc, "Alaois");
        var rogue = Find(bc, "Enemy Rogue");
        var stats = alaois.GetComponent<Stats>();
        var effect = AttackOf(alaois).GetComponentInChildren<DamageAbilityEffect>();

        // Recoil: half of a 40-damage hit comes back
        EquipWeapon(alaois, "twohead_blade");
        var hp = stats[StatTypes.HP];
        effect.Publish(new AbilityHitEvent(alaois, rogue, -40));
        Check("recoil feeds back half", stats[StatTypes.HP] == hp - 20, $"{hp} -> {stats[StatTypes.HP]}");

        // Winded: the cleaver Throttles its wielder once
        EquipWeapon(alaois, "pit_cleaver");
        effect.Publish(new AbilityHitEvent(alaois, rogue, -30));
        var throttle = alaois.GetComponentInChildren<ThrottleStatus>();
        Check("cleaver winds the attacker", throttle != null);
        if (throttle != null)
        {
            var condition = throttle.GetComponentInChildren<DurationStatusCondition>();
            Check("winded duration", condition != null && condition.duration == 2);
        }

        // Lifesteal: grief-edge heals 25% of the wound
        EquipWeapon(alaois, "grief_edge");
        stats.SetValue(StatTypes.HP, 50, false);
        effect.Publish(new AbilityHitEvent(alaois, rogue, -40));
        Check("lifesteal heals", stats[StatTypes.HP] == 60, "HP " + stats[StatTypes.HP]);

        // StatusOnHit at forced 100%
        EquipWeapon(alaois, "static_knife");
        var gear = GearCatalog.Get("static_knife");
        GearTraitData statusTrait = null;
        foreach (var t in gear.traits)
            if (t.type == GearTraitType.StatusOnHit)
                statusTrait = t;
        var oldChance = statusTrait.value;
        statusTrait.value = 100;
        effect.Publish(new AbilityHitEvent(alaois, rogue, -10));
        statusTrait.value = oldChance;
        Check("status on hit inflicts", rogue.GetComponentInChildren<StaticStatus>() != null);

        // Flank: the Absolution Point from front vs back
        EquipWeapon(alaois, "absolution_point");
        var board = bc.board;
        var rPos = rogue.tile.pos;
        rogue.dir = Directions.East;
        var frontTile = board.GetTile(new Point(rPos.x + 1, rPos.y));
        var backTile = board.GetTile(new Point(rPos.x - 1, rPos.y));
        if (frontTile != null && backTile != null && frontTile.content == null && backTile.content == null)
        {
            var home = alaois.tile;
            alaois.Place(frontTile);
            alaois.Match();
            var front = -effect.Predict(rogue.tile);
            alaois.Place(backTile);
            alaois.Match();
            var back = -effect.Predict(rogue.tile);
            alaois.Place(home);
            alaois.Match();
            Check("flank bonus from behind", Near(back, front * 1.4f, 2), $"{front} front vs {back} back");
        }

        // Opener/Execute thresholds
        var rogueStats = rogue.GetComponent<Stats>();
        EquipWeapon(alaois, "wrapped_knuckles");
        rogueStats.SetValue(StatTypes.HP, rogueStats[StatTypes.MHP], false);
        var vsFull = -effect.Predict(rogue.tile);
        rogueStats.SetValue(StatTypes.HP, rogueStats[StatTypes.MHP] - 5, false);
        var vsTouched = -effect.Predict(rogue.tile);
        Check("opener rewards first blood", vsFull > vsTouched, $"{vsFull} vs {vsTouched}");

        EquipWeapon(alaois, "pit_cleaver");
        var vsHealthy = -effect.Predict(rogue.tile);
        rogueStats.SetValue(StatTypes.HP, Mathf.Max(1, rogueStats[StatTypes.MHP] / 5), false);
        var vsDying = -effect.Predict(rogue.tile);
        rogueStats.SetValue(StatTypes.HP, rogueStats[StatTypes.MHP], false);
        Check("execute rewards finishing", vsDying > vsHealthy, $"{vsHealthy} vs {vsDying}");
    }

    // ---- 1.10: elements + crits -------------------------------------------

    private void ProbeElementsAndCrits(BattleController bc)
    {
        var alaois = Find(bc, "Alaois");
        var rogue = Find(bc, "Enemy Rogue");
        var effect = AttackOf(alaois).GetComponentInChildren<DamageAbilityEffect>();

        var atkEl = alaois.GetComponent<Elements>();
        var defEl = rogue.GetComponent<Elements>();
        Check("units carry affinities", atkEl != null && defEl != null);
        if (atkEl == null || defEl == null)
            return;

        EquipWeapon(alaois, "linebreaker_mace");
        var atkType = atkEl.types;
        var defType = defEl.types;
        atkEl.types = ElementTypes.Fire;
        defEl.types = ElementTypes.Earth;
        var neutral = -effect.Predict(rogue.tile);
        defEl.types = ElementTypes.Ice;
        var advantage = -effect.Predict(rogue.tile);
        defEl.types = ElementTypes.Water;
        var restrained = -effect.Predict(rogue.tile);
        atkEl.types = atkType;
        defEl.types = defType;

        Check("element advantage +25%", Near(advantage, neutral * 1.25f), $"{neutral} -> {advantage}");
        Check("element restraint -25%", Near(restrained, neutral * 0.75f), $"{neutral} -> {restrained}");

        // Crit chance: base and geared
        Check("crit base 5%", CriticalHit.Chance(rogue) == 5, "got " + CriticalHit.Chance(rogue));
        EquipWeapon(alaois, "absolution_point");
        Check("crit gear bonus", CriticalHit.Chance(alaois) == 15, "got " + CriticalHit.Chance(alaois));

        var crits = 0;
        for (var i = 0; i < 400; i++)
            if (CriticalHit.Roll(alaois))
                crits++;
        Check("crit roll near 15%", crits > 20 && crits < 110, crits + "/400");
    }

    // ---- issue #53: target allegiance filters ------------------------------

    // Support abilities must respect allegiance: Ally includes the caster and
    // teammates but never foes, Self only the caster, KOdAlly only downed
    // teammates. Filters are exercised from a hero's perspective against a
    // live friendly and hostile unit.
    private void ProbeTargetFilters(BattleController bc)
    {
        var alaois = Find(bc, "Alaois");
        var hania = Find(bc, "Hania");
        var rogue = Find(bc, "Enemy Rogue");
        if (alaois == null || hania == null || rogue == null)
        {
            Check("target filter cast present", false);
            return;
        }

        var holder = new GameObject("Probe Target Filters");
        holder.transform.SetParent(alaois.transform);
        var ally = holder.AddComponent<AllyAbilityEffectTarget>();
        var self = holder.AddComponent<SelfAbilityEffectTarget>();
        var koAlly = holder.AddComponent<KOdAllyAbilityEffectTarget>();

        Check("ally filter accepts teammate", ally.IsTarget(hania.tile));
        Check("ally filter accepts caster", ally.IsTarget(alaois.tile));
        Check("ally filter rejects foe", !ally.IsTarget(rogue.tile));

        Check("self filter accepts caster", self.IsTarget(alaois.tile));
        Check("self filter rejects teammate", !self.IsTarget(hania.tile));
        Check("self filter rejects foe", !self.IsTarget(rogue.tile));

        Check("ko-ally filter rejects living teammate", !koAlly.IsTarget(hania.tile));

        // Drop HP directly (bypassing the event pipeline) so the KO branch is
        // observable without spinning up the full KO/revive status flow
        var haniaStats = hania.GetComponent<Stats>();
        var rogueStats = rogue.GetComponent<Stats>();
        var haniaHP = haniaStats[StatTypes.HP];
        var rogueHP = rogueStats[StatTypes.HP];
        haniaStats.SetValue(StatTypes.HP, 0, false);
        rogueStats.SetValue(StatTypes.HP, 0, false);

        Check("ko-ally filter accepts downed teammate", koAlly.IsTarget(hania.tile));
        Check("ko-ally filter rejects downed foe", !koAlly.IsTarget(rogue.tile));
        Check("ally filter rejects downed teammate", !ally.IsTarget(hania.tile));

        haniaStats.SetValue(StatTypes.HP, haniaHP, false);
        rogueStats.SetValue(StatTypes.HP, rogueHP, false);

        // Neutral units are nobody's ally and nobody's foe — support and
        // attack filters must both exclude them
        var rogueAlliance = rogue.GetComponent<Alliance>();
        var savedType = rogueAlliance.type;
        rogueAlliance.type = Alliances.Neutral;
        var attackFilter = AttackOf(alaois).GetComponentInChildren<AbilityEffectTarget>();
        Check("ally filter rejects neutral", !ally.IsTarget(rogue.tile));
        Check("attack filter rejects neutral", !attackFilter.IsTarget(rogue.tile));
        rogueAlliance.type = savedType;

        // Confusion (Swayed) swaps ally and foe — it must not admit Neutral
        // units or divert the Self contract
        var alaoisAlliance = alaois.GetComponent<Alliance>();
        alaoisAlliance.confused = true;
        Check("confused ally filter targets foes", ally.IsTarget(rogue.tile));
        Check("confused ally filter rejects teammate", !ally.IsTarget(hania.tile));
        Check("confused ally filter still accepts caster", ally.IsTarget(alaois.tile));
        Check("confused attack filter never targets caster", !attackFilter.IsTarget(alaois.tile));
        Check("confused self filter unaffected",
            self.IsTarget(alaois.tile) && !self.IsTarget(rogue.tile));
        rogueAlliance.type = Alliances.Neutral;
        Check("confused ally filter still rejects neutral", !ally.IsTarget(rogue.tile));
        rogueAlliance.type = savedType;
        alaoisAlliance.confused = false;

        // End to end through generated data: a full-board support broadcast
        // must carry the Ally contract from JSON to prefab and be legal on
        // every living hero and illegal on every enemy
        var broadcast = Resources.Load<GameObject>("Abilities/Balladeer/Grit Ballad");
        Check("generated broadcast present", broadcast != null,
            "run Tactics RPG → Generate Content → Abilities");
        if (broadcast != null)
        {
            var instance = Instantiate(broadcast);
            instance.transform.SetParent(alaois.transform);
            var broadcastFilter = instance.GetComponentInChildren<AbilityEffectTarget>();
            Check("broadcast carries Ally contract", broadcastFilter is AllyAbilityEffectTarget,
                broadcastFilter != null ? broadcastFilter.GetType().Name : "no filter");
            if (broadcastFilter != null)
            {
                foreach (var u in bc.units)
                {
                    var side = u.GetComponent<Alliance>().type;
                    var living = u.GetComponent<Stats>()[StatTypes.HP] > 0;
                    var expectLegal = side == Alliances.Hero && living;
                    Check("broadcast legality " + u.name,
                        broadcastFilter.IsTarget(u.tile) == expectLegal,
                        $"side {side}, living {living}");
                }
            }

            Destroy(instance);
        }

        // The same contracts drive the AI: from an enemy caster every filter
        // must resolve relative to its own side, for single targets and
        // full-board broadcasts alike (issue #53 final criterion).
        // Guest-alliance coverage is intentionally absent — no Guest value
        // exists in Alliances yet; Guest-unit rules land with the escort
        // objective (#32) and must extend this matrix when they do.
        var warrior = Find(bc, "Enemy Warrior");
        Check("second enemy present", warrior != null);
        if (warrior != null)
        {
            var enemyHolder = new GameObject("Probe Enemy Filters");
            enemyHolder.transform.SetParent(rogue.transform);
            var enemyAlly = enemyHolder.AddComponent<AllyAbilityEffectTarget>();
            var enemySelf = enemyHolder.AddComponent<SelfAbilityEffectTarget>();

            Check("enemy ally filter accepts fellow enemy", enemyAlly.IsTarget(warrior.tile));
            Check("enemy ally filter accepts caster", enemyAlly.IsTarget(rogue.tile));
            Check("enemy ally filter rejects hero", !enemyAlly.IsTarget(alaois.tile));
            Check("enemy self filter accepts only caster",
                enemySelf.IsTarget(rogue.tile) && !enemySelf.IsTarget(warrior.tile) &&
                !enemySelf.IsTarget(alaois.tile));

            var enemyAttack = AttackOf(rogue).GetComponentInChildren<AbilityEffectTarget>();
            Check("enemy attack filter accepts hero", enemyAttack.IsTarget(alaois.tile));
            Check("enemy attack filter rejects fellow enemy", !enemyAttack.IsTarget(warrior.tile));
            Check("enemy attack filter rejects caster", !enemyAttack.IsTarget(rogue.tile));

            // Neutral from the AI side: nobody's ally, nobody's foe — the
            // enemy caster's support and attack must both exclude it
            var haniaAlliance = hania.GetComponent<Alliance>();
            var haniaSide = haniaAlliance.type;
            haniaAlliance.type = Alliances.Neutral;
            Check("enemy ally filter rejects neutral", !enemyAlly.IsTarget(hania.tile));
            Check("enemy attack filter rejects neutral", !enemyAttack.IsTarget(hania.tile));
            haniaAlliance.type = haniaSide;

            // KO across sides: a downed fellow enemy stops being a support
            // target for the AI just as a downed teammate does for the player
            var warriorStats = warrior.GetComponent<Stats>();
            var warriorHP = warriorStats[StatTypes.HP];
            warriorStats.SetValue(StatTypes.HP, 0, false);
            Check("enemy ally filter rejects downed fellow enemy", !enemyAlly.IsTarget(warrior.tile));
            warriorStats.SetValue(StatTypes.HP, warriorHP, false);

            // Confused enemy: its support turns on heroes, never on itself
            var rogueAllianceComp = rogue.GetComponent<Alliance>();
            rogueAllianceComp.confused = true;
            Check("confused enemy ally filter targets heroes", enemyAlly.IsTarget(alaois.tile));
            Check("confused enemy ally filter rejects fellow enemy", !enemyAlly.IsTarget(warrior.tile));
            Check("confused enemy ally filter still accepts caster", enemyAlly.IsTarget(rogue.tile));
            haniaAlliance.type = Alliances.Neutral;
            Check("confused enemy ally filter still rejects neutral", !enemyAlly.IsTarget(hania.tile));
            haniaAlliance.type = haniaSide;
            rogueAllianceComp.confused = false;

            // Full-board support from the AI side: the Ally contract blesses
            // enemies only when an enemy sings it
            var enemyBroadcast = Resources.Load<GameObject>("Abilities/Balladeer/Grit Ballad");
            if (enemyBroadcast != null)
            {
                var bInstance = Instantiate(enemyBroadcast);
                bInstance.transform.SetParent(rogue.transform);
                var bFilter = bInstance.GetComponentInChildren<AbilityEffectTarget>();
                foreach (var u in bc.units)
                {
                    var side = u.GetComponent<Alliance>().type;
                    var living = u.GetComponent<Stats>()[StatTypes.HP] > 0;
                    var expectLegal = side == Alliances.Enemy && living;
                    Check("enemy broadcast legality " + u.name,
                        bFilter.IsTarget(u.tile) == expectLegal,
                        $"side {side}, living {living}");
                }

                Destroy(bInstance);
            }

            // Generated single-target heal, both perspectives: legal only on
            // the caster's own living side
            var heal = Resources.Load<GameObject>("Abilities/Sawbones/Patch Up");
            Check("generated single-target heal present", heal != null,
                "run Tactics RPG → Generate Content → Abilities");
            if (heal != null)
            {
                var heroHeal = Instantiate(heal);
                heroHeal.transform.SetParent(alaois.transform);
                var heroHealFilter = heroHeal.GetComponentInChildren<AbilityEffectTarget>();
                Check("hero heal carries Ally contract", heroHealFilter is AllyAbilityEffectTarget,
                    heroHealFilter != null ? heroHealFilter.GetType().Name : "no filter");
                Check("hero heal legal on teammate, illegal on enemy",
                    heroHealFilter.IsTarget(hania.tile) && !heroHealFilter.IsTarget(rogue.tile));
                Destroy(heroHeal);

                var foeHeal = Instantiate(heal);
                foeHeal.transform.SetParent(rogue.transform);
                var foeHealFilter = foeHeal.GetComponentInChildren<AbilityEffectTarget>();
                Check("enemy heal legal on fellow enemy, illegal on hero",
                    foeHealFilter.IsTarget(warrior.tile) && !foeHealFilter.IsTarget(alaois.tile));
                Destroy(foeHeal);
            }

            Destroy(enemyHolder);
        }

        Destroy(holder);
    }

    // ---- 1.11: behavior-control statuses ------------------------------------

    private IEnumerator ProbeControlStatuses(BattleController bc)
    {
        var alaois = Find(bc, "Alaois");
        var rogue = Find(bc, "Enemy Rogue");
        var driver = alaois.GetComponent<Driver>();
        var alliance = alaois.GetComponent<Alliance>();
        var status = alaois.GetComponent<Status>();

        // Swayed: control seized, alliance checks inverted
        var swayed = status.Add<SwayedStatus, DurationStatusCondition>();
        swayed.duration = 9;
        Check("swayed seizes control", driver.Current == Drivers.Computer);
        Check("swayed flips targeting",
            alliance.IsMatch(rogue.GetComponent<Alliance>(), Targets.Ally));
        swayed.Remove();
        yield return null;
        Check("control returns after swayed", driver.Current == Drivers.Human);
        Check("targeting restored",
            alliance.IsMatch(rogue.GetComponent<Alliance>(), Targets.Foe));

        // Redline: seized, and the plan charges the nearest unit
        var redline = status.Add<RedlineStatus, DurationStatusCondition>();
        redline.duration = 9;
        Check("redline seizes control", driver.Current == Drivers.Computer);
        var dictator = alaois.GetComponentInChildren<ITurnPlanOverride>();
        Check("redline dictates the turn", dictator != null);
        if (dictator != null)
        {
            var plan = dictator.BuildPlan(bc, alaois);
            Unit nearest = null;
            var best = int.MaxValue;
            foreach (var u in bc.units)
            {
                if (u == alaois || u.tile == null)
                    continue;
                var d = Mathf.Abs(u.tile.pos.x - alaois.tile.pos.x) + Mathf.Abs(u.tile.pos.y - alaois.tile.pos.y);
                if (d < best)
                {
                    best = d;
                    nearest = u;
                }
            }

            if (nearest != null)
            {
                var before = Mathf.Abs(nearest.tile.pos.x - alaois.tile.pos.x) +
                             Mathf.Abs(nearest.tile.pos.y - alaois.tile.pos.y);
                var after = Mathf.Abs(nearest.tile.pos.x - plan.moveLocation.x) +
                            Mathf.Abs(nearest.tile.pos.y - plan.moveLocation.y);
                Check("redline charges the nearest", after <= before, $"{before} -> {after}");
            }
        }

        redline.Remove();
        yield return null;

        // Scrambled: seized, and the plan stays within legal ground
        var scrambled = status.Add<ScrambledStatus, DurationStatusCondition>();
        scrambled.duration = 9;
        var scrambledDictator = alaois.GetComponentInChildren<ITurnPlanOverride>();
        Check("scrambled dictates the turn", scrambledDictator != null);
        if (scrambledDictator != null)
        {
            var plan = scrambledDictator.BuildPlan(bc, alaois);
            var destination = bc.board.GetTile(plan.moveLocation);
            Check("scrambled wanders on real ground",
                destination != null && destination.CanStop(TileTraversalFlags.Ground));
        }

        scrambled.Remove();
        yield return null;
        Check("control returns after all statuses", driver.Current == Drivers.Human);

        // KO visuals: a downed body squashes flat (and is passable), then
        // stands back up on revival
        var hania = Find(bc, "Hania");
        var body = hania.transform.Find("Jumper");
        var standingScale = body.localScale.y;
        var haniaStats = hania.GetComponent<Stats>();
        haniaStats[StatTypes.HP] -= haniaStats[StatTypes.HP];
        yield return null;
        Check("zero HP applies KO", hania.GetComponentInChildren<KOStatus>() != null);
        Check("downed body drops flat", body.localScale.y < standingScale * 0.5f,
            $"{standingScale} -> {body.localScale.y}");

        // A corpse is not a combatant: hostile attacks can't target it
        var rogueAttackTarget = AttackOf(rogue).GetComponentInChildren<AbilityEffectTarget>();
        Check("downed body not attackable", !rogueAttackTarget.IsTarget(hania.tile));
        haniaStats[StatTypes.HP] += haniaStats[StatTypes.MHP] / 2;
        yield return null;
        Check("revival removes KO", hania.GetComponentInChildren<KOStatus>() == null);
        Check("revived body stands back up", Mathf.Approximately(body.localScale.y, standingScale),
            $"{body.localScale.y}");
        Check("revived unit attackable again", rogueAttackTarget.IsTarget(hania.tile));

        // Resource attacks: MP burn and the Shredded payload
        EquipWeapon(alaois, "cipher_rod");
        var rogueStats = rogue.GetComponent<Stats>();
        rogueStats.SetValue(StatTypes.MP, 10, false);
        var effect = AttackOf(alaois).GetComponentInChildren<DamageAbilityEffect>();
        effect.Publish(new AbilityHitEvent(alaois, rogue, -10));
        Check("mp burn drains reserve", rogueStats[StatTypes.MP] == 4, "MP " + rogueStats[StatTypes.MP]);

        var shredded = StatusRegistry.Inflict(rogue, "Shredded", 3);
        Check("shredded inflictable", shredded != null);
        if (shredded != null)
        {
            var torn = -effect.Predict(rogue.tile);
            shredded.Remove();
            yield return null;
            var whole = -effect.Predict(rogue.tile);
            Check("shredded raises physical damage", torn > whole, $"{whole} -> {torn}");
        }
    }

    // ---- 1.8b: terrain ------------------------------------------------------

    private void ProbeTerrain(BattleController bc)
    {
        var board = bc.board;

        var water = 0;
        var bridges = 0;
        foreach (var tile in board.tiles.Values)
        {
            if (tile.terrain == TerrainType.Water) water++;
            if (tile.terrain == TerrainType.Bridge) bridges++;
        }

        // These only run on maps that actually have a river
        if (water == 0)
            return;

        Check("bridge exists", bridges > 0);

        var alaois = Find(bc, "Alaois");
        var stats = alaois.GetComponent<Stats>();
        var oldMov = stats[StatTypes.MOV];
        stats.SetValue(StatTypes.MOV, 12, false);
        var reach = alaois.GetComponent<Movement>().GetTilesInRange(board);
        stats.SetValue(StatTypes.MOV, oldMov, false);

        var reachesWater = false;
        var reachesBlocked = false;
        foreach (var t in reach)
        {
            if (t.terrain == TerrainType.Water) reachesWater = true;
            if (t.terrain == TerrainType.Obstacle || t.terrain == TerrainType.Building) reachesBlocked = true;
        }

        Check("walker never stops on water", !reachesWater);
        Check("walker never stops in obstacles", !reachesBlocked);

        // Occupant rule: allies are passable, standing foes are walls.
        // Wall off Alaois in a plus-shape with one gap, put a unit in the
        // gap, and see whether he can escape the pocket.
        var rogue = Find(bc, "Enemy Rogue");
        var hania = Find(bc, "Hania");
        ProbePassThrough(bc, alaois, hania, true, "ally is passable");
        ProbePassThrough(bc, alaois, rogue, false, "standing foe blocks");

        // LoS: sight-blocking terrain stops shots, open water doesn't
        Tile obstacle = null;
        foreach (var tile in board.tiles.Values)
            if (tile.terrain == TerrainType.Obstacle)
            {
                obstacle = tile;
                break;
            }

        if (obstacle != null)
        {
            var west = board.GetTile(new Point(obstacle.pos.x - 1, obstacle.pos.y));
            var east = board.GetTile(new Point(obstacle.pos.x + 1, obstacle.pos.y));
            if (west != null && east != null)
                Check("trees block sight", !LineOfSight.Clear(board, west, east));
        }
    }

    // Checks whether mover can path past `occupant` standing directly in the
    // way: mover at the west end of the road, occupant one tile east, and
    // the destination beyond — reachable only by passing through
    private void ProbePassThrough(BattleController bc, Unit mover, Unit occupant, bool expectPassable, string label)
    {
        var board = bc.board;
        var moverHome = mover.tile;
        var occupantHome = occupant.tile;

        // A clear 3-in-a-row strip of standable ground
        Tile a = null, b = null, c = null;
        foreach (var tile in board.tiles.Values)
        {
            var t2 = board.GetTile(new Point(tile.pos.x + 1, tile.pos.y));
            var t3 = board.GetTile(new Point(tile.pos.x + 2, tile.pos.y));
            if (t2 == null || t3 == null)
                continue;
            if (tile.content != null || t2.content != null || t3.content != null)
                continue;
            if (!tile.CanStop(TileTraversalFlags.Ground) || !t2.CanStop(TileTraversalFlags.Ground) ||
                !t3.CanStop(TileTraversalFlags.Ground))
                continue;
            if (tile.height != t2.height || t2.height != t3.height)
                continue;

            a = tile;
            b = t2;
            c = t3;
            break;
        }

        if (a == null)
            return;

        mover.Place(a);
        mover.Match();
        occupant.Place(b);
        occupant.Match();

        var stats = mover.GetComponent<Stats>();
        var oldMov = stats[StatTypes.MOV];
        stats.SetValue(StatTypes.MOV, 2, false);
        var reach = mover.GetComponent<Movement>().GetTilesInRange(board);
        stats.SetValue(StatTypes.MOV, oldMov, false);

        // With MOV 2 the only route to tile c runs straight through b
        Check(label, reach.Contains(c) == expectPassable);

        mover.Place(moverHome);
        mover.Match();
        occupant.Place(occupantHome);
        occupant.Match();
    }

    // ---- issue #52: spawn level drives combat stats -------------------------

    // Golden checks for the progression model: units of the same recipe
    // spawned at levels 1/10/30/99 must differ by exactly the documented
    // level-growth term (ProgressionModel), across striker/skirmisher/caster
    // archetypes — MHP included. Difficulty is pinned to Easy for the exact
    // block, then a Hard spawn must show the enemy-HP multiplier applied at
    // creation time (the factory adds Alliance before the first stat calc).
    private void ProbeLevelScaling()
    {
        var savedDifficulty = DifficultySettings.Current;
        DifficultySettings.Current = Difficulty.Easy;

        string[] recipes = { "Enemy Warrior", "Enemy Rogue", "Enemy Wizard" };
        int[] levels = { 1, 10, 30, 99 };
        var exactStats = new[] { StatTypes.MHP, StatTypes.MMP, StatTypes.ATK, StatTypes.MAT, StatTypes.SPD };
        var easyWarrior10MHP = 0;

        foreach (var recipe in recipes)
        {
            var spawned = new GameObject[levels.Length];
            for (var i = 0; i < levels.Length; i++)
                spawned[i] = UnitFactory.Create(recipe, levels[i]);

            if (spawned[0] == null)
            {
                Check("level scaling recipe " + recipe, false, "recipe failed to spawn");
                continue;
            }

            var baseStats = spawned[0].GetComponent<Stats>();
            var job = spawned[0].GetComponent<JobManager>().CurrentJob;

            for (var i = 1; i < levels.Length; i++)
            {
                var s = spawned[i].GetComponent<Stats>();

                for (var k = 0; k < exactStats.Length; k++)
                {
                    var statIndex = System.Array.IndexOf(JobManager.statOrder, exactStats[k]);
                    var expected = baseStats[exactStats[k]] +
                                   ProgressionModel.LevelGrowthBonus(job, statIndex, levels[i]);
                    expected = exactStats[k] switch
                    {
                        StatTypes.MHP => Mathf.Min(expected, StatLimits.MaxHP),
                        StatTypes.MMP => Mathf.Min(expected, StatLimits.MaxMP),
                        _ => Mathf.Min(expected, StatLimits.MaxPrimaryStat)
                    };
                    Check($"{recipe} L{levels[i]} {exactStats[k]} golden",
                        s[exactStats[k]] == expected,
                        $"expected {expected}, got {s[exactStats[k]]}");
                }

                if (recipe == "Enemy Warrior" && levels[i] == 10)
                    easyWarrior10MHP = s[StatTypes.MHP];
            }

            foreach (var go in spawned)
                if (go != null)
                    Destroy(go);
        }

        // Hard difficulty must scale enemy MHP at creation, not only on the
        // next recalculation
        if (easyWarrior10MHP > 0)
        {
            DifficultySettings.Current = Difficulty.Hard;
            var hardWarrior = UnitFactory.Create("Enemy Warrior", 10);
            if (hardWarrior != null)
            {
                var expected = Mathf.Min(
                    Mathf.RoundToInt(easyWarrior10MHP * DifficultySettings.EnemyHpMultiplier),
                    StatLimits.MaxHP);
                var actual = hardWarrior.GetComponent<Stats>()[StatTypes.MHP];
                Check("hard mode scales enemy MHP at spawn", actual == expected,
                    $"expected {expected}, got {actual}");
                Destroy(hardWarrior);
            }
        }

        DifficultySettings.Current = savedDifficulty;
    }

    // ---- issue #54: growth model v2 -----------------------------------------

    // Golden checks for the bounded growth model, delta-based so gear cancels
    // out: unlocking a job grants nothing, cross-job carryover is the exact
    // bounded kit-fraction at partial training and mastery, and the current
    // job's earned grades add the exact trained step. Difficulty is pinned to
    // Easy so enemy-HP scaling doesn't skew the MHP deltas.
    private void ProbeGrowthModel()
    {
        var savedDifficulty = DifficultySettings.Current;
        DifficultySettings.Current = Difficulty.Easy;

        var unit = UnitFactory.Create("Enemy Warrior", 1);
        if (unit == null)
        {
            Check("growth model unit spawns", false);
            DifficultySettings.Current = savedDifficulty;
            return;
        }

        var jm = unit.GetComponent<JobManager>();
        var stats = unit.GetComponent<Stats>();
        var current = jm.CurrentJob;

        JobDefinition other = null;
        foreach (var job in jm.allJobs)
        {
            if (job != null && job != current && !job.isUnique)
            {
                other = job;
                break;
            }
        }

        if (current == null || other == null)
        {
            Check("growth model jobs available", false);
            Destroy(unit);
            DifficultySettings.Current = savedDifficulty;
            return;
        }

        var order = JobManager.statOrder;
        var baseline = new int[order.Length];
        for (var i = 0; i < order.Length; i++)
            baseline[i] = stats[order[i]];

        // Unlocking without training must change nothing
        jm.ProgressData.UnlockJob(other);
        jm.RecalculateStats();
        for (var i = 0; i < order.Length; i++)
            Check("unlock grants nothing " + order[i], stats[order[i]] == baseline[i],
                $"{baseline[i]} -> {stats[order[i]]}");

        // Partial training carries over the exact bounded fraction
        jm.ProgressData.SetJobLevel(other, 4);
        jm.RecalculateStats();
        for (var i = 0; i < order.Length; i++)
        {
            var expected = baseline[i] + ProgressionModel.CrossJobContribution(other, i, 4);
            Check("grade-4 carryover " + order[i], stats[order[i]] == expected,
                $"expected {expected}, got {stats[order[i]]}");
        }

        // Mastery carries over the full (still small) node
        jm.ProgressData.SetJobLevel(other, 8);
        jm.RecalculateStats();
        for (var i = 0; i < order.Length; i++)
        {
            var expected = baseline[i] + ProgressionModel.CrossJobContribution(other, i, 8);
            Check("mastery carryover " + order[i], stats[order[i]] == expected,
                $"expected {expected}, got {stats[order[i]]}");
        }

        // Mastering the current job adds the exact trained steps on top
        jm.ProgressData.SetJobLevel(current, 8);
        jm.RecalculateStats();
        for (var i = 0; i < order.Length; i++)
        {
            var expected = baseline[i]
                           - ProgressionModel.CurrentJobContribution(current, i, 1)
                           + ProgressionModel.CurrentJobContribution(current, i, 8)
                           + ProgressionModel.CrossJobContribution(other, i, 8);
            Check("current-job mastery " + order[i], stats[order[i]] == expected,
                $"expected {expected}, got {stats[order[i]]}");
        }

        Destroy(unit);
        DifficultySettings.Current = savedDifficulty;
    }

    // ---- issue #20: JP thresholds ------------------------------------------

    // Every job carries exactly seven strictly-increasing cumulative JP
    // thresholds (levels 2-8) with in-range unlock levels and no dead master
    // gate, and the level lookup agrees with the data at every boundary.
    private void ProbeJobThresholds()
    {
        var jobs = Resources.LoadAll<JobDefinition>("Jobs");
        Check("jobs loaded for threshold probe", jobs.Length > 0);

        JobDefinition drifter = null;
        foreach (var job in jobs)
        {
            if (job.id == "drifter")
                drifter = job;

            Check("threshold curve valid " + job.id,
                JobDefinition.ValidateJPCurve(job.jpRequirements) == null,
                JobDefinition.ValidateJPCurve(job.jpRequirements) ?? "");

            var unlocksInRange = true;
            foreach (var unlock in job.abilityUnlocks)
                unlocksInRange &= JobDefinition.IsValidUnlockLevel(unlock.unlockAtJobLevel);
            Check("unlock levels in range " + job.id, unlocksInRange);
        }

        // Failing-data contract: the shared validator the generator gates on
        // must reject every malformed shape it exists to catch
        Check("validator rejects null curve",
            JobDefinition.ValidateJPCurve(null) != null);
        Check("validator rejects wrong length",
            JobDefinition.ValidateJPCurve(new[] { 100, 250 }) != null);
        Check("validator rejects a plateau",
            JobDefinition.ValidateJPCurve(new[] { 100, 250, 450, 700, 1000, 1400, 1400 }) != null);
        Check("validator rejects non-positive gates",
            JobDefinition.ValidateJPCurve(new[] { 0, 250, 450, 700, 1000, 1400, 1900 }) != null);
        Check("unlock level 0 rejected", !JobDefinition.IsValidUnlockLevel(0));
        Check("unlock level 9 rejected", !JobDefinition.IsValidUnlockLevel(9));
        Check("unlock levels 1 and 8 accepted",
            JobDefinition.IsValidUnlockLevel(1) && JobDefinition.IsValidUnlockLevel(8));

        // Boundary behavior on the shared curve: one JP below each gate stays
        // at the previous level, the gate itself advances, and past the top
        // gate the job is simply level 8 — no phantom master gate beyond it
        Check("drifter present for boundaries", drifter != null);
        if (drifter != null)
        {
            Check("zero JP is grade 1", drifter.GetJobLevelForJP(0) == 1);
            for (var i = 0; i < drifter.jpRequirements.Length; i++)
            {
                var gate = drifter.jpRequirements[i];
                Check($"jp {gate - 1} holds level {i + 1}",
                    drifter.GetJobLevelForJP(gate - 1) == i + 1,
                    "got " + drifter.GetJobLevelForJP(gate - 1));
                Check($"jp {gate} reaches level {i + 2}",
                    drifter.GetJobLevelForJP(gate) == i + 2,
                    "got " + drifter.GetJobLevelForJP(gate));
            }

            var top = drifter.jpRequirements[drifter.jpRequirements.Length - 1];
            Check("huge JP stays level 8", drifter.GetJobLevelForJP(99999) == 8);
            Check("maxed next-gate reports the real top",
                drifter.GetJPForNextLevel(top) == top,
                "got " + drifter.GetJPForNextLevel(top));
        }
    }

    // ---- issue #57: RES growth + control budget -----------------------------

    // The control contract: RES grows with level and job profile per
    // ProgressionModel, facing shifts effective resistance, every landed
    // control adds a Steeled stack (+20 effective RES), data-driven control
    // durations cap at the budget, and status chances stay inside the
    // contestability bounds.
    private void ProbeControlBudget(BattleController bc)
    {
        // RES growth goldens: same recipe, levels 1 and 30
        var low = UnitFactory.Create("Enemy Warrior", 1);
        var high = UnitFactory.Create("Enemy Warrior", 30);
        if (low != null && high != null)
        {
            var job = low.GetComponent<JobManager>().CurrentJob;
            Check("RES initialized at L1",
                low.GetComponent<Stats>()[StatTypes.RES] == ProgressionModel.ResistanceFor(job, 1),
                $"expected {ProgressionModel.ResistanceFor(job, 1)}, got {low.GetComponent<Stats>()[StatTypes.RES]}");
            Check("RES grows with level",
                high.GetComponent<Stats>()[StatTypes.RES] == ProgressionModel.ResistanceFor(job, 30),
                $"expected {ProgressionModel.ResistanceFor(job, 30)}, got {high.GetComponent<Stats>()[StatTypes.RES]}");
            Check("RES respects the contestability cap",
                high.GetComponent<Stats>()[StatTypes.RES] <= StatLimits.MaxRES);
        }
        else
        {
            Check("RES growth units spawn", false);
        }

        if (low != null) Destroy(low);
        if (high != null) Destroy(high);

        var alaois = Find(bc, "Alaois");
        var rogue = Find(bc, "Enemy Rogue");
        if (alaois == null || rogue == null)
        {
            Check("control budget cast present", false);
            return;
        }

        var holder = new GameObject("Probe SType");
        holder.transform.SetParent(alaois.transform);
        var hitRate = holder.AddComponent<STypeHitRate>();
        hitRate.accuracy = 85;

        // Facing: attacking from behind is 20 points easier than head-on
        var board = bc.board;
        var rPos = rogue.tile.pos;
        rogue.dir = Directions.East;
        var frontTile = board.GetTile(new Point(rPos.x + 1, rPos.y));
        var backTile = board.GetTile(new Point(rPos.x - 1, rPos.y));
        if (frontTile != null && backTile != null && frontTile.content == null && backTile.content == null)
        {
            var home = alaois.tile;
            alaois.Place(frontTile);
            alaois.Match();
            var front = hitRate.Calculate(rogue.tile);
            alaois.Place(backTile);
            alaois.Match();
            var back = hitRate.Calculate(rogue.tile);
            alaois.Place(home);
            alaois.Match();
            Check("back attack beats front by 20", back - front == 20, $"{front} front vs {back} back");
        }

        // Steeled: each landed control adds one stack of +20 effective RES,
        // and data-driven control durations clamp to the budget
        var before = hitRate.Calculate(rogue.tile);
        var firstControl = StatusRegistry.Inflict(rogue, "Scrambled", 9);
        Check("control duration clamped", firstControl != null && firstControl.duration == ControlBudget.MaxControlDuration,
            firstControl != null ? "duration " + firstControl.duration : "inflict failed");
        var oneStack = hitRate.Calculate(rogue.tile);
        Check("steeled raises resistance", before - oneStack == ControlBudget.SteeledResistancePerStack,
            $"{before} -> {oneStack}");

        var secondControl = StatusRegistry.Inflict(rogue, "Scrambled", 2);
        var twoStacks = hitRate.Calculate(rogue.tile);
        Check("steeled stacks", before - twoStacks == 2 * ControlBudget.SteeledResistancePerStack,
            $"{before} -> {twoStacks}");
        Check("non-control keeps its duration",
            StatusRegistry.Inflict(rogue, "Shredded", 9)?.duration == 9);

        // Contestability bounds: chance never leaves [Min, Max] on the
        // normal path regardless of accuracy extremes
        hitRate.accuracy = 500;
        Check("chance ceiling", hitRate.Calculate(rogue.tile) == ControlBudget.MaxChance,
            "got " + hitRate.Calculate(rogue.tile));
        hitRate.accuracy = -50;
        Check("chance floor", hitRate.Calculate(rogue.tile) == ControlBudget.MinChance,
            "got " + hitRate.Calculate(rogue.tile));

        // Restore the rogue: drop the probe statuses we inflicted
        firstControl?.Remove();
        secondControl?.Remove();
        foreach (var condition in rogue.GetComponentsInChildren<DurationStatusCondition>())
        {
            var effect = condition.GetComponentInParent<StatusEffect>();
            if (effect is SteeledStatus || effect is ShreddedStatus)
                condition.Remove();
        }

        Destroy(holder);

        // Graycast is stasis — full action/CT denial belongs to the budget
        Check("graycast classified as control", ControlBudget.IsControl("Graycast"));

        // RES gear golden at the cap: equip clamps, recalculation converges,
        // a baseline change while equipped stays exact, and unequip restores
        // the (new) derived value — equip and unequip both route through the
        // same deterministic recomputation
        var wizard = UnitFactory.Create("Enemy Wizard", 99);
        if (wizard != null)
        {
            var wizardStats = wizard.GetComponent<Stats>();
            var wizardJm = wizard.GetComponent<JobManager>();
            var derived = wizardStats[StatTypes.RES];
            var expected = Mathf.Min(derived + 10, StatLimits.MaxRES);

            var charm = new GameObject("Probe Res Charm");
            var equippable = charm.AddComponent<Equippable>();
            equippable.defaultSlots = EquipSlots.Accessory;
            var modifier = charm.AddComponent<StatModifierFeature>();
            modifier.type = StatTypes.RES;
            modifier.amount = 10;

            wizard.GetComponent<Equipment>().Equip(equippable, EquipSlots.Accessory);
            Check("res gear equips cap-safe", wizardStats[StatTypes.RES] == expected,
                $"derived {derived}, got {wizardStats[StatTypes.RES]}");
            wizardJm.RecalculateStats();
            Check("res gear recalc converges", wizardStats[StatTypes.RES] == expected,
                $"expected {expected}, got {wizardStats[StatTypes.RES]}");

            // Baseline change while equipped: switch to another job (different
            // MDF kit → different derived RES) and the totals must stay exact
            JobDefinition otherJob = null;
            foreach (var job in wizardJm.allJobs)
            {
                if (job != null && job != wizardJm.CurrentJob && !job.isUnique)
                {
                    otherJob = job;
                    break;
                }
            }

            if (otherJob != null)
            {
                wizardJm.ProgressData.UnlockJob(otherJob);
                wizardJm.SwitchJob(otherJob);
                var switchedDerived = ProgressionModel.ResistanceFor(otherJob, 99);
                var switchedExpected = Mathf.Min(switchedDerived + 10, StatLimits.MaxRES);
                Check("res gear exact after job switch", wizardStats[StatTypes.RES] == switchedExpected,
                    $"expected {switchedExpected}, got {wizardStats[StatTypes.RES]}");
                wizard.GetComponent<Equipment>().UnEquip(equippable);
                Check("res gear unequip restores new baseline", wizardStats[StatTypes.RES] == switchedDerived,
                    $"expected {switchedDerived}, got {wizardStats[StatTypes.RES]}");
            }
            else
            {
                Check("res gear switch job available", false);
            }

            Destroy(wizard);
        }
        else
        {
            Check("res gear golden unit spawns", false);
        }
    }

    // ---- 1.12: tempo statuses (issue #19) ----------------------------------

    // Overclock and Throttle must apply their configured CT multipliers —
    // 1.5x and 0.5x — to every CTR gain, and compose multiplicatively when
    // stacked, so the scheduler, tooltips, and any future initiative preview
    // all read the same value.
    private IEnumerator ProbeTempoStatuses(BattleController bc)
    {
        var alaois = Find(bc, "Alaois");
        if (alaois == null)
        {
            Check("tempo cast present", false);
            yield break;
        }

        var status = alaois.GetComponent<Status>();
        var stats = alaois.GetComponent<Stats>();
        var savedCT = stats[StatTypes.CTR];

        // Earlier probes leave tempo statuses behind (the pit-cleaver's Winded
        // Throttle) — clear the sheet so the baseline is genuinely unmodified
        foreach (var stale in alaois.GetComponentsInChildren<StatusEffect>())
        {
            if (!(stale is ThrottleStatus) && !(stale is OverclockStatus))
                continue;
            var staleCondition = stale.GetComponentInChildren<StatusCondition>();
            if (staleCondition != null)
                staleCondition.Remove();
        }

        yield return null;

        // Baseline: an unmodified gain lands whole
        stats.SetValue(StatTypes.CTR, 0, false);
        stats[StatTypes.CTR] += 100;
        Check("plain CT gain unmodified", stats[StatTypes.CTR] == 100, "CTR " + stats[StatTypes.CTR]);

        // Overclock: the configured 1.5x, not the old hard-coded 2x
        var overclock = status.Add<OverclockStatus, DurationStatusCondition>();
        overclock.duration = 9;
        stats.SetValue(StatTypes.CTR, 0, false);
        stats[StatTypes.CTR] += 100;
        Check("overclock applies 1.5x CT", stats[StatTypes.CTR] == 150, "CTR " + stats[StatTypes.CTR]);
        overclock.Remove();
        yield return null;

        // Throttle: half CT gain
        var throttle = status.Add<ThrottleStatus, DurationStatusCondition>();
        throttle.duration = 9;
        stats.SetValue(StatTypes.CTR, 0, false);
        stats[StatTypes.CTR] += 100;
        Check("throttle applies 0.5x CT", stats[StatTypes.CTR] == 50, "CTR " + stats[StatTypes.CTR]);

        // Both at once: delta multipliers compose to 0.75x in either order
        var combined = status.Add<OverclockStatus, DurationStatusCondition>();
        combined.duration = 9;
        stats.SetValue(StatTypes.CTR, 0, false);
        stats[StatTypes.CTR] += 100;
        Check("overclock + throttle compose", stats[StatTypes.CTR] == 75, "CTR " + stats[StatTypes.CTR]);

        combined.Remove();
        throttle.Remove();
        yield return null;

        // Clean exit: gains return to normal and the sheet is restored
        stats.SetValue(StatTypes.CTR, 0, false);
        stats[StatTypes.CTR] += 100;
        Check("tempo statuses detach", stats[StatTypes.CTR] == 100, "CTR " + stats[StatTypes.CTR]);
        stats.SetValue(StatTypes.CTR, savedCT, false);
    }

    // A CT-frozen victim never begins a turn, so its control (and Steeled)
    // conditions must expire through the frozen-window fallback — the #12
    // failure this contract exists to close — while an unfrozen bystander's
    // statuses must never fallback-tick, and a fresh inflict must survive a
    // partial window. Publishes whole rounds of TurnCompletedEvents, so it
    // runs after every other probe.
    private IEnumerator ProbeControlExpiry(BattleController bc)
    {
        var rogue = Find(bc, "Enemy Rogue");
        var hania = Find(bc, "Hania");
        var clock = bc.GetComponent<BattleClock>();
        if (rogue == null || hania == null || clock == null)
        {
            Check("expiry cast present", false);
            yield break;
        }

        var frozen = StatusRegistry.Inflict(rogue, "FreezeFrame", 2);
        // Unfrozen bystander: same battle time passes, nothing may tick
        var bystander = StatusRegistry.Inflict(hania, "Shredded", 2);
        Check("freezeframe inflicted", frozen != null && bystander != null);
        if (frozen == null || bystander == null)
            yield break;

        var steeled = rogue.GetComponentInChildren<SteeledStatus>();
        var steeledCondition = steeled != null ? steeled.GetComponentInChildren<DurationStatusCondition>() : null;
        Check("steeled accompanies control", steeledCondition != null);
        if (steeledCondition == null)
            yield break;

        // Late-inflict guard: a partial window (one activation) must not tick
        var reporter = bc.units[0];
        reporter.Publish(new TurnCompletedEvent(reporter));
        Check("partial window does not tick", frozen.duration == 2, "duration " + frozen.duration);

        // Complete exactly two frozen windows: the control (2) runs out, and
        // Steeled must ALSO count both denied turns — including the window in
        // which the control removed itself mid-event — leaving exactly 1
        for (var i = 0; i < clock.RoundLength * 2 - 1; i++)
            reporter.Publish(new TurnCompletedEvent(reporter));

        // Status removal destroys deferred
        yield return null;
        yield return null;

        Check("frozen control expired", rogue.GetComponentInChildren<FreezeFrameStatus>() == null);
        Check("steeled counts the control's final window", steeledCondition != null && steeledCondition.duration == 1,
            steeledCondition != null ? "duration " + steeledCondition.duration : "condition gone");

        // Unfrozen again, the fallback stays quiet; the rogue's next real
        // turn spends Steeled's last point
        rogue.Publish(new TurnBeganEvent(rogue));
        yield return null;
        yield return null;

        Check("steeled expires on the next real turn", rogue.GetComponentInChildren<SteeledStatus>() == null);
        Check("unfrozen bystander never fallback-ticks", bystander != null && bystander.duration == 2,
            bystander != null ? "duration " + bystander.duration : "condition gone");
        bystander.Remove();
    }

    // ---- 1.8: clock + reinforcements (mutates state — runs last) -----------

    private void ProbeClockAndWaves(BattleController bc)
    {
        var def = bc.testBattle;
        var clock = bc.GetComponent<BattleClock>();
        if (def == null || clock == null || def.waves.Count == 0)
            return;

        var wave = def.waves[0];
        if (clock.CurrentRound >= wave.round)
            return;

        var before = bc.units.Count;
        var turnsPerRound = before;
        for (var i = 0; i < turnsPerRound * (wave.round - clock.CurrentRound); i++)
            bc.units[0].Publish(new TurnCompletedEvent(bc.units[0]));

        Check("clock reached wave round", clock.CurrentRound >= wave.round,
            "round " + clock.CurrentRound);
        Check("reinforcements arrived", bc.units.Count == before + wave.spawns.Count,
            $"{before} -> {bc.units.Count}");
    }
}
#endif
