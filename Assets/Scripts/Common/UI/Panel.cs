using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LayoutAnchor))]
public class Panel : MonoBehaviour
{
    #region Sub Types

    [Serializable]
    public class Position
    {
        public TextAnchor myAnchor;
        public string name;
        public Vector2 offset;
        public TextAnchor parentAnchor;

        public Position(string name)
        {
            this.name = name;
        }

        public Position(string name, TextAnchor myAnchor, TextAnchor parentAnchor) : this(name)
        {
            this.myAnchor = myAnchor;
            this.parentAnchor = parentAnchor;
        }

        public Position(string name, TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset) : this(name,
            myAnchor, parentAnchor)
        {
            this.offset = offset;
        }
    }

    #endregion

    #region Fields / Properties

    [SerializeField] private List<Position> positionList;
    private Dictionary<string, Position> positionMap;
    private LayoutAnchor anchor;

    public Position CurrentPosition { get; private set; }
    public Tweener Transition { get; private set; }
    public bool InTransition => Transition != null;

    public Position this[string name]
    {
        get
        {
            EnsureInitialized();
            return positionMap.ContainsKey(name) ? positionMap[name] : null;
        }
    }

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// Builds the position map on demand. Awake never runs if the object is
    /// deactivated before its first frame, but other components may still
    /// call into this panel — lazy init keeps that safe.
    /// </summary>
    private void EnsureInitialized()
    {
        if (positionMap != null)
            return;

        anchor = GetComponent<LayoutAnchor>();
        positionMap = new Dictionary<string, Position>(positionList.Count);
        for (var i = positionList.Count - 1; i >= 0; i--)
            positionMap[positionList[i].name] = positionList[i];
    }

    private void Start()
    {
        if (CurrentPosition == null && positionList.Count > 0)
            SetPosition(positionList[0], false);
    }

    #endregion

    #region Public

    public void AddPosition(Position p)
    {
        EnsureInitialized();
        positionMap[p.name] = p;
    }

    public void RemovePosition(Position p)
    {
        if (positionMap.ContainsKey(p.name))
            positionMap.Remove(p.name);
    }

    public Tweener SetPosition(string positionName, bool animated)
    {
        return SetPosition(this[positionName], animated);
    }

    public Tweener SetPosition(Position p, bool animated)
    {
        EnsureInitialized();
        CurrentPosition = p;
        if (CurrentPosition == null)
            return null;

        if (InTransition)
            Transition.Stop();

        if (animated)
        {
            Transition = anchor.MoveToAnchorPosition(p.myAnchor, p.parentAnchor, p.offset);
            return Transition;
        }

        anchor.SnapToAnchorPosition(p.myAnchor, p.parentAnchor, p.offset);
        return null;
    }

    #endregion
}