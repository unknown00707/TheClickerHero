using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;

[Serializable]
public class DungeonDataToJson
{
    public List<bool> isCleared = new(); // 던전 클리어 여부
}
public class DungeonDataFromCSV
{
    public string floor; // 층 정보 (예: 0-0, 0-1 등)
    public int enemyCount; // 적 수
    public int stageLevel; // 스테이지 레벨
}
public class DungeonManager : MonoBehaviour
{
    [Header("던전 관련 설정")]
    public Transform playerTransform;
    public MainSenceUIManager mainSenceUIManager;
    public EnemyManager enemyManager;
    private readonly Dictionary<string, Dictionary<string, DungeonDataFromCSV>> dungeonDataList = new(); // CSV에서 읽어온 던전 데이터 리스트
    private readonly DungeonDataToJson dungeonDataToJson; // JSON으로 저장할 던전
    private string currentDungeonMainFloor = "0"; 
    private string currentDungeonSubFloor = "0";
    [Header("던전 UI 설정")]
    public GameObject dungeonClearUIObj;
    public TextMeshProUGUI dungeonClearRewardTxt;
    public TextMeshProUGUI dungeonClearClickRewardTxt;
    public TextMeshProUGUI dungeonClearCoinRewardTxt;
    public TextMeshProUGUI dungeonClearRelicRewardTxt;
    public TextMeshProUGUI dungeonGettenRelicRewardTxt;
    public TextMeshProUGUI dungeonGettenCoinByEnemyTxt;
    private int gettenCoinByEnemy = 0; // 적 처치 시 획득한 코인
    private readonly string DUNGEON_REWARD_SUCCESS = "DGN_DESC_REWARD_SUCCESS";
    private readonly string DUNGEON_REWARD_FAILED = "DGN_DESC_REWARD_FAILED";
    private readonly string DUNGEON_REWARD_CLICK = "DGN_DESC_REWARD_CLICK";
    private readonly string DUNGEON_REWARD_COIN = "DGN_DESC_REWARD_COIN";
    private readonly string DUNGEON_REWARD_RELIC = "DGN_DESC_REWARD_RELIC";
    private readonly string dungeonDataFileName = "DungeonData.json"; // JSON 파일 이름
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadDungeonDataFromCSV();
        LoadDungeonDataFromJson();
        SetRewardUI(false); // 처음에는 던전 클리어 UI를 비활성화 상태로 시작
    }

    public void LoadDungeon(int mainStageID)
    {
        // 던전 순회 
        string startDungeonIndex = "0"; // 각 스테이지의 첫 번째 던전만 로드
        LoadDungeonLogic(mainStageID.ToString(), startDungeonIndex);
    }

    public void NextStageDungeonLoad()
    {
        string nextSubStageID = (int.Parse(currentDungeonSubFloor) + 1).ToString();
        if(dungeonDataList[currentDungeonMainFloor].ContainsKey(nextSubStageID))
        {
            LoadDungeonLogic(currentDungeonMainFloor, nextSubStageID);
        }
        else
        {
            Debug.Log($"모든 던전을 클리어했습니다! 현재 메인 스테이지: {currentDungeonMainFloor}");
            SetRewardUI(true, true);
        }
    }

    private void LoadDungeonLogic(string mainID, string subID)
    {
        DungeonDataFromCSV selectedDungeonData = dungeonDataList[mainID][subID];
        if (selectedDungeonData != null)
        {
            playerTransform.position = Vector3.zero; 
            enemyManager.SpawnEnemy(selectedDungeonData.stageLevel, selectedDungeonData.enemyCount); // 적 스폰

            currentDungeonMainFloor = mainID; // 현재 던전의 메인 층 정보 업데이트
            currentDungeonSubFloor = subID; // 현재 던전의 서브 층 정보 업데이트
            AddCoinByEnemy(0); // 적 처치 시 획득한 코인 초기화
        }
        else
        {
            Debug.LogError($"던전 로드 실패! mainStageID {mainID}, subStageID {subID}에 해당하는 던전을 찾을 수 없습니다.");
        }
    }

    private void SetRewardUI(bool isOpen, bool isSuccessful = false)
    {
        dungeonClearUIObj.SetActive(isOpen);
        string rewardText = isSuccessful ? LanguageManager.Instance.GetText(DUNGEON_REWARD_SUCCESS) 
                                         : LanguageManager.Instance.GetText(DUNGEON_REWARD_FAILED);
        dungeonClearRewardTxt.text = rewardText;     
        dungeonClearClickRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_CLICK);
        dungeonClearCoinRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_COIN);
        dungeonClearRelicRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_RELIC);
    }   
    public void AddCoinByEnemy(int coinAmount)
    {
        gettenCoinByEnemy += coinAmount;
        dungeonGettenCoinByEnemyTxt.text = gettenCoinByEnemy.ToString();
    }
// ----------- CSV 데이터 로드 및 JSON 저장/로드 메서드 -----------
    void LoadDungeonDataFromCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>("DungeonStageData");
        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        string[] lines = csvData.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // 리스트를 깔끔하게 비우고 새로 시작
        dungeonDataList.Clear(); 

        // i = 1부터 시작하여 첫 줄 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string[] part = lines[i].Split(',');
            if (part.Length < 3) continue;

            // 새로운 데이터 객체 생성 (DungeonData 클래스/구조체 이름에 맞게 수정하세요)
            DungeonDataFromCSV data = new()
            {
                floor = part[0],
                enemyCount = int.Parse(part[1]),
                stageLevel = int.Parse(part[2])
            };
            string mainStageID = part[0].Split('-')[0];
            string subStageID = part[0].Split('-')[1];

            // 리스트에 순서대로 추가 (인덱스 관리가 필요 없음)
            if (!dungeonDataList.ContainsKey(mainStageID))
            {
                dungeonDataList[mainStageID] = new Dictionary<string, DungeonDataFromCSV>();
            }
            dungeonDataList[mainStageID][subStageID] = data;
        }

        foreach (var mainStage in dungeonDataList)
        {
            foreach (var subStage in mainStage.Value)
            {
                Debug.Log($"Loaded Dungeon Data - Main Stage: {mainStage.Key}, Sub Stage: {subStage.Key}, Enemy Count: {subStage.Value.enemyCount}, Stage Level: {subStage.Value.stageLevel}");
            }
        }
    }

    public void SaveDungeonDataToJson()
    {
        GameManger.instance.SaveData(dungeonDataToJson, dungeonDataFileName);
    }

    public void LoadDungeonDataFromJson()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, dungeonDataFileName)))
        {
            Debug.Log($"{dungeonDataFileName} 파일이 존재하지 않습니다. 기본 데이터를 사용합니다.");
            int floorCount = dungeonDataList.Count;
            dungeonDataToJson.isCleared = new List<bool>(new bool[floorCount]); // 기본적으로 모든 던전 클리어 여부를 false로 초기화
            SaveDungeonDataToJson(); // 기본 데이터를 JSON으로 저장
            return;
        }
        GameManger.instance.LoadData(dungeonDataToJson, dungeonDataFileName);
    }
}
