using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        if (IsBattleOver())
        {
            if (DidPlayerWin())
                data = Resources.Load<ConversationData>("Conversations/OutroSceneWin");
            else
                data = Resources.Load<ConversationData>("Conversations/OutroSceneLose");
        }
        else
        {
            data = Resources.Load<ConversationData>("Conversations/IntroScene");
        }

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
        base.AddListeners();
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

    private void OnCompleteConversation(object sender, System.EventArgs e)
    {
        if (IsBattleOver())
            owner.ChangeState<EndBattleState>();
        else
            owner.ChangeState<SelectUnitState>();
    }
}