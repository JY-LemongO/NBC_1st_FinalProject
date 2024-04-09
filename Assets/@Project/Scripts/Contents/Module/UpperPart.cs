using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperPart : BasePart
{
    public enum WeaponType
    {
        LeftArm,
        RightArm,
        LeftShoulder,
        RightShoulder,
    }    
    [field: SerializeField] public Transform[] WeaponPositions { get; private set; }    

    public override void Setup(Define.PartsType type, Module module)
    {
        base.Setup(type, module);
    }
}
