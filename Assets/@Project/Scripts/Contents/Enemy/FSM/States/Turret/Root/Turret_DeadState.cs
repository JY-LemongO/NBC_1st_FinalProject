using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_DeadState : BaseState
{
    public Turret_DeadState(BaseStateMachine context, BaseStateProvider provider) : base(context, provider)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
    }

    public override void InitializeSubState()
    {
    }
}
