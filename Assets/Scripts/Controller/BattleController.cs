using UnityEngine;
using System.Collections;

public class BattleController : StateMachine
{
    public GameObject heroPrefab;
    public CameraRig cameraRig;
    public Board board;
    public LevelData levelData;
    public Transform tileSelectionIndicator;
    public Point pos;
    public Unit currentUnit;
    public Tile currentTile => board.GetTile(pos);

    private void Start()
    {
        ChangeState<InitBattleState>();
    }
}