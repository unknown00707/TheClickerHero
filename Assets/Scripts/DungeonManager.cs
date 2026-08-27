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

    // 생성자에서 초기 던전 개수(7개)만큼 기본 데이터(false)를 채워줍니다.
    public DungeonDataToJson()
    {
        int initialDungeonCount = 7;
        for (int i = 0; i < initialDungeonCount; i++)
        {
            isCleared.Add(false); 
        }
    }
}
public class DungeonDataFromCSV
{
    public string floor; // 층 정보 (예: 0-0, 0-1 등)
    public int enemyCount; // 적 수
    public int stageLevel; // 스테이지 레벨
    public int clearCoin; // 던전 클리어 시 획득 코인
    public int clearClick; // 던전 클리어 시 획득 클릭 수
}
public class DungeonManager : MonoBehaviour
{
    [Header("Manager 참조")]
    public EnemyManager enemyManager;
    public PlayerStatsManager playerStatsManager;
    public ClickBTN clickBTN;
    [Header("던전 관련 설정")]
    public Transform playerTransform;
    private readonly Dictionary<int, Dictionary<int, DungeonDataFromCSV>> dungeonDataList = new(); // CSV에서 읽어온 던전 데이터 리스트
    private DungeonDataToJson dungeonDataToJson = new(); // JSON으로 저장할 던전
    private int currentDungeonMainFloor = 0; 
    private int currentDungeonSubFloor = 0;
    private readonly int START_SUB_FLOOR = 0;
    [Header("던전 UI 설정")]
    public GameObject clearUIObj;
    public TextMeshProUGUI clearRewardTxt;
    public TextMeshProUGUI clearClickRewardTxt;
    public TextMeshProUGUI clearCoinRewardTxt;
    public TextMeshProUGUI clearRelicRewardTxt;
    public TextMeshProUGUI gettenClickByEnemyTxt;
    public TextMeshProUGUI gettenCoinByEnemyTxt;
    private int gettenCoinByEnemy = 0; // 적 처치 및 던전 클리어 시 획득한 코인
    private int gettenClickByEnemy = 0; 
    private int gettenCoinByStage = 0;
    private int gettenClickByStage = 0;
    private int totalGettenClick = 0;
    private int totalGettenCoin = 0;
    private int defeatMonsters = 0;
    [Header("Language Key")]
    private readonly string DUNGEON_REWARD_SUCCESS = "DGN_DESC_REWARD_SUCCESS";
    private readonly string DUNGEON_REWARD_FAILED = "DGN_DESC_REWARD_FAILED";
    private readonly string DUNGEON_REWARD_CLICK = "DGN_DESC_REWARD_CLICK";
    private readonly string DUNGEON_REWARD_COIN = "DGN_DESC_REWARD_COIN";
    private readonly string DUNGEON_REWARD_RELIC = "DGN_DESC_REWARD_RELIC";
    private readonly string DGN_DESC_TOOLTIP = "DGN_DESC_TOOLTIP_";
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
        int startDungeonIndex = START_SUB_FLOOR; // 각 스테이지의 첫 번째 던전만 로드
        DungeonUIInit();
        LoadDungeonLogic(mainStageID, startDungeonIndex);
    }

    public void NextStageDungeonLoad()
    {
        int nextSubStageID = currentDungeonSubFloor + 1;
        if(dungeonDataList[currentDungeonMainFloor].ContainsKey(nextSubStageID))
        {
            // 계산 후 로드
            LoadDungeonLogic(currentDungeonMainFloor, nextSubStageID);
        }
        else
        {
            Debug.Log($"모든 던전을 클리어했습니다! 현재 메인 스테이지: {currentDungeonMainFloor}");
            AddDungeonClearAward();
            dungeonDataToJson.isCleared[currentDungeonMainFloor] = true;
            SetRewardUI(true, true);
        }
    }

    private void LoadDungeonLogic(int mainID, int subID)
    {
        // 던전 클리어 보상 - 처음 빼고
        if (subID != START_SUB_FLOOR)
            AddDungeonClearAward();
           
        DungeonDataFromCSV selectedDungeonData = dungeonDataList[mainID][subID];
        
        if (selectedDungeonData != null)
        {
            playerTransform.position = Vector3.zero; 
            enemyManager.SpawnEnemy(selectedDungeonData.stageLevel, selectedDungeonData.enemyCount); // 적 스폰

            currentDungeonMainFloor = mainID; // 현재 던전의 메인 층 정보 업데이트
            currentDungeonSubFloor = subID; // 현재 던전의 서브 층 정보 업데이트
        }
        else
        {
            Debug.LogError($"던전 로드 실패! mainStageID {mainID}, subStageID {subID}에 해당하는 던전을 찾을 수 없습니다.");
        }
    }
    public void DungeonUIInit()
    {
        clearUIObj.SetActive(false);
        gettenCoinByEnemy = 0; 
        gettenClickByEnemy = 0; 
        gettenCoinByStage = 0;
        gettenClickByStage = 0;
        ReFreshGettenTextByEnemy();
    }
    public void SetRewardUI(bool isOpen, bool isSuccessful = false)
    {
        CalculateTotalGettenValue();

        enemyManager.ClearAllActiveEnemies(); // 모든 적 제거

        string clearText = isSuccessful ? LanguageManager.Instance.GetText(DUNGEON_REWARD_SUCCESS) 
                                         : LanguageManager.Instance.GetText(DUNGEON_REWARD_FAILED);
        clearRewardTxt.text = clearText;     
        clearClickRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_CLICK).ReplaceTagsParams("GettenClick", totalGettenClick);
        clearCoinRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_COIN).ReplaceTagsParams("GettenCoin", totalGettenCoin);
        clearRelicRewardTxt.text = LanguageManager.Instance.GetText(DUNGEON_REWARD_RELIC).ReplaceTagsParams("GettenReic", $"{ColorPalette.Legend}Null{ColorPalette.End}");
        clearUIObj.SetActive(isOpen);
    }   
    public string[] ToTooltipByGetID(string id)
    {
        // 1. 타이틀과 기본 컨텐츠의 '키(Key)'를 먼저 조합합니다.
        string titleKey = DGN_DESC_TOOLTIP + id + "_TITLE";
        string contentKey = DGN_DESC_TOOLTIP + id;

        // 2. 키를 이용해 기본 번역 텍스트를 가져옵니다.
        string title = LanguageManager.Instance.GetText(titleKey);
        string baseContent = LanguageManager.Instance.GetText(contentKey);

        // 3. 가져온 기본 컨텐츠 텍스트를 id에 맞게 치환합니다.
        string finalContent = id switch
        {
            "CLICK" => GetClickContent(baseContent),
            "COIN"  => GetCoinContent(baseContent) + $"\n{ColorPalette.Yellow}{LanguageManager.Instance.GetText(contentKey+"_EXTRA")}{ColorPalette.End}",
            "RELIC" => GetRelicContent(baseContent),
            _       => null
        };

        // 4. 이미 번역 및 치환이 완료된 텍스트 배열을 그대로 리턴합니다.
        return new string[] { title, finalContent };
    }
    private string GetClickContent(string content)
    {
        return content.ReplaceTagsDict(new()
        {
            { "StageClick",    gettenClickByStage },
            { "GettenClick",    gettenClickByEnemy },
            { "DefeatMonsters", defeatMonsters},
            { "ClickMultiply",  playerStatsManager.GetStatValueInDict("클릭 배수") },
            { "TotalBenefit",   playerStatsManager.GetStatValueInDict("총 혜택") }
        });
    }
    private string GetCoinContent(string content)
    {
        return content.ReplaceTagsDict(new()
        {
            { "StageCoin",     gettenCoinByStage },
            { "GettenCoin",     gettenCoinByEnemy },
            { "DropRate",       playerStatsManager.GetStatValueInDict("희귀 확률") },
            { "TotalBenefit",   playerStatsManager.GetStatValueInDict("총 혜택") }
        });
    }
    private string GetRelicContent(string content)
    {
        return content;
    }
    public void AddCoinAndClickByEnemy(int amount, bool isCoin)
    {
        if(isCoin)
            gettenCoinByEnemy += amount;
        else
            gettenClickByEnemy += amount;
        
        ReFreshGettenTextByEnemy();
    }
    private void ReFreshGettenTextByEnemy()
    {
        gettenCoinByEnemyTxt.text = gettenCoinByEnemy.ToString();
        gettenClickByEnemyTxt.text = gettenClickByEnemy.ToString();
    }
    private void AddDungeonClearAward()
    {
        DungeonDataFromCSV clearDungeonData = dungeonDataList[currentDungeonMainFloor][currentDungeonSubFloor];
        
        gettenClickByStage += clearDungeonData.clearClick;
        gettenCoinByStage += clearDungeonData.clearCoin;
        
    }
    private void CalculateTotalGettenValue()
    {
        totalGettenClick = (int)((gettenClickByStage + gettenClickByEnemy) * 
            (1.0f + (playerStatsManager.playerStats.ClickMultiplier + playerStatsManager.playerStats.ToTalbenefit)));
    
        totalGettenCoin = gettenCoinByEnemy + gettenCoinByStage;
    }
    public void AddEnemyDieCount()
    {
        defeatMonsters++;
    }
    public bool RollRareResourceDrop()
    {
        float finalChance = 
                playerStatsManager.playerStats.RareGoodsProbability 
                * playerStatsManager.playerStats.ToTalbenefit;

        float randomRoll = UnityEngine.Random.value;

        if (randomRoll <= finalChance)
            return true;
        else
            return false;
    }
    public bool IsCanPlayDungeon(int id)
    {
        if (id == 0) // 가장 처음 던전
            return true;
        if (dungeonDataToJson.isCleared.Count < id) 
        // 반환값이 id 전 던전이 클리어 여부이기 때문에 반드시 Count >= id 여야함. 
        {
            Debug.Log($"{ColorPalette.Red}던전 클리어 리스트가 비어있음!{ColorPalette.End}");
            return false;
        }

        return dungeonDataToJson.isCleared[--id];
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
            if (part.Length < 5) continue;

            DungeonDataFromCSV data = new()
            {
                floor = part[0],
                enemyCount = int.Parse(part[1]),
                stageLevel = int.Parse(part[2]),
                clearCoin = int.Parse(part[3]),
                clearClick = int.Parse(part[4])
            };
            int mainStageID = int.Parse(part[0].Split('-')[0]);
            int subStageID = int.Parse(part[0].Split('-')[1]);

            // 리스트에 순서대로 추가 (인덱스 관리가 필요 없음)
            if (!dungeonDataList.ContainsKey(mainStageID))
            {
                dungeonDataList[mainStageID] = new Dictionary<int, DungeonDataFromCSV>();
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
            Debug.Log($"{ColorPalette.Rare}던전 클리어 데이터 초기화{ColorPalette.End}");
            dungeonDataToJson = new DungeonDataToJson();  // 기본적으로 모든 던전 클리어 여부를 false로 초기화
            SaveDungeonDataToJson();
        }
        
        GameManger.instance.LoadData(dungeonDataToJson, dungeonDataFileName);

        // 업데이트로 던전이 늘어났을 때를 위한 세이브 파일 보정 코드
        // 세이브 파일에 기록된 개수가 현재 게임 던전 수보다 적다면? -- 업데이트에 따른 차이 발생 보정
        if(dungeonDataToJson.isCleared.Count < clickBTN.dungeonDataSos.Length)
        {
            while (dungeonDataToJson.isCleared.Count >= clickBTN.dungeonDataSos.Length)
            {
                dungeonDataToJson.isCleared.Add(false); // 부족한 칸만큼 false를 뒤에 붙여줌
            }
            Debug.Log($"새로운 던전 데이터 추가됨! 현재 총 개수: {dungeonDataToJson.isCleared.Count}");
            SaveDungeonDataToJson();
            LoadDungeonDataFromJson();
        }
    }
}
