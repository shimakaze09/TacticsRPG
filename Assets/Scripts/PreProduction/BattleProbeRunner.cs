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
            ProbeTargetFilters(bc);
            ProbeLevelScaling();
            ProbeWeaponBehavior(bc);
            ProbeTraits(bc);
            ProbeElementsAndCrits(bc);
            // Status removal destroys deferred, so this block yields frames
            yield return StartCoroutine(ProbeControlStatuses(bc));
            ProbeTerrain(bc);
            ProbeClockAndWaves(bc); // mutates turn count — keep last
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

            // Save repair scrubs leaked ids and nothing else
            memory.learnedAbilities.Add("__probe_leaked_ability__");
            var removed = memory.RepairLearnedAbilities(progress, jm.allJobs);
            Check("repair scrubs leaked id " + u.name,
                removed == 1 && !memory.HasLearnedAbility("__probe_leaked_ability__"),
                "removed " + removed);
            Check("repair keeps justified " + u.name, memory.GetLearnedAbilityCount() == before,
                $"{before} -> {memory.GetLearnedAbilityCount()}");
        }
    }

    // ---- issue #52: spawn level drives combat stats -------------------------

    // Golden checks for the progression model: units of the same recipe
    // spawned at levels 1/10/30/99 must differ by exactly the documented
    // level-growth term (ProgressionModel), across striker/skirmisher/caster
    // archetypes. MHP is only checked monotonically because difficulty may
    // scale enemy HP; ATK/MAT/SPD/MMP are never difficulty-scaled.
    private void ProbeLevelScaling()
    {
        string[] recipes = { "Enemy Warrior", "Enemy Rogue", "Enemy Wizard" };
        int[] levels = { 1, 10, 30, 99 };
        var exactStats = new[] { StatTypes.MMP, StatTypes.ATK, StatTypes.MAT, StatTypes.SPD };

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

                Check($"{recipe} L{levels[i]} MHP grows",
                    s[StatTypes.MHP] > baseStats[StatTypes.MHP],
                    $"{baseStats[StatTypes.MHP]} -> {s[StatTypes.MHP]}");

                for (var k = 0; k < exactStats.Length; k++)
                {
                    var statIndex = System.Array.IndexOf(JobManager.statOrder, exactStats[k]);
                    var expected = baseStats[exactStats[k]] +
                                   ProgressionModel.LevelGrowthBonus(job, statIndex, levels[i]);
                    expected = Mathf.Min(expected,
                        exactStats[k] == StatTypes.MMP ? StatLimits.MaxMP : StatLimits.MaxPrimaryStat);
                    Check($"{recipe} L{levels[i]} {exactStats[k]} golden",
                        s[exactStats[k]] == expected,
                        $"expected {expected}, got {s[exactStats[k]]}");
                }
            }

            foreach (var go in spawned)
                if (go != null)
                    Destroy(go);
        }
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
        Destroy(holder);
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
