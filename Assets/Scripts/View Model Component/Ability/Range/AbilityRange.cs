﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbilityRange : MonoBehaviour
{
    public int horizontal = 1;
    public int vertical = int.MaxValue;
    public virtual bool positionOriented => true;
    public virtual bool directionOriented => false;
    protected Unit unit => GetComponentInParent<Unit>();

    public abstract List<Tile> GetTilesInRange(Board board);
}