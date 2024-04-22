
public class Ball_CombatState : BaseState
{
    public Ball_CombatState(BaseStateMachine context, BaseStateProvider provider) : base(context, provider)
    {
        IsRootState = false;
    }
    public override void EnterState()
    {
        Context.Entity.GetDamaged(100);
    }

    public override void UpdateState() 
    {
            
    }

    public override void ExitState(){}

    public override void CheckSwitchStates() { }

    public override void InitializeSubState(){}  
}
