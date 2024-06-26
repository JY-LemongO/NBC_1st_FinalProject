using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ModuleManager
{
    // ToDo - 게임 시작 시 Resources(또는 다른 위치)폴더 내 모든 파츠 부위에 맞게 자료구조에 담기.
    // ToDo - 마지막으로 저장된 파츠 정보를 토대로 현재 Module 정보 생성에 사용.
    // ToDo - MainScene의 Module 창에서 파츠 변경 시 갱신된 정보를 저장.

    public Dictionary<Type, BasePart[]> Modules { get; private set; } = new Dictionary<Type, BasePart[]>();

    public int LowerCount => Modules[typeof(LowerPart)].Length;
    public int UpperCount => Modules[typeof(UpperPart)].Length;
    public int ArmCount => Modules[typeof(ArmsPart)].Length;
    public int ShoulderCount => Modules[typeof(ShouldersPart)].Length;

    #region Events
    public event Action<PartData> OnUpperChange;
    public event Action<PartData> OnLowerChange;
    public event Action<PartData> OnLeftArmChange;
    public event Action<PartData> OnRightArmChange;
    public event Action<PartData> OnLeftShoulderChange;
    public event Action<PartData> OnRightSoulderChange;
    public event Action<string, string> OnInfoChange;

    public void CallUpperPartChange(PartData part) => OnUpperChange?.Invoke(part);
    public void CallLowerPartChange(PartData lower) => OnLowerChange?.Invoke(lower);
    public void CallLeftArmPartChange(PartData lower) => OnLeftArmChange?.Invoke(lower);
    public void CallRightArmPartChange(PartData lower) => OnRightArmChange?.Invoke(lower);
    public void CallLeftShoulderPartChange(PartData lower) => OnLeftShoulderChange?.Invoke(lower);
    public void CallRightShoulderPartChange(PartData lower) => OnRightSoulderChange?.Invoke(lower);
    public void CallInfoChange(string name, string desc) => OnInfoChange?.Invoke(name, desc);
    #endregion

    public Module CurrentModule { get; private set; }
    public LowerPart CurrentLowerPart { get; private set; }
    public UpperPart CurrentUpperPart { get; private set; }
    public ArmsPart CurrentLeftArmPart { get; private set; }
    public ArmsPart CurrentRightArmPart { get; private set; }
    public ShouldersPart CurrentLeftShoulderPart { get; private set; }
    public ShouldersPart CurrentRightShoulderPart { get; private set; }

    private GameManager _gameManager;

    public void Init() // 게임 시작 시 Resources 폴더 내 초기 파츠 담기.
    {
        _gameManager = Managers.GameManager;
        _gameManager.OnReceivePartID += UnlockParts;

        BasePart[] lowerParts = Resources.LoadAll<BasePart>("Prefabs/Parts/Lower");
        BasePart[] upperParts = Resources.LoadAll<BasePart>("Prefabs/Parts/Upper");
        BasePart[] armWeaponParts = Resources.LoadAll<BasePart>("Prefabs/Parts/Weapon_Arm");
        BasePart[] shoulderWeaponParts = Resources.LoadAll<BasePart>("Prefabs/Parts/Weapon_Shoulder");

        Modules.Add(typeof(LowerPart), lowerParts);
        Modules.Add(typeof(UpperPart), upperParts);
        Modules.Add(typeof(ArmsPart), armWeaponParts);
        Modules.Add(typeof(ShouldersPart), shoulderWeaponParts);

        InitUnlock();
    }

    private void InitUnlock()
    {
        IEnumerator allPartDatas = Managers.Data.GetPartDataEnumerator();

        while (allPartDatas.MoveNext())
        {
            KeyValuePair<int, PartData> currentPartData = (KeyValuePair<int, PartData>)allPartDatas.Current;

            int id = currentPartData.Key;
            bool initPart = currentPartData.Value.InitialPart;

            if (initPart)
                UnlockParts(id);
        }

        foreach (int id in _gameManager.UnlockedPartsIDList)
            UnlockParts(id);
    }

    public void UnlockParts(int id) => Managers.Data.GetPartData(id).Unlock();

    // Player 또는 Selector 빈 모듈 생성 : Scene Script에서 처리하는게 맞다고 봄.
    public void CreatePlayerModule() => CreateEmptyModule("Prefabs/Parts/Player_Module");
    public void CreateSelectorModule() => CreateEmptyModule("Prefabs/Parts/Selector_Module");

    public void CreateEmptyModule(string path)
    {
        GameObject emptyModule = Resources.Load<GameObject>(path);
        CurrentModule = UnityEngine.Object.Instantiate(emptyModule).GetComponent<Module>();

        AssembleModule(CurrentModule.LowerPosition);
    }

    public void AssembleModule(Transform createPosition) // Lower - Upper 순차적 생성 및 Pivot 할당.
    {
        // Lower 생성
        CurrentLowerPart = CreatePart<LowerPart>(Parts_Location.Lower, createPosition, _gameManager.PartIndex_Lower);

        // Upper 생성
        CurrentUpperPart = CreatePart<UpperPart>(Parts_Location.Upper, CurrentLowerPart.UpperPositions, _gameManager.PartIndex_Upper);

        // Weapon 생성
        CurrentLeftArmPart = CreatePart<ArmsPart>(Parts_Location.Weapon_Arm_L, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftArm], _gameManager.PartIndex_LeftArm);
        CurrentRightArmPart = CreatePart<ArmsPart>(Parts_Location.Weapon_Arm_R, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightArm], _gameManager.PartIndex_RightArm);
        CurrentLeftShoulderPart = CreatePart<ShouldersPart>(Parts_Location.Weapon_Shoulder_L, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftShoulder], _gameManager.PartIndex_LeftShoulder);
        CurrentRightShoulderPart = CreatePart<ShouldersPart>(Parts_Location.Weapon_Shoulder_R, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightShoulder], _gameManager.PartIndex_RightShoulder);

        CurrentModule.Setup(CurrentLowerPart, CurrentUpperPart, CurrentLeftArmPart, CurrentRightArmPart, CurrentLeftShoulderPart, CurrentRightShoulderPart);
    }

    private T CreatePart<T>(Parts_Location type, Transform createPosition, int index = 0) where T : BasePart
    {
        BasePart[] parts = GetParts<T>();

        GameObject go = UnityEngine.Object.Instantiate(parts[index].gameObject, createPosition);
        T part = go.GetComponent<T>();
        part.Setup(type, CurrentModule);

        return part;
    }

    public void ChangePart(int index, Parts_Location partsType) // 디자인 패턴을 사용해 만들어보자... 할 수 있으면 ㅠ
    {
        switch (partsType)
        {
            case Parts_Location.Lower:
                if (_gameManager.PartIndex_Lower == index) return;

                CurrentUpperPart.transform.SetParent(CurrentModule.transform); // 상체 부모 오브젝트 변경
                UnityEngine.Object.DestroyImmediate(CurrentLowerPart.gameObject); // 즉시 파괴

                CurrentLowerPart = CreatePart<LowerPart>(partsType, CurrentModule.LowerPosition, index); // 하체 생성
                _gameManager.PartIndex_Lower = index;

                CurrentUpperPart.transform.SetParent(CurrentLowerPart.UpperPositions); // 상체 부모 오브젝트 변경
                CurrentUpperPart.transform.localPosition = Vector3.zero; // 상체 위치 조정
                break;
            case Parts_Location.Upper:
                if (_gameManager.PartIndex_Upper == index) return;

                WeaponPartsStore();
                UnityEngine.Object.DestroyImmediate(CurrentUpperPart.gameObject);

                CurrentUpperPart = CreatePart<UpperPart>(partsType, CurrentLowerPart.UpperPositions, index);
                _gameManager.PartIndex_Upper = index;

                WeaponPartsRestore();
                break;
            case Parts_Location.Weapon_Arm_L:
                if (_gameManager.PartIndex_LeftArm == index) return;

                UnityEngine.Object.DestroyImmediate(CurrentLeftArmPart.gameObject);

                CurrentLeftArmPart = CreatePart<ArmsPart>(partsType, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftArm], index);
                _gameManager.PartIndex_LeftArm = index;
                break;
            case Parts_Location.Weapon_Arm_R:
                if (_gameManager.PartIndex_RightArm == index) return;

                UnityEngine.Object.DestroyImmediate(CurrentRightArmPart.gameObject);

                CurrentRightArmPart = CreatePart<ArmsPart>(partsType, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightArm], index);
                _gameManager.PartIndex_RightArm = index;
                break;
            case Parts_Location.Weapon_Shoulder_L:
                if (_gameManager.PartIndex_LeftShoulder == index) return;

                UnityEngine.Object.DestroyImmediate(CurrentLeftShoulderPart.gameObject);

                CurrentLeftShoulderPart = CreatePart<ShouldersPart>(partsType, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftShoulder], index);
                _gameManager.PartIndex_LeftShoulder = index;
                break;
            case Parts_Location.Weapon_Shoulder_R:
                if (_gameManager.PartIndex_RightShoulder == index) return;

                UnityEngine.Object.DestroyImmediate(CurrentRightShoulderPart.gameObject);

                CurrentRightShoulderPart = CreatePart<ShouldersPart>(partsType, CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightShoulder], index);
                _gameManager.PartIndex_RightShoulder = index;
                break;
        }
        CurrentModule.Setup(CurrentLowerPart, CurrentUpperPart, CurrentLeftArmPart, CurrentRightArmPart, CurrentLeftShoulderPart, CurrentRightShoulderPart);
    }

    private void WeaponPartsStore()
    {
        CurrentLeftArmPart.transform.SetParent(CurrentModule.transform);
        CurrentRightArmPart.transform.SetParent(CurrentModule.transform);
        CurrentLeftShoulderPart.transform.SetParent(CurrentModule.transform);
        CurrentRightShoulderPart.transform.SetParent(CurrentModule.transform);
    }

    private void WeaponPartsRestore()
    {
        CurrentLeftArmPart.transform.SetParent(CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftArm]);
        CurrentRightArmPart.transform.SetParent(CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightArm]);
        CurrentLeftShoulderPart.transform.SetParent(CurrentUpperPart.WeaponPositions[(int)Weapon_Location.LeftShoulder]);
        CurrentRightShoulderPart.transform.SetParent(CurrentUpperPart.WeaponPositions[(int)Weapon_Location.RightShoulder]);

        CurrentLeftArmPart.transform.localPosition = Vector3.zero;
        CurrentRightArmPart.transform.localPosition = Vector3.zero;
        CurrentLeftShoulderPart.transform.localPosition = Vector3.zero;
        CurrentRightShoulderPart.transform.localPosition = Vector3.zero;
    }

    public T GetPartOfIndex<T>(int index) where T : BasePart
    {
        BasePart[] parts = GetParts<T>();

        if (index >= parts.Length || parts[index] == null)
        {
            Debug.Log($"해당 번호의 파츠는 없어용{index}");
            return null;
        }

        return parts[index] as T;
    }

    private BasePart[] GetParts<T>() where T : BasePart
    {
        if (!Modules.TryGetValue(typeof(T), out BasePart[] parts))
        {
            Debug.Log($"파츠 정보가 없습니다. {typeof(T)}");
            return null;
        }
        return parts;
    }

    public void Clear()
    {
        CurrentModule = null;
        OnUpperChange = null;
        OnLowerChange = null;
        OnLeftArmChange = null;
        OnRightArmChange = null;
        OnLeftShoulderChange = null;
        OnRightSoulderChange = null;
        OnInfoChange = null;
    }
}
