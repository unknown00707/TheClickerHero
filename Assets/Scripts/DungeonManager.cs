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
    public List<DungeonDataFromCSV> dungeonDataList = new(); // CSV에서 읽어온 던전 데이터 리스트
    public DungeonDataToJson dungeonDataToJson; // JSON으로 저장할 던전
    [Header("던전 UI 설정")]

    private readonly string dungeonDataFileName = "DungeonData.json"; // JSON 파일 이름
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadDungeonDataFromCSV();
    }


    public void LoadDungeon(int mainStageID)
    {
        // 던전 순회 
        int findIndex = -1;
        for (int i = 0; i < dungeonDataList.Count; i++)
        {  
            string[] floorParts = dungeonDataList[i].floor.Split('-'); // "0-0" -> ["0", "0"]
            string floorMainStageID = floorParts[0];
            string floorSubStageID = floorParts[1];
            if (floorMainStageID == mainStageID.ToString())
            {
                if (floorSubStageID == "0") // 메인 스테이지인 경우
                {
                    findIndex = i;
                    break;
                }
            }
        }

        // 찾은 인덱스가 유효한 경우에만 던전 로드 진행
        if (findIndex != -1)
        {
            playerTransform.position = Vector3.zero; 
            enemyManager.SpawnEnemy(dungeonDataList[findIndex].stageLevel, dungeonDataList[findIndex].enemyCount); // 적 스폰
            mainSenceUIManager.OpenMainUI(false); // 메인 UI 닫기
            Debug.Log($"던전 로드 성공! 찾은 인덱스: {findIndex}, 층 정보: {dungeonDataList[findIndex].floor}");
        }
        else
        {
            Debug.LogError($"던전 로드 실패! mainStageID {mainStageID}에 해당하는 던전을 찾을 수 없습니다.");
        }
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

            // 리스트에 순서대로 추가 (인덱스 관리가 필요 없음)
            dungeonDataList.Add(data);
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
            int floorCount = 0;
            foreach (var dungeon in dungeonDataList)
            {
               string part1 = dungeon.floor.Split('-')[0];
               if (int.TryParse(part1, out int mainStageID))
               {
                   if (mainStageID > floorCount)
                   {
                       floorCount = mainStageID;
                   }
               } 
            }
            dungeonDataToJson.isCleared = new List<bool>(new bool[floorCount + 1]); // 기본적으로 모든 던전 클리어 여부를 false로 초기화
            SaveDungeonDataToJson(); // 기본 데이터를 JSON으로 저장
            return;
        }
        GameManger.instance.LoadData(dungeonDataToJson, dungeonDataFileName);
    }
}
