﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Movement : MonoBehaviour
{
    #region Properties

    public int range => stats[StatTypes.MOV];
    public int jumpHeight => stats[StatTypes.JMP];
    protected Unit unit;
    protected Transform jumper;
    protected Stats stats;

    #endregion

    #region MonoBehaviour

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        jumper = transform.Find("Jumper");
    }

    protected virtual void Start()
    {
        stats = GetComponent<Stats>();
    }

    #endregion

    #region Public

    public virtual List<Tile> GetTilesInRange(Board board)
    {
        var retValue = board.Search(unit.tile, ExpandSearch);
        Filter(retValue);
        return retValue;
    }

    public abstract IEnumerator Traverse(Tile tile);

    #endregion

    #region Protected

    protected virtual bool ExpandSearch(Tile from, Tile to)
    {
        return from.distance + 1 <= range;
    }

    protected virtual void Filter(List<Tile> tiles)
    {
        for (var i = 0; i < tiles.Count; i++)
            if (tiles[i].content != null)
                tiles.RemoveAt(i);
    }

    protected virtual IEnumerator Turn(Directions dir)
    {
        var t = (TransformLocalEulerTweener)transform.RotateToLocal(dir.ToEuler(), 0.25f,
            EasingEquations.EaseInOutQuad);

        // When rotating between North and West, we must make an exception so it looks like the unit
        // rotates the most efficient way (since 0 and 360 are treated the same)
        if (Mathf.Approximately(t.startTweenValue.y, 0f) && Mathf.Approximately(t.endTweenValue.y, 270f))
            t.startTweenValue = new Vector3(t.startTweenValue.x, 360f, t.startTweenValue.z);
        else if (Mathf.Approximately(t.startTweenValue.y, 270f) && Mathf.Approximately(t.endTweenValue.y, 0f))
            t.endTweenValue = new Vector3(t.endTweenValue.x, 360f, t.endTweenValue.z);

        unit.dir = dir;

        while (t != null)
            yield return null;
    }

    #endregion
}