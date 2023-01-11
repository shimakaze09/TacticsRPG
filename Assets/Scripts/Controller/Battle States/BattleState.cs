using UnityEngine;
using System.Collections.Generic;

public abstract class BattleState : State
{
    protected BattleController owner;
    protected Driver driver;
    public CameraRig cameraRig => owner.cameraRig;
    public Board board => owner.board;
    public LevelData levelData => owner.levelData;
    public Transform tileSelectionIndicator => owner.tileSelectionIndicator;
    public Tile currentTile => owner.currentTile;
    public AbilityMenuPanelController abilityMenuPanelController => owner.abilityMenuPanelController;
    public StatPanelController statPanelController => owner.statPanelController;
    public HitSuccessIndicator hitSuccessIndicator => owner.hitSuccessIndicator;
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
        if (driver == null || driver.Current == Drivers.Human)
        {
            InputController.moveEvent += OnMove;
            InputController.fireEvent += OnFire;
        }
    }

    protected override void RemoveListeners()
    {
        InputController.moveEvent -= OnMove;
        InputController.fireEvent -= OnFire;
    }

    public override void Enter()
    {
        driver = turn.actor != null ? turn.actor.GetComponent<Driver>() : null;
        base.Enter();
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

    protected virtual Unit GetUnit(Point p)
    {
        var t = board.GetTile(p);
        var content = t != null ? t.content : null;
        return content != null ? content.GetComponent<Unit>() : null;
    }

    protected virtual void RefreshPrimaryStatPanel(Point p)
    {
        var target = GetUnit(p);
        if (target != null)
            statPanelController.ShowPrimary(target.gameObject);
        else
            statPanelController.HidePrimary();
    }

    protected virtual void RefreshSecondaryStatPanel(Point p)
    {
        var target = GetUnit(p);
        if (target != null)
            statPanelController.ShowSecondary(target.gameObject);
        else
            statPanelController.HideSecondary();
    }

    protected virtual bool DidPlayerWin()
    {
        return owner.GetComponent<BaseVictoryCondition>().Victor == Alliances.Hero;
    }

    protected virtual bool IsBattleOver()
    {
        return owner.GetComponent<BaseVictoryCondition>().Victor != Alliances.None;
    }
}