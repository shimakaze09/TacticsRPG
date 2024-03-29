﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleController : StateMachine
{
    public CameraRig cameraRig;
    public Board board;
    public LevelData levelData;
    public Transform tileSelectionIndicator;
    public Point pos;
    public Tile currentTile => board.GetTile(pos);
    public AbilityMenuPanelController abilityMenuPanelController;
    public StatPanelController statPanelController;
    public HitSuccessIndicator hitSuccessIndicator;
    public BattleMessageController battleMessageController;
    public FacingIndicator facingIndicator;
    public Turn turn = new();
    public List<Unit> units = new();
    public IEnumerator round;
    public ComputerPlayer cpu;

    private void Start()
    {
        ChangeState<InitBattleState>();
    }
}