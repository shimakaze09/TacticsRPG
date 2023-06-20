using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LayoutAnchor))]
public class Panel : MonoBehaviour
{
    #region Sub Types

    [Serializable]
    public class Position
    {
        public string name;
        public TextAnchor myAnchor;
        public TextAnchor parentAnchor;
        public Vector2 offset;

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

    public Position this[string name] => positionMap.ContainsKey(name) ? positionMap[name] : null;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        anchor = GetComponent<LayoutAnchor>();
        positionMap = new Dictionary<string, Position>(positionList.Count);
        for (var i = positionList.Count - 1; i >= 0; i--)
            AddPosition(positionList[i]);
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