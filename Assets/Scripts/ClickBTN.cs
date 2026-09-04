using System;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickBTN : MonoBehaviour
{
    [Header("Managers")]
    public PlayerHealth playerHealth;
    public GoodsManager goodsManager;
    public DungeonManager dungeonManager;
    public MainSenceUIManager mainSenceUIManager;
    public AutoManager autoManager;

    [Header("Tap Objects")]
    public GameObject[] tapObjects;
    public GameObject[] tapLeftObjects;

    [Header("Dungeon Select")]
    public DungeonDataSo[] dungeonDataSos;
    public GameObject dungeonSelectBTN;
    [SerializeField] private UnityEngine.UI.Image dungeonInfoIMG;
    [SerializeField] private TextMeshProUGUI dungeonInfoTxt;
    [SerializeField] private LocalizedText localizedText;
    private int currentDungeonID = 0;

    [Header("Auto Online Upgrade")]
    private int currentAutoOnlineIndex = 0;
    void Awake()
    {
        if (localizedText == null)
        {
            localizedText = dungeonInfoTxt.GetComponent<LocalizedText>();
        }
        OpenTapByIndexDungeonSelect();
    }
    public void ClickBTNClick()
    {
        goodsManager.AddGoods();
    }
// --------------------- 탭 UI 로직 ---------------------//
    public void OpenTapByIndex(int index)
    {
        LoopOpenTapByIndex(tapObjects, index);
    }

    public void OpenTapLeftByIndex(int index)
    {
        LoopOpenTapByIndex(tapLeftObjects, index);
    }
    void LoopOpenTapByIndex(GameObject[] tapArray, int index)
    {
        if (tapArray == tapLeftObjects && tapLeftObjects[index].activeInHierarchy == true) // tapLeftObjects 배열에서 선택된 인덱스가 이미 활성화되어 있는 경우
        {
            tapArray[index].SetActive(false); // 모든 탭을 비활성화 => 왼쪽 창만 적용
            return; // 더 이상 진행하지 않고 종료
        }

        for (int i = 0; i < tapArray.Length; i++)
        {

            if (i == index)
            {
                tapArray[i].SetActive(true);
            }
          
            else
            {
                tapArray[i].SetActive(false);
            }
        }
    }
// --------------------- 던전 선택 로직 ---------------------//
    public void MoveDungeonSelect(int moveIndex)
    {
        currentDungeonID += moveIndex;
        if (currentDungeonID < 0)
        {
            currentDungeonID = dungeonDataSos.Length - 1;
        }
        else if (currentDungeonID >= dungeonDataSos.Length)
        {
            currentDungeonID = 0;
        }
        // IsDungeonClear(currentDungeonID) => true == clear
        dungeonSelectBTN.SetActive(dungeonManager.IsCanPlayDungeon(currentDungeonID));
        OpenTapByIndexDungeonSelect();
    }

    void OpenTapByIndexDungeonSelect()
    {
        dungeonInfoIMG.sprite = dungeonDataSos[currentDungeonID].dungeonSprite;
        localizedText.textID = dungeonDataSos[currentDungeonID].dungeonName;
        localizedText.Refresh();
    }

    public void SelectDungeon()
    {
        // 선택된 던전의 메인 스테이지 ID를 가져와서 던전 매니저에 전달
        if (currentDungeonID == dungeonDataSos[currentDungeonID].dungeonMainStageID) // 동일한게 정상이지만 확인 용
        {
            mainSenceUIManager.OpenMainUI(false); // 메인 UI 닫기
            playerHealth.PlayerHealthInit(); // 플레이어 체력 초기화
            playerHealth.ChangePlayerActiveState(playerHealth.isDead); // 플레이어 오브젝트 활성화
            dungeonManager.LoadDungeon(currentDungeonID);
        }
    }
// --------------------- Auto 관련 로직 ---------------------//
    public void MoveAutoOnlineUpgrade(int moveDir)
    {
        if (Math.Abs(moveDir) != 1)
            moveDir = moveDir > 0 ? 1 : -1; // moveDir가 1 또는 -1이 아닌 경우, 양수면 1, 음수면 -1로 설정

        currentAutoOnlineIndex += moveDir;
        int maxIndex = autoManager.autoOnlineUpradeSo.Length;

        if (currentAutoOnlineIndex < 0)
            currentAutoOnlineIndex = maxIndex - 1;
        else if (currentAutoOnlineIndex >= maxIndex)
            currentAutoOnlineIndex = 0;
        
        autoManager.UpdateAutoOnlineUpgradeUI(currentAutoOnlineIndex);
    }

    public void UnLockAutoOnlineUpgrade()
    {
        autoManager.UnlockAutoOnlineUpgrade(currentAutoOnlineIndex);
    }
// --------------------- 기타 ---------------------//
    public void RetrunToMainSence()
    {
        playerHealth.ChangePlayerActiveState(true); // 플레이어 오브젝트 비활성화
        dungeonManager.DungeonUIInit(); // 던전 UI 초기화 - 혹시 모르는 보험
        mainSenceUIManager.OpenMainUI(true); // 메인 UI 열기
    }
}
