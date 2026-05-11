using System;
using System.Collections.Generic;
using UnityEngine;
public struct DamageInfo
{
    public float damage;
    public AttackType type;  // 근접인지 검기인지!
}
[Serializable]
public class UnlockWeaponData
{
    public List<bool> weaponUnlockStatus = new() { true, false, false, false, false, false, false, false, false, false };
    public int currentWeaponIndex = 0; // 현재 장착 중인 무기 인덱스 (예: 0은 첫 번째 무기)
}
public enum AttackType { Melee, Aura }
public class WeaponManager : MonoBehaviour
{
    public WeaponDataSo[] weaponDataArray; // 모든 무기 데이터 배열 (인덱스 0부터 순서대로)
    public UnlockWeaponData unlockWeaponData; // 무기 잠금 해제 상태 및 현재
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
