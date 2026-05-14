using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class SaveSkinData 
{
    public List<int> unlockedSkins; // 잠금 해제된 스킨 인덱스 목록
    public int equippedSkin; // 현재 장착된 스킨 인덱스
}
public class PlayerSkinManager : MonoBehaviour
{
    public GoodsManager goodsManager; // 재화 매니저 참조
    public ActivablePlayer activablePlayer; // 플레이어 참조
    public List<PlayerSkinDataSo> allSkins; // 게임에 존재하는 모든 스킨 데이터
    private readonly SaveSkinData saveData = new(); // 저장할 스킨 데이터
    private readonly String SAVE_FILE_NAME = "SaveSkinsData.json"; // 저장 파일 이름

    public void UnlockSkin(int skinIndex)
    {
        if (!saveData.unlockedSkins.Contains(skinIndex) && 
        allSkins[skinIndex].unlockCost < goodsManager.goodsData.GoodsCount)
        {
            goodsManager.goodsData.GoodsCount -= allSkins[skinIndex].unlockCost;
            goodsManager.GoodTXTUpdate();

            saveData.unlockedSkins.Add(skinIndex);
            GameManger.instance.SaveGame(); // 게임 저장
        }
        else
        {
            Debug.Log("재화가 부족합니다!");
            return;
        }
    }
    public void EquipSkin(int skinIndex)
    {
        if (saveData.unlockedSkins.Contains(skinIndex))
        {
            saveData.equippedSkin = skinIndex;
            UpPlayerSkin();
            GameManger.instance.SaveGame(); // 게임 저장
        }
    }

    public void SaveSkins() => GameManger.instance.SaveData(saveData, SAVE_FILE_NAME);
    public void LoadSkins()
    {
        GameManger.instance.LoadData(saveData, SAVE_FILE_NAME);
        UpPlayerSkin();
    }

    public void UpPlayerSkin()
    {
        activablePlayer.SetSkinAnimeOverride().runtimeAnimatorController 
        = allSkins[saveData.equippedSkin].skinOverrideController;
    }
}
