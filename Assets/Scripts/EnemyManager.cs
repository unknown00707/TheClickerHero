using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public DungeonManager dungeonManager;
    public List<EnemyDataSo> enemyDataList; // 다양한 적 데이터를 리스트로 관리
    public GameObject emptyEnemyPrefab; // 적 프리팹 (공통된 기본 형태)
    public Transform playerTransform; // 플레이어 위치 참조 (적 스폰 시 플레이어를 기준으로 위치 설정)
    public int MAX_ENEMY_INSTANCES = 100; // 최대 적 인스턴스 수 (풀링 시스템에서 활용)
    [Header("적 던전 UI관련")]
    public TextMeshProUGUI leftEnemyText; // 남은 적 수 표시용 텍스트
    private readonly Dictionary<int, List<EnemyDataSo>> enemyPoolDict = new(); // 스테이지 기반으로 적 데이터를 빠르게 조회할 수 있는 딕셔너리
    private readonly Stack<EnemyComme> emptyEnemyInstancePool = new(); // 적 인스턴스 풀링 리스트
    private readonly List<EnemyComme> activeEnemies = new(); // 현재 활성화된 적 인스턴스 리스트
    void Awake()
    {
        for (int i = 0; i < MAX_ENEMY_INSTANCES; i++)
        {
            GameObject enemyInstance = Instantiate(emptyEnemyPrefab, transform);
            // 적 인스턴스에 EnemyComme 컴포넌트를 참조하고 초기 설정 (예: 체력, 애니메이션 등)
            EnemyComme enemyComme = enemyInstance.GetComponentInChildren<EnemyComme>();
            enemyComme.playerTransform = playerTransform; // 플레이어 위치 참조 설정
            enemyComme.enemyManager = this; // 적 매니저 참조 설정
            // 초기 설정은 나중에 활성화 시점에 해당 적 데이터로 적용할 예정
            enemyComme.enemyTransform.SetParent(transform); // 적 인스턴스를 EnemyManager의 자식으로 설정
            enemyComme.enemyTransform.gameObject.SetActive(false); // 초기에는 비활성화 상태로 시작
            ReturnEnemyToPool(enemyComme); // 초기에는 모든 인스턴스를 풀에 반환하여 비활성화 상태로 시작
        }
        EnemyPoolDictInit(); // 스테이지 레벨에 따른 적 데이터 딕셔너리 초기화
    }
    void EnemyPoolDictInit()
    {
        LoadCSVDataToInitPoolDict(); // CSV 파일에서 스테이지 레벨에 따른 적 데이터 딕셔너리 초기화
    }
    public void SpawnEnemy(int stageLevel, int enemySpawnCount)
    {
        // 스테이지 레벨에 맞는 적 데이터를 기반으로 적 인스턴스 활성화 및 위치 설정
        for (int i = 0; i < enemySpawnCount; i++)
        {
            if (emptyEnemyInstancePool.Count > 0)
            {
                EnemyComme enemyComme = emptyEnemyInstancePool.Pop();
                // 스테이지 레벨에 맞는 적 데이터 리스트에서 랜덤으로 하나 선택
                List<EnemyDataSo> possibleEnemies = enemyPoolDict.ContainsKey(stageLevel) ? enemyPoolDict[stageLevel] : new List<EnemyDataSo>();
                if (possibleEnemies.Count > 0)
                {
                    int selectedEnemyIndex = GetRandomEnemyIndex(possibleEnemies.Count);
                    EnemyDataSo selectedEnemyData = possibleEnemies[selectedEnemyIndex];
                    // 적 인스턴스 활성화 및 위치 설정 (예: 플레이어 주변 랜덤 위치)
                    Transform grandEnemyTransform = enemyComme.enemyTransform;
                    Vector2 spawnPosition = (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * 5f; // 플레이어 주변 반경 5 내에서 랜덤 위치
                    grandEnemyTransform.position = spawnPosition;
                    // 적 데이터 적용 (체력, 애니메이션 등)
                    enemyComme.SynchronizeBySo(selectedEnemyData); // 적 인스턴스에 선택된 적 데이터 적용

                    grandEnemyTransform.gameObject.SetActive(true);
                    activeEnemies.Add(enemyComme); // 활성화된 적 리스트에 추가
                }
                else
                {
                    Debug.LogWarning("해당 스테이지 레벨에 적 데이터가 없습니다: " + stageLevel);
                    emptyEnemyInstancePool.Push(enemyComme); // 사용하지 않은 인스턴스는 다시 풀에 반환
                }
            }
            else
            {
                Debug.LogWarning("적 인스턴스 풀이 부족합니다!");
                break; // 더 이상 인스턴스를 사용할 수 없으므로 루프 종료
            }
        }
        UpdateLeftEnemyCountUI(); // 남은 적 수 UI 업데이트
    }

    // 등장 가능한 몬스터 수(N)를 받아, 가중치 확률에 따라 인덱스를 하나 뽑아주는 함수
    int GetRandomEnemyIndex(int possibleEnemyCount)
    {
        // 1. 각 인덱스별 가중치를 담을 배열과, 전체 가중치 합
        int[] weights = new int[possibleEnemyCount];
        int totalWeight = 0;

        // 2. 가중치 계산 (인덱스가 작을수록 높은 가중치, 겹치지 않게 2의 제곱수 활용)
        // 예: N이 4라면 -> 가중치는 8, 4, 2, 1 이 됨!
        for (int i = 0; i < possibleEnemyCount; i++)
        {
            // 1 << (possibleEnemyCount - 1 - i) 는 2의 제곱을 빠르게 계산하는 비트 연산이야.
            weights[i] = 1 << (possibleEnemyCount - 1 - i); 
            totalWeight += weights[i];
        }

        // 3. 0부터 전체 가중치 합(totalWeight) 사이에서 랜덤 값 하나 뽑기
        int randomValue = Random.Range(0, totalWeight);

        // 4. 룰렛 휠 돌리기 (랜덤 값이 어느 구간에 속하는지 체크)
        int currentWeight = 0;
        for (int i = 0; i < possibleEnemyCount; i++)
        {
            currentWeight += weights[i];
            
            // 랜덤값이 현재 누적 가중치 구간 안에 들어왔다면 당첨!
            if (randomValue < currentWeight)
            {
                return i;
            }
        }

        // 수학적으로 여기까지 올 일은 없지만, 안전장치로 마지막 인덱스 반환
        return possibleEnemyCount - 1; 
    }
    public void ReturnEnemyToPool(EnemyComme enemyComme)
    {
        // 적 인스턴스를 비활성화하고 풀에 반환
        emptyEnemyInstancePool.Push(enemyComme);
        activeEnemies.Remove(enemyComme);
        UpdateLeftEnemyCountUI();

        if (activeEnemies.Count == 0)
        {
            // 모든 적이 제거되었을 때 던전 클리어 처리
            dungeonManager.NextStageDungeonLoad();
        }
    }
    private void UpdateLeftEnemyCountUI()
    {
        leftEnemyText.text = activeEnemies.Count.ToString();
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ Save & Load ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ //

    public void LoadCSVDataToInitPoolDict()
    {
        TextAsset csvData = Resources.Load<TextAsset>("EnemyIDByStageLevelData");
        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        // 1. \r\n과 \n 모두 완벽하게 분리하고 빈 줄은 무시
        string[] lines = csvData.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        bool isFirstLine = true;

        foreach (string line in lines)
        {
            // 2. 첫 줄(헤더) 건너뛰기
            if (isFirstLine) 
            { 
                isFirstLine = false; 
                continue; 
            }

            // 3. 쌍따옴표(") 제거 후 콤마(,)로 분리
            // 예: 1,"0,1" -> 1,0,1 로 바꾼 뒤 쪼갬 -> ["1", "0", "1"]
            string cleanLine = line.Replace("\"", "");
            string[] parts = cleanLine.Split(',');

            if (parts.Length < 2) continue;

            // 4. parts[0]은 스테이지 레벨
            if (!int.TryParse(parts[0], out int stageLevel))
            {
                Debug.LogWarning($"스테이지 파싱 에러: {parts[0]}");
                continue;
            }

            List<EnemyDataSo> enemyDataForStage = new();

            // 5. parts[1] 부터 끝까지는 전부 몬스터 ID
            for (int i = 1; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int id))
                {
                    if (id >= 0 && id < enemyDataList.Count) // 안전 장치
                    {
                        enemyDataForStage.Add(enemyDataList[id]);
                    }
                    else
                    {
                        Debug.LogWarning($"EnemyID {id}가 리스트 범위를 벗어났습니다!");
                    }
                }
            }

            // 6. 딕셔너리에 추가
            if (enemyDataForStage.Count > 0)
            {
                enemyPoolDict[stageLevel] = enemyDataForStage;
            }
        }
        Debug.Log("스테이지 적 데이터 CSV 로드 완료!");
    }
}
