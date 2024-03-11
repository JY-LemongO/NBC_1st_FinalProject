using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperPart : BasePart
{
    [SerializeField] Transform[] _primaryMuzzles;
    [SerializeField] Transform[] _secondaryMuzzles;

    public Weapon_Primary Primary { get; private set; }
    public Weapon_Secondary Secondary { get; private set; }

    private float _primaryFireRate = float.MaxValue;
    private float _secondaryCoolDown = float.MaxValue;

    public override void Setup()
    {
        base.Setup();
    }

    private void Start()
    {
        Primary = GetComponent<Weapon_Primary>();
        Secondary = GetComponent<Weapon_Secondary>();
    }

    private void Update()
    {
        if (_primaryFireRate < Primary.WeaponSO.fireRate)
            _primaryFireRate += Time.deltaTime;
        if (_secondaryCoolDown < Secondary.WeaponSO.coolDownTime)
            _secondaryCoolDown += Time.deltaTime;
    }

    public void UseWeapon_Primary()
    {
        if (_primaryFireRate < Primary.WeaponSO.fireRate)
            return;

        _primaryFireRate = 0;
        Primary.UseWeapon_Primary(_primaryMuzzles);
    }

    public void UseWeapon_Secondary()
    {
        if (_secondaryCoolDown < Secondary.WeaponSO.coolDownTime)
            return;

        _secondaryCoolDown = 0;
        StartCoroutine(Secondary.UseWeapon_Secondary(_secondaryMuzzles));
    }
}
