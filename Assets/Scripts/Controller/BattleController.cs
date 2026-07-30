using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene root for a battle: holds the board, camera, UI controllers, turn data,
/// and unit list, and runs the battle state machine starting at
/// InitBattleState.
/// </summary>
public class BattleController : StateMachine
{
    public AbilityMenuPanelController abilityMenuPanelController;
    public BattleMessageController battleMessageController;
    public Board board;

    /// <summary>
    /// Dev/test override: play this authored battle when the game flow has
    /// no pending battle (e.g. when the Battle scene is played directly).
    /// Null = writ-style random generation.
    /// </summary>
    public BattleDefinition testBattle;
    public CameraRig cameraRig;
    public ComputerPlayer cpu;
    public FacingIndicator facingIndicator;
    public HitSuccessIndicator hitSuccessIndicator;
    public LevelData levelData;
    public Point pos;
    public IEnumerator round;
    public StatPanelController statPanelController;
    public Transform tileSelectionIndicator;
    [System.NonSerialized] public Turn turn = new();
    public List<Unit> units = new();
    public Tile currentTile => board.GetTile(pos);

    private void Start()
    {
        ChangeState<InitBattleState>();
    }
}