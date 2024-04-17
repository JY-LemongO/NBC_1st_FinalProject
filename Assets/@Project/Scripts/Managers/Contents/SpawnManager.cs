using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;




public class SpawnManager
{
    public List<Vector3> _groundCell = new List<Vector3>(); // 탐지 전의 모든 지상 셀 (어떤 요소와도 겹치지 않음)

    private HashSet<Vector3> _groundTempPoint = new HashSet<Vector3>(); // 탐지 후 중복 요소 제거용

    public List<Vector3> _groundSpawnPoints = new List<Vector3>(); // 탐지 후 찾아낸 스폰 포인트

    public Vector3 gridWorldSize; // 맵의 3차원 크기
    public float cellRadius;
    int gridSizeX, gridSizeY, gridSizeZ;
    private float _cellDiameter;

    private List<GameObject> _activatedUnits = new List<GameObject>();

    public SpawnManager()
    {
        gridWorldSize = new Vector3(200, 200, 200);
        cellRadius = 5;

        _cellDiameter = cellRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _cellDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _cellDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / _cellDiameter);
    }


    public void SpawnUnits(List<UnitSpawnInfo> unitSpawnInfos)
    {
        DestroyAllUnit();

        CreateCell();
        DetectSpawnPoint();
        ShuffleSpawnPoint();


        if (_activatedUnits.Count != 0)
            _activatedUnits.Clear();

        int spawnIndex = 0;

        foreach (UnitSpawnInfo spawninfo in unitSpawnInfos)
        {
            string unitType = spawninfo.unitType.ToString();
            for (int i = 0; i < spawninfo.count; ++i)
            {
                _activatedUnits.Add(ObjectPooler.SpawnFromPool(unitType, _groundSpawnPoints[spawnIndex++]));
            }
        }
    }

    public void SpawnBoss()
    {

    }

    private void DestroyAllUnit()
    {
        if (_activatedUnits.Count > 0)
        {
            foreach (GameObject obj in _activatedUnits)
            {
                obj.SetActive(false);
            }
            _activatedUnits.Clear();
        }
    }

    public bool AllUnitDisabled()
    {
        if (_activatedUnits.Count == 0) return true; // 임시

        foreach (GameObject unit in _activatedUnits)
        {
            if (unit.activeSelf) return false;
        }

        return true;
    }

    #region 스폰 포인트 연산
    public void CreateCell() // 그리드의 셀만 만듦
    {
        _groundCell.Clear();
        Vector3 wolrdBottomLeft = Vector3.zero - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2;

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int z = 0; z < gridSizeZ; ++z)
            {
                Vector3 groundSpawnPoint =
                    wolrdBottomLeft +
                    Vector3.right * (x * _cellDiameter + cellRadius) +
                    Vector3.up * 40 +
                    Vector3.forward * (z * _cellDiameter + cellRadius);

                if (!Physics.CheckSphere(groundSpawnPoint, cellRadius / 2))
                {
                    _groundCell.Add(groundSpawnPoint);
                }
            }
        }

        DetectSpawnPoint();
        ShuffleSpawnPoint();
    }

    public void DetectSpawnPoint() // 중복 없는 스폰 포인트 탐지
    {
        // 기존 포인트 지우기 작업
        _groundTempPoint.Clear();

        _groundSpawnPoints.Clear();

        RaycastHit hit;
        foreach (Vector3 cell in _groundCell)
        {
            if (Physics.Raycast(cell, Vector3.down, out hit, 41f))
                _groundTempPoint.Add(hit.point + Vector3.up * 3);
        }
        _groundSpawnPoints = new List<Vector3>(_groundTempPoint);

    }
    #endregion

    public void ShuffleSpawnPoint() // 유틸에 넣기
    {
        if (_groundSpawnPoints.Count <= 0)
            return;

        System.Random random = new System.Random();

        int n = _groundSpawnPoints.Count;

        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            Vector3 value = _groundSpawnPoints[k];
            _groundSpawnPoints[k] = _groundSpawnPoints[n];
            _groundSpawnPoints[n] = value;
        }
    }


    #region 국 작업
    public event Action OnStageClear;

    public StageData StageData { get; private set; }
    public LevelData LevelData { get; private set; }

    public int CurrentSpawnCount { get; private set; }
    public bool IsStarted { get; private set; }

    private float _totaltime;
    private int _currentStage;
    private int _minionKillCount;
    private int _bossKillCount;
    private int _researchPoint;

    public float Timer { get => _totaltime; private set { _totaltime = value; StageData.bestTime = _totaltime; } }
    public int CurrentStage { get => _currentStage; private set { _currentStage = value; StageData.bestStage = _currentStage; } }
    public int MinionKillCount { get => _minionKillCount; private set { _minionKillCount = value; StageData.highestMinionKill = _minionKillCount; } }
    public int BossKillCount { get => _bossKillCount; private set { _bossKillCount = value; StageData.highestBossKill = _bossKillCount; } }
    public int ResearchPoint { get => _researchPoint; private set { _researchPoint = value; StageData.researchPoint = _researchPoint; } }

    private int _currentWaveSpawnCount;
    private float _timer;

    public void Init()
    {
        StageData = new StageData();

        Managers.StageActionManager.OnMinionKilled += () => MinionKillCount++;
        Managers.StageActionManager.OnBossKilled += () => BossKillCount++;
    }

    public IEnumerator Co_TimerOn()
    {
        while (true)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator GameStart()
    {
        if (!IsStarted)
        {
            IsStarted = true;
            CurrentStage = 1;
        }
        int levelCount = Managers.Data.GetLevelDataDictCount();
        CreateCell();

        while (CurrentStage <= levelCount)
        {
            LevelData = Managers.Data.GetLevelData(CurrentStage);

            Debug.Log($"{LevelData.SpawnCount} 마리 스폰 해야됨");

            yield return CoroutineManager.StartCoroutine(Co_SpawnEnemies());
            yield return CoroutineManager.StartCoroutine(Co_StartCountDown());

            if (LevelData.EnemyType == EnemyType.Minion || (LevelData.EnemyType == EnemyType.Boss && CheckStageClear()))
            {
                _currentWaveSpawnCount = 0;
                CurrentStage++;                
                OnStageClear?.Invoke();
                continue;
            }
            yield break;
        }
    }

    public IEnumerator Co_SpawnEnemies()
    {
        while (true)
        {
            yield return Util.GetWaitSeconds(LevelData.SpawnDelayTime);

            string unitType = LevelData.SpawnTypes[UnityEngine.Random.Range(0, LevelData.SpawnTypes.Count)].ToString();
            SpawnEnemy(unitType);

            if (_currentWaveSpawnCount >= LevelData.SpawnCount)
                break;
        }
    }

    public IEnumerator Co_StartCountDown()
    {
        _timer = LevelData.CountDownTime;

        while (_timer > 0)
        {
            _timer -= Time.deltaTime;
            Managers.StageActionManager.CallCountDown(_timer);
            yield return null;
        }
    }

    public void SpawnEnemy(string unitType)
    {
        _currentWaveSpawnCount++;
        CurrentSpawnCount++;

        int index = _groundSpawnPoints.Count % CurrentSpawnCount;
        ObjectPooler.SpawnFromPool(unitType, _groundSpawnPoints[index]).GetComponent<Entity>();
        Managers.StageActionManager.CallEnemySpawned(CurrentSpawnCount);
    }

    public bool CheckStageClear()
    {
        if (CurrentSpawnCount == MinionKillCount + BossKillCount)
            return true;
        return false;
    }

    public void TimeOut()
    {
        IsStarted = false;
        Managers.ActionManager.CallPlayerDead();
    }
    #endregion

    public void Clear()
    {
        OnStageClear = null;
    }
}