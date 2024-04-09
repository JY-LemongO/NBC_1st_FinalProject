using System;
using UnityEngine;
using UnityEngine.Events;

public class ModuleStatus
{
    // To Do - SO 받아서 기본 능력치 Setup 하기

    // # Common Stats
    public float Armor { get; private set; } // HP
    public float Weight { get; private set; }

    // # Lower Stats
    public float MovementSpeed { get; private set; }
    public float JumpPower { get; private set; }
    public float BoostPower { get; private set; }
    public bool CanJump { get; private set; }

    // # Upper Stats 
    public float SmoothRotateValue { get; private set; }
    public float BoosterGauge { get; private set; }
    public float VTOL { get; private set; }

    public static event Action<float, float> OnChangeArmorPoint;
    public static event Action<float, float> OnChangeBoosterGauge;

    public float CurrentArmor { get; private set; }
    public float CurrentBooster { get; private set; }

    public bool IsDead { get; private set; } = false;

    private readonly float DASH_BOOSTER_CONSUME = 20f;
    private readonly float HOVER_BOOSTER_CONSUME = 1f;

    public ModuleStatus(LowerPart lower, UpperPart upper, WeaponPart leftArm, WeaponPart rightArm, WeaponPart leftShoulder, WeaponPart rightShoulder)
    {
        PartData lowerData = Managers.Data.GetPartData(lower.ID);
        PartData upperData = Managers.Data.GetPartData(upper.ID);
        PartData leftArmData = Managers.Data.GetPartData(leftArm.ID);
        PartData rightArmData = Managers.Data.GetPartData(rightArm.ID);
        PartData leftShoulderData = Managers.Data.GetPartData(leftShoulder.ID);
        PartData rightShoulderData = Managers.Data.GetPartData(rightShoulder.ID);

        Armor = lowerData.Armor + upperData.Armor;
        Weight = lowerData.Weight + upperData.Weight + leftArmData.Weight + rightArmData.Weight + leftShoulderData.Weight + rightShoulderData.Weight;

        MovementSpeed = lowerData.Speed;
        JumpPower = lowerData.JumpPower;
        CanJump = lowerData.CanJump;
        BoostPower = lowerData.BoosterPower;

        SmoothRotateValue = upperData.SmoothRotation;
        BoosterGauge = upperData.BoosterGauge;
        VTOL = upperData.Hovering;

        CurrentArmor = Armor;
        CurrentBooster = BoosterGauge;

        OnChangeArmorPoint?.Invoke(Armor, CurrentArmor);
        OnChangeBoosterGauge?.Invoke(BoosterGauge, CurrentBooster);
    }

    public void GetDamage(float damage)
    {
        CurrentArmor -= damage;
        if (CurrentArmor <= 0)
            Dead();
        OnChangeArmorPoint?.Invoke(Armor, CurrentArmor);
    }

    public void Repair()
    {
        CurrentArmor = Mathf.Min(CurrentArmor + 250, Armor);
        OnChangeArmorPoint?.Invoke(Armor, CurrentArmor);
    }

    public bool Boost()
    {
        if (CurrentBooster < DASH_BOOSTER_CONSUME)
            return false;

        CurrentBooster = Mathf.Max(0, CurrentBooster - DASH_BOOSTER_CONSUME);
        OnChangeBoosterGauge?.Invoke(BoosterGauge, CurrentBooster);
        return true;
    }

    public void Hovering(UnityAction action)
    {
        if (CurrentBooster <= 0)
            return;

        CurrentBooster = Mathf.Max(0, CurrentBooster - HOVER_BOOSTER_CONSUME);
        action.Invoke();
        OnChangeBoosterGauge?.Invoke(BoosterGauge, CurrentBooster);
    }

    public void BoosterRecharge()
    {
        CurrentBooster = Mathf.Min(BoosterGauge, CurrentBooster + 0.5f);
        OnChangeBoosterGauge?.Invoke(BoosterGauge, CurrentBooster);
    }

    private void Dead()
    {
        if (IsDead)
            return;

        Managers.ActionManager.CallPlayerDead();
    }
}
