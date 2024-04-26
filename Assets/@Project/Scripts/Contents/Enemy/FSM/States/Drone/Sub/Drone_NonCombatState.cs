using UnityEngine;

public class Drone_NonCombatState : BaseState
{
    public Drone_NonCombatState(BaseStateMachine context, BaseStateProvider provider) : base(context, provider)
    {
        IsRootState = false;
    }

    public override void EnterState()
    {
        InitializeSubState();
        _currentSubState?.EnterState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        Vector3 entity = new Vector3(_entityTransform.position.x, 0, _entityTransform.position.z);
        Vector3 target = new Vector3(_targetTransform.position.x, 0, _targetTransform.position.z);

        float distance = Vector3.Distance(entity, target);
        if (Context.Entity.Stat.stopDistance > distance)
        {
            SwitchState(Context.Provider.GetState(Minion_States.Combat));
        }
    }

    public override void InitializeSubState()
    {
        SetSubState(Provider.GetState(Minion_States.Idle));
    }
}
