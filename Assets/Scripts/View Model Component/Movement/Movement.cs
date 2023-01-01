using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Movement : MonoBehaviour
{
    #region Properties

    public int range;
    public int jumpHeight;
    protected Unit unit;
    protected Transform jumper;

    #endregion

    #region MonoBehaviour

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        jumper = transform.Find("Jumper");
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
        for (var i = tiles.Count - 1; i >= 0; --i)
            if (tiles[i].content != null)
                tiles.RemoveAt(i);
    }

    protected virtual IEnumerator Turn(Directions dir)
    {
        var t = (TransformLocalEulerTweener)transform.RotateToLocal(dir.ToEuler(), 0.25f,
            EasingEquations.EaseInOutQuad);

        // When rotating between North and West, we must make an exception so it looks like the unit
        // rotates the most efficient way (since 0 and 360 are treated the same)
        if (Mathf.Approximately(t.startValue.y, 0f) && Mathf.Approximately(t.endValue.y, 270f))
            t.startValue = new Vector3(t.startValue.x, 360f, t.startValue.z);
        else if (Mathf.Approximately(t.startValue.y, 270) && Mathf.Approximately(t.endValue.y, 0))
            t.endValue = new Vector3(t.startValue.x, 360f, t.startValue.z);

        unit.dir = dir;

        while (t != null)
            yield return null;
    }

    #endregion
}