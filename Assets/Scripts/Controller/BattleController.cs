using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : StateMachine
{
    public AbilityMenuPanelController abilityMenuPanelController;
    public BattleMessageController battleMessageController;
    public Board board;
    public CameraRig cameraRig;
    public ComputerPlayer cpu;
    public FacingIndicator facingIndicator;
    public HitSuccessIndicator hitSuccessIndicator;
    public LevelData levelData;
    public Point pos;
    public IEnumerator round;
    public StatPanelController statPanelController;
    public Transform tileSelectionIndicator;
    public Turn turn = new();
    public List<Unit> units = new();
    public Tile currentTile => board.GetTile(pos);

    private void Start()
    {
        ChangeState<InitBattleState>();
    }
}