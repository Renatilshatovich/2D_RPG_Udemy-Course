using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimSwordState : PlayerState
{
    public PlayerAnimSwordState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        player.skill.sword.DotsActive(true);
    }

    public override void Update()
    {
        base.Update();
        
        if(Input.GetKeyDown(KeyCode.Mouse1))
            stateMachine.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
