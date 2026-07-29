using UnityEngine;

/// <summary>
/// Asset describing a spawnable unit: model, job id, base attack, alliance, AI
/// strategy, locomotion, and element.
/// </summary>
public class UnitRecipe : ScriptableObject
{
    public string abilityCatalog;
    public Alliances alliance;
    public string attack;
    public string element;
    public string job;
    public Locomotions locomotion;
    public string model;
    public string strategy;
}