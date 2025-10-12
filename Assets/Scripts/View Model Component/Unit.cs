using UnityEngine;

public class Unit : MonoBehaviour, IDataPersistence
{
    private string _name;
    public Directions dir;
    private Rank rank;
    private Stats stats;
    public Tile tile { get; protected set; }

    public void LoadData(GameData data)
    {
        stats ??= GetComponent<Stats>();
        if (stats != null && data.unitLevel.TryGetValue(_name, out var exp))
            stats.SetValue(StatTypes.EXP, exp, false);
    }

    public void SaveData(ref GameData data)
    {
        rank ??= GetComponent<Rank>();
        if (rank == null)
            return;

        if (data.unitLevel.ContainsKey(_name))
            data.unitLevel.Remove(_name);
        data.unitLevel.Add(_name, rank.EXP);
    }

    private void Awake()
    {
        _name = gameObject.name;
        stats = GetComponent<Stats>();
        rank = GetComponent<Rank>();
    }

    private void OnEnable()
    {
        DataPersistenceManager.Register(this);
    }

    private void OnDisable()
    {
        DataPersistenceManager.Unregister(this);
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
}