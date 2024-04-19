using System.Collections;
using UnityEngine;

public class SkyFire_Phase1State : BaseState
{
    
    public SkyFire_Phase1State(BaseStateMachine context, BaseStateProvider provider) 
        : base(context, provider)
    {
        IsRootState = false;
    }

    public override void EnterState()
    {
        Debug.Log("Enter Phase1");

        // 이전 사용중인 패턴 멈추기
        Context.Entity.enemyPhaseStarter.StopBullet();

        if (Context.Entity.gameObject.activeSelf)
        {
            Context.Entity.enemyPhaseStarter.StartPattern(0, 4, 1); // 다연발 투사체
            Context.Entity.enemyPhaseStarter.StartPattern(0, 4, 2);
        }
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // 페이즈 끄기
    }

    public override void CheckSwitchStates()
    {
        if (Context.Entity.CurrentHelth <= Context.Entity.Data.maxHealth * 0.6f)
        {
            SwitchState(Provider.GetState(SkyFire_States.Phase2));
        }
    }

    public override void InitializeSubState()
    {
    }
}
