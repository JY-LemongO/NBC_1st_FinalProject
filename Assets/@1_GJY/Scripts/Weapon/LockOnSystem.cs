using Cinemachine;
using System;
using UnityEngine;

[Serializable]
public class LockOnSystem
{
    // To - Do List
    // 기능 1. 락온시스템 : 마우스 휠 Started시, SphereCast로 주변 적 검색.
    // 기능 2. 타겟팅 시스템 : 다중 락온시스템에 의해 검색된 적들 중 가장 가까운 적 타겟팅
    // 기능 3. 카메라 추적 시스템 : 타겟팅 된 적에게 Cinemachine Cam - Look At Transform 이 해당 적으로 변경.    
    private PlayerStateMachine _stateMachine;

    [SerializeField] Transform _followOnTargetMode;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] private float _scanRange;

    public CinemachineFreeLook FollowCam { get; private set; }    
    public CinemachineVirtualCamera LockOnCam { get; private set; }
    public CinemachineTargetGroup TargetGroup { get; private set; }

    public Transform TargetEnemy { get; private set; }    

    public static event Action<Transform> OnLockOn;
    public static event Action OnRelease;    

    public void Setup(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        // 시네머신 카메라 초기화
        FollowCam = GameObject.Find("@FollowCam").GetComponent<CinemachineFreeLook>();
        LockOnCam = GameObject.Find("@LockOnCam").GetComponent<CinemachineVirtualCamera>();
        TargetGroup = GameObject.Find("@TargetGroup").GetComponent<CinemachineTargetGroup>();       

        FollowCam.Follow = _stateMachine.transform;
        FollowCam.LookAt = _stateMachine.transform;

        LockOnCam.Follow = _followOnTargetMode;
        LockOnCam.LookAt = TargetGroup.transform;
        LockOnCam.gameObject.SetActive(false);

        TargetGroup.AddMember(_stateMachine.transform, 1, 0);
        TargetGroup.AddMember(_followOnTargetMode, 1, 0);
    }

    public bool IsThereEnemyScanned()
    {
        Vector3 origin = Camera.main.transform.position;
        RaycastHit[] hits = Physics.SphereCastAll(origin, _scanRange, Camera.main.transform.forward, 50f, _targetLayer);
        if (hits.Length == 0)
        {
            Debug.Log("현재 조준시스템에 포착된 적이 없습니다.");
            return false;
        }

        int closestIndex = GetClosestTargetIndex(hits);
        TargetEnemy = hits[closestIndex].transform.GetComponent<Test_Enemy>().transform;
        return true;
    }

    private int GetClosestTargetIndex(RaycastHit[] hits)
    {
        float closestDist = float.MaxValue;
        int closestIndex = -1;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].distance < closestDist)
            {
                closestIndex = i;
                closestDist = hits[i].distance;
            }
        }

        return closestIndex;
    }

    public void LockOnTarget()
    {
        OnLockOn.Invoke(TargetEnemy);
        LockOnCam.gameObject.SetActive(true);
        TargetGroup.AddMember(TargetEnemy, 1, 0);        
    }

    public void ReleaseTarget()
    {
        FollowCam.m_XAxis.Value = LockOnCam.transform.rotation.eulerAngles.y;        

        OnRelease.Invoke();
        LockOnCam.gameObject.SetActive(false);
        TargetGroup.RemoveMember(TargetEnemy);
        TargetEnemy = null;
    }
}
