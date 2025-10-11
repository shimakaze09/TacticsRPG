using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, IDataPersistence
{
    public Tile tile { get; protected set; }
    public Directions dir;

    private string _name;

    private void Start()
    {
        _name = gameObject.name;
    }

    public void Place(Tile target)
    {
        // Make sure old tile location is not still pointing to this unit
        if (tile != null && tile.content == gameObject)
            tile.content = null;

        // Link unit and tile references
        tile = target;

        if (target != null)
            target.content = gameObject;
    }

    public void Match()
    {
        transform.localPosition = tile.center;
        transform.localEulerAngles = dir.ToEuler();
    }

    public void LoadData(GameData data)
    {
        if (data.unitLevel.TryGetValue(_name, out var exp))
            GetComponent<Stats>().SetValue(StatTypes.EXP, exp, false);
    }

    public void SaveData(ref GameData data)
    {
        if (data.unitLevel.ContainsKey(_name))
            data.unitLevel.Remove(_name);
        data.unitLevel.Add(_name, GetComponent<Rank>().EXP);
    }
}