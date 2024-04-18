public class Ball_AliveState : BaseState
{
    public Ball_AliveState(BaseStateMachine context, BaseStateProvider provider) : base(context, provider)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
       _currentSubState?.EnterState();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Others_Appear, Context.Entity.transform.position);
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void CheckSwitchStates()
    {
        if (Context.Entity.AP <= 0)
            SwitchState(Provider.GetState(Ball_States.Dead));
    }

    public override void ExitState()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Enemy_Down, Context.Entity.transform.position);
    }

    public override void InitializeSubState()
    {
        SetSubState(Provider.GetState(Ball_States.NonCombat));
    }

}
