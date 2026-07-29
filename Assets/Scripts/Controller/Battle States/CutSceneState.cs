using System;
using UnityEngine;

/// <summary>
/// Battle state: plays the intro/outro conversation, then continues to the
/// first turn or battle end.
/// </summary>
public class CutSceneState : BattleState
{
    private ConversationController conversationController;
    private ConversationData data;

    protected override void Awake()
    {
        base.Awake();
        conversationController = owner.GetComponentInChildren<ConversationController>();
    }

    public override void Enter()
    {
        base.Enter();
        data = IsBattleOver()
            ? Resources.Load<ConversationData>(DidPlayerWin()
                ? "Conversations/OutroSceneWin"
                : "Conversations/OutroSceneLose")
            : Resources.Load<ConversationData>("Conversations/IntroScene");

        conversationController.Show(data);
    }

    public override void Exit()
    {
        base.Exit();
        if (data)
            Resources.UnloadAsset(data);
    }

    protected override void AddListeners()
    {
        // Cutscenes are always advanced by the player, even when the battle
        // ended on a computer-driven turn, so bypass the driver gate in
        // BattleState.AddListeners and subscribe unconditionally.
        InputController.moveEvent += OnMove;
        InputController.fireEvent += OnFire;
        ConversationController.completeEvent += OnCompleteConversation;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();
        ConversationController.completeEvent -= OnCompleteConversation;
    }

    protected override void OnFire(object sender, InfoEventArgs<int> e)
    {
        base.OnFire(sender, e);
        conversationController.Next();
    }

    private void OnCompleteConversation(object sender, EventArgs e)
    {
        if (IsBattleOver())
            owner.ChangeState<EndBattleState>();
        else
            owner.ChangeState<SelectUnitState>();
    }
}