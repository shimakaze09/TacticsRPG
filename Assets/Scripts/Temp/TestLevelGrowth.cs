using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Party = System.Collections.Generic.List<UnityEngine.GameObject>;

public class TestLevelGrowth : MonoBehaviour
{
    private void OnEnable()
    {
        this.AddObserver(OnLevelChange, Stats.DidChangeNotification(StatTypes.LVL));
        this.AddObserver(OnExperienceException, Stats.WillChangeNotification(StatTypes.EXP));
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnLevelChange, Stats.DidChangeNotification(StatTypes.LVL));
        this.RemoveObserver(OnExperienceException, Stats.WillChangeNotification(StatTypes.EXP));
    }

    private void Start()
    {
        VerifyLevelToExperienceCalculations();
        VerifySharedExperienceDistribution();
    }

    private void VerifyLevelToExperienceCalculations()
    {
        for (var i = 1; i < 100; i++)
        {
            var expLvl = Rank.ExperienceForLevel(i);
            var lvlExp = Rank.LevelForExperience(expLvl);
            Debug.Log(lvlExp != i
                ? $"Mismatch on level:{i} with exp:{expLvl} returned:{lvlExp}"
                : $"Level:{lvlExp} = Exp:{expLvl}");
        }
    }

    private void VerifySharedExperienceDistribution()
    {
        var names = new string[] { "Russell", "Brian", "Josh", "Ian", "Adam", "Andy" };
        var heroes = new Party();
        foreach (var name in names)
        {
            var actor = new GameObject(name);
            actor.AddComponent<Stats>();
            var rank = actor.AddComponent<Rank>();
            rank.Init((int)Random.Range(1, 5));
            heroes.Add(actor);
        }

        Debug.Log("===== Before Adding Experience ======");
        LogParty(heroes);
        Debug.Log("=====================================");
        ExperienceManager.AwardExperience(1000, heroes);
        Debug.Log("===== After Adding Experience ======");
        LogParty(heroes);
    }

    private void LogParty(Party p)
    {
        foreach (var actor in p)
        {
            var rank = actor.GetComponent<Rank>();
            Debug.Log($"Name:{actor.name} Level:{rank.LVL} Exp:{rank.EXP}");
        }
    }

    private void OnLevelChange(object sender, object args)
    {
        var stats = sender as Stats;
        Debug.Log(stats.name + " leveled up!");
    }

    private void OnExperienceException(object sender, object args)
    {
        var actor = (sender as Stats).gameObject;
        var vce = args as ValueChangeException;
        var roll = Random.Range(0, 5);
        switch (roll)
        {
            case 0:
                vce.FlipToggle();
                Debug.Log($"{actor.name} would have received {vce.delta} experience, but we stopped it");
                break;
            case 1:
                vce.AddModifier(new AddValueModifier(0, 1000));
                Debug.Log($"{actor.name} would have received {vce.delta} experience, but we added 1000");
                break;
            case 2:
                vce.AddModifier(new MultValueModifier(0, 2f));
                Debug.Log($"{actor.name} would have received {vce.delta} experience, but we multiplied by 2");
                break;
            default:
                Debug.Log($"{actor.name} will receive {vce.delta} experience");
                break;
        }
    }
}