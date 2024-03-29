﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Party = System.Collections.Generic.List<UnityEngine.GameObject>;

public static class ExperienceManager
{
    private const float minLevelBonus = 1.5f;
    private const float maxLevelBonus = 0.5f;

    public static void AwardExperience(int amount, Party party)
    {
        // Grab a list of all of the rank components from our hero party
        var ranks = new List<Rank>(party.Count);
        ranks.AddRange(party.Select(t => t.GetComponent<Rank>()).Where(r => r != null));

        // Step 1: determine the range in actor level stats
        var min = int.MaxValue;
        var max = int.MinValue;
        foreach (var rank in ranks)
        {
            min = Mathf.Min(rank.LVL, min);
            max = Mathf.Max(rank.LVL, max);
        }

        // Step 2: weight the amount to award per actor based on their level
        var weights = new float[ranks.Count];
        float summedWeights = 0;
        for (var i = ranks.Count - 1; i >= 0; i--)
        {
            var percent = (float)(ranks[i].LVL - min) / (max - min);
            weights[i] = Mathf.Lerp(minLevelBonus, maxLevelBonus, percent);
            summedWeights += weights[i];
        }

        // Step 3: hand out the weighted award
        for (var i = ranks.Count - 1; i >= 0; i--)
        {
            var subAmount = Mathf.FloorToInt(weights[i] / summedWeights * amount);
            ranks[i].EXP += subAmount;
        }
    }
}