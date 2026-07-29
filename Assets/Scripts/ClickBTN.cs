using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickBTN : MonoBehaviour
{
    public GoodsManager goodsManager;
    public DungeonManager dungeonManager;
    public MainSenceUIManager mainSenceUIManager;
    public GameObject[] tapObjects;
    public GameObject[] tapLeftObjects;
    public DungeonDataSo[] dungeonDataSos;

    
    [SerializeField] private UnityEngine.UI.Image dungeonInfoIMG;
    [SerializeField] private TextMeshProUGUI dungeonInfoTxt;
    private int currentDungeonID = 0;
    private LocalizedText localizedText;
    void Awake()
    {
        localizedText = dungeonInfoTxt.GetComponent<LocalizedText>();
        OpenTapByIndexDungeonSelect();
    }
    public void ClickBTNClick()
    {
        goodsManager.AddGoods();
    }

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
            dungeonManager.LoadDungeon(currentDungeonID);
        }
    }
}
