using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbilityEffect : MonoBehaviour
{
    public abstract int Predict(Tile target);
    public abstract void Apply(Tile target);
}