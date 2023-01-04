using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BattleState : State
{
    protected BattleController owner;
    public CameraRig cameraRig => owner.cameraRig;
    public Board board => owner.board;
    public LevelData levelData => owner.levelData;
    public Transform tileSelectionIndicator => owner.tileSelectionIndicator;
    public AbilityMenuPanelController abilityMenuPanelController => owner.abilityMenuPanelController;
    public Turn turn => owner.turn;
    public List<Unit> units => owner.units;

    public Point pos
    {
        get => owner.pos;
        set => owner.pos = value;
    }

    protected virtual void Awake()
    {
        owner = GetComponent<BattleController>();
    }

    protected override void AddListeners()
    {
        InputController.moveEvent += OnMove;
        InputController.fireEvent += OnFire;
    }

    protected override void RemoveListeners()
    {
        InputController.moveEvent -= OnMove;
        InputController.fireEvent -= OnFire;
    }

    protected virtual void OnMove(object sender, InfoEventArgs<Point> e)
    {
    }

    protected virtual void OnFire(object sender, InfoEventArgs<int> e)
    {
    }

    protected virtual void SelectTile(Point p)
    {
        if (pos == p || !board.tiles.ContainsKey(p))
            return;

        pos = p;
        tileSelectionIndicator.localPosition = board.tiles[p].center;
    }
}