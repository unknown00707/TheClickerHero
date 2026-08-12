using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public struct DamageInfo
{
    public float damage;
    public AttackType type;  // 근접인지 검기인지!
}
[Serializable]
public class UnlockWeaponData
{
    public List<int> weaponUnlockStatus = new() {0}; // 잠금 해제된 무기 인덱스 목록 (예: 0은 첫 번째 무기)
    public int equippedWeaponIndex = 0; // 현재 장착 중인 무기 인덱스 (예: 0은 첫 번째 무기)
}
public enum AttackType { Melee, Aura }
public class WeaponManager : MonoBehaviour
{
    [Header("Managers")]
    public GoodsManager goodsManager; // 재화 매니저 참조
    public ActivablePlayer activablePlayer; // 플레이어 참조
    [Header("Weapon Data")]
    public WeaponDataSo[] weaponDataArray; // 모든 무기 데이터 배열 (인덱스 0부터 순서대로)
    public UnlockWeaponData unlockWeaponData = new(); // 무기 잠금 해제 상태 및 현재
    private int currentWeaponIndex = 0;
    [Header("Weapon UI")]
    public TextMeshProUGUI weaponNameText; // 무기 이름 텍스트
    public TextMeshProUGUI weaponDescriptionText; // 무기 설명 텍스트
    public TextMeshProUGUI weaponCostText; // 무기 비용 텍스트
    public Image weaponImage; // 무기 이미지 UI
    public GameObject unlockButton; // 무기 잠금 해제 버튼
    public GameObject equipButton; // 무기 장착 버튼
    private readonly string SAVE_FILE_NAME = "SaveWeaponData.json"; // 저장 파일 이름
    private void Start()
    {
        LoadWeaponData(); // 게임 시작 시 무기 데이터 로드
        UpdateWeaponUI(); // 현재 장착된 무기 UI 업데이트
    }
    public void MoveToNextWeapon(int direction)
    {
        if (Mathf.Abs(direction) != 1)
        {
            if (direction > 1)
                direction = 1;
            else if (direction < -1)
                direction = -1;
        }

        currentWeaponIndex += direction;
        if (currentWeaponIndex < 0)
            currentWeaponIndex = weaponDataArray.Length - 1;
        else if (currentWeaponIndex >= weaponDataArray.Length)
            currentWeaponIndex = 0;

        UpdateWeaponUI();
    }
    public void UnlockWeapon()
    {
        if (weaponDataArray[currentWeaponIndex].unlockCost > goodsManager.goodsData.GoodsCount)
        {
            Debug.Log("재화 부족으로 무기 잠금 해제 실패");
            return;
        }

        if (!unlockWeaponData.weaponUnlockStatus.Contains(currentWeaponIndex))
        {
            goodsManager.goodsData.GoodsCount -= weaponDataArray[currentWeaponIndex].unlockCost;
            unlockWeaponData.weaponUnlockStatus.Add(currentWeaponIndex);
            UpdateWeaponUI(); // UI 업데이트
            SaveWeaponData(); // 무기 잠금 해제 후 저장
        }
    }
    public void EquipWeapon()
    {
        if (unlockWeaponData.weaponUnlockStatus.Contains(currentWeaponIndex))
        {
            unlockWeaponData.equippedWeaponIndex = currentWeaponIndex;
            activablePlayer.SetSameAnimeOverride(GetCurrentWeaponData()); // 무기 장착 시 애니메이션 오버라이드 설정
            UpdateWeaponUI(); // UI 업데이트
            SaveWeaponData(); // 무기 장착 후 저장
        }
    }
    private void UpdateWeaponUI()
    {
        bool isUnlocked = unlockWeaponData.weaponUnlockStatus.Contains(currentWeaponIndex);
        WeaponDataSo currentWeaponData = weaponDataArray[currentWeaponIndex];

        weaponNameText.text = currentWeaponData.weaponName;
        weaponDescriptionText.text = currentWeaponData.description;
        weaponCostText.text = currentWeaponData.unlockCost.ToString();
        weaponImage.sprite = currentWeaponData.weaponSprite;

        unlockButton.SetActive(!isUnlocked);
        equipButton.SetActive(isUnlocked);
    }

    public WeaponDataSo GetCurrentWeaponData()
    {
        return weaponDataArray[unlockWeaponData.equippedWeaponIndex];
    }
//-----------------------Save/Load--------------------------------------------------//
    public void SaveWeaponData() => GameManger.instance.SaveData(unlockWeaponData, SAVE_FILE_NAME);
    public void LoadWeaponData()
    {
        GameManger.instance.LoadData(unlockWeaponData, SAVE_FILE_NAME);
    }
}
