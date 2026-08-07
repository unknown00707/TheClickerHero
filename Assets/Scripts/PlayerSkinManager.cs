using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class SaveSkinData 
{
    public List<int> unlockedSkins = new() {0}; // 잠금 해제된 스킨 인덱스 목록
    public int equippedSkin = 0; // 현재 장착된 스킨 인덱스
}
public class PlayerSkinManager : MonoBehaviour
{
    public GoodsManager goodsManager; // 재화 매니저 참조
    public ActivablePlayer activablePlayer; // 플레이어 참조
    public List<PlayerSkinDataSo> allSkins; // 게임에 존재하는 모든 스킨 데이터
    [Header("스킨 UI")]
    public TextMeshProUGUI skinNameText; // 스킨 이름 텍스트
    public TextMeshProUGUI skinDescriptionText; // 스킨 설명 텍스트
    public TextMeshProUGUI skinCostText; // 스킨 비용 텍스트
    public TextMeshProUGUI skinUnequipButtonText; // 스킨 해제 버튼 텍스트
    public TextMeshProUGUI skinEquipButtonText; // 스킨 장착 버튼 텍스트
    public Image skinImage; // 스킨 이미지 UI
    [Header("GameObjects")]
    public GameObject unlockButton; // 스킨 잠금 해제 버튼
    public GameObject equipButton; // 스킨 장착 버튼
    private readonly SaveSkinData saveData = new(); // 저장할 스킨 데이터
    private readonly string SAVE_FILE_NAME = "SaveSkinsData.json"; // 저장 파일 이름
    private readonly string TXT_SKIN_EQUIP = "TXT_SKIN_EQUIP"; // 스킨 장착 텍스트 ID
    private readonly string TXT_SKIN_UNEQUIP = "TXT_SKIN_UNEQUIP"; // 스킨 해제 텍스트 ID
    private int currentSkinIndex = 0; // 현재 선택된 스킨 인덱스

    private void Start()
    {
        LoadSkins(); // 게임 시작 시 스킨 데이터 로드
        SetupSkinUI(saveData.equippedSkin); // 현재 장착된 스킨 UI 설정

        LanguageManager.Instance.OnLanguageChanged += () => SetupSkinUI(saveData.equippedSkin); // 언어 변경 시 UI 업데이트
    }
    private void SetupSkinUI(int skinIndex)
    {
        bool isUnlocked = saveData.unlockedSkins.Contains(skinIndex);

        unlockButton.SetActive(!isUnlocked); // 잠금 해제 버튼 활성화 여부
        equipButton.SetActive(isUnlocked); // 장착 버튼 활성화 여부

        // UI 업데이트
        skinNameText.text = LanguageManager.Instance.GetText(allSkins[skinIndex].skinName);
        skinDescriptionText.text = LanguageManager.Instance.GetText(allSkins[skinIndex].description);
        skinCostText.text = "$ : " + allSkins[skinIndex].unlockCost.ToString();
        skinImage.sprite = isUnlocked ? allSkins[skinIndex].skinSprite : allSkins[skinIndex].unlockSprite;

        skinUnequipButtonText.text = LanguageManager.Instance.GetText(TXT_SKIN_UNEQUIP);
        skinEquipButtonText.text = LanguageManager.Instance.GetText(TXT_SKIN_EQUIP);
    }
    public void UnlockSkin()
    {
        if (!saveData.unlockedSkins.Contains(currentSkinIndex) && 
        allSkins[currentSkinIndex].unlockCost < goodsManager.goodsData.GoodsCount)
        {
            goodsManager.goodsData.GoodsCount -= allSkins[currentSkinIndex].unlockCost;
            goodsManager.GoodTXTUpdate();

            saveData.unlockedSkins.Add(currentSkinIndex);
            SetupSkinUI(currentSkinIndex); // UI 업데이트
            GameManger.instance.SaveGame(); // 게임 저장
        }
        else
        {
            Debug.Log("재화가 부족합니다!");
            return;
        }
    }
    public void EquipSkin()
    {
        if (saveData.unlockedSkins.Contains(currentSkinIndex))
        {
            saveData.equippedSkin = currentSkinIndex;
            UpPlayerSkin();
            GameManger.instance.SaveGame(); // 게임 저장
        }
    }
    public void NextSkin(int direction)
    {
        if(Mathf.Abs(direction) != 1) 
            direction = direction > 0 ? 1 : -1; // 방향이 1 또는 -1이 아니면, 방향을 1 또는 -1로 설정
        
        currentSkinIndex += direction;
        if (currentSkinIndex >= allSkins.Count) currentSkinIndex = 0; // 마지막 스킨이면 첫 번째 스킨으로
        else if (currentSkinIndex < 0) currentSkinIndex = allSkins.Count - 1; // 첫 번째 스킨이면 마지막 스킨으로

        SetupSkinUI(currentSkinIndex); // UI 업데이트
    }
    private void UpPlayerSkin()
    {
        activablePlayer.SetSkinAnimeOverride().runtimeAnimatorController 
        = allSkins[saveData.equippedSkin].skinOverrideController;
    }

    public void SaveSkins() => GameManger.instance.SaveData(saveData, SAVE_FILE_NAME);
    public void LoadSkins()
    {
        GameManger.instance.LoadData(saveData, SAVE_FILE_NAME);
        UpPlayerSkin();
    }


    private void OnDestroy()
    {
        LanguageManager.Instance.OnLanguageChanged -= () => SetupSkinUI(saveData.equippedSkin); // 언어 변경 이벤트 해제
    }
}
