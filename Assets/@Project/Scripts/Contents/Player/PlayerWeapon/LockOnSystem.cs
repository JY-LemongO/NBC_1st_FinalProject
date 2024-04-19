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
    private Module _module;

    [SerializeField] Transform _followOnTargetMode;
    [SerializeField] LayerMask _targetLayer;
    [field: SerializeField] public float ScanRange;
    [field: SerializeField] public float ScanRadius;

    public CinemachineFreeLook FollowCam { get; private set; }
    public CinemachineVirtualCamera LockOnCam { get; private set; }
    public CinemachineTargetGroup TargetGroup { get; private set; }

    public bool IsLockon { get; private set; }
    public ITarget TargetEnemy { get; private set; }

    private readonly float INIT_CAM_POS_Y = 0.5f;
    private readonly float INIT_CAM_POS_X = -8f;

    public void Setup(Module module)
    {
        _module = module;
        ScanRange *= module.ModuleStatus.ScanRangeAdjust;

        // 시네머신 카메라 초기화
        FollowCam = GameObject.Find("@FollowCam").GetComponent<CinemachineFreeLook>();
        LockOnCam = GameObject.Find("@LockOnCam").GetComponent<CinemachineVirtualCamera>();
        TargetGroup = GameObject.Find("@TargetGroup").GetComponent<CinemachineTargetGroup>();

        FollowCam.Follow = _module.transform;
        FollowCam.LookAt = _module.transform;

        LockOnCam.Follow = _followOnTargetMode;
        LockOnCam.LookAt = TargetGroup.transform;
        LockOnCam.gameObject.SetActive(false);

        TargetGroup.AddMember(_module.transform, 1, 0);
        TargetGroup.AddMember(_followOnTargetMode, 1, 0);

        FollowCam.m_YAxis.Value = INIT_CAM_POS_Y;
        FollowCam.m_XAxis.Value = INIT_CAM_POS_X;
    }    

    public bool IsThereEnemyScanned()
    {
        Vector3 origin = _module.transform.position + Camera.main.transform.forward * ScanRadius * 0.5f;
        Vector3 dir;
        RaycastHit[] hits;
        if (IsLockon)
        {
            Vector3 camdir = Camera.main.transform.forward;
            dir = new Vector3(camdir.x, 0, camdir.z).normalized;
            Debug.Log(dir);
            hits = Physics.SphereCastAll(origin, ScanRadius, dir, ScanRange, _targetLayer);
        }
        else
        {
            dir = Camera.main.transform.forward;
            hits = Physics.SphereCastAll(origin, ScanRadius, dir, ScanRange, _targetLayer);            
        }

        if (hits.Length == 0)
        {
            Debug.Log("현재 조준시스템에 포착된 적이 없습니다.");
            return false;
        }

        int closestIndex = GetClosestTargetIndex(hits);

        if (hits[closestIndex].transform.TryGetComponent(out ITarget target) == false)
            return false;
        if (target == TargetEnemy || !target.IsAlive)
            return false;

        TargetEnemy = target;
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
        IsLockon = true;

        float percent = TargetEnemy.AP / TargetEnemy.MaxAP;
        Managers.ActionManager.CallLockOn(TargetEnemy.Transform, percent);
        LockOnCam.gameObject.SetActive(true);
        TargetGroup.AddMember(TargetEnemy.Transform, 1, 0);

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Player_LockOn, Vector3.zero);
    }

    public void ReleaseTarget()
    {
        IsLockon = false;
        FollowCam.m_XAxis.Value = LockOnCam.transform.rotation.eulerAngles.y;

        Managers.ActionManager.CallRelease();
        LockOnCam.gameObject.SetActive(false);
        TargetGroup.RemoveMember(TargetEnemy.Transform);
        TargetEnemy = null;
    }

    public void LockTargetChange(ITarget prevTarget)
    {
        if (TargetEnemy == null)
            return;

        if (!IsThereEnemyScanned())
        {
            ReleaseTarget();
            return;
        }

        TargetGroup.RemoveMember(prevTarget.Transform);
        LockOnTarget();
    }
}
