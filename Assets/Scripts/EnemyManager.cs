using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyDataSo> enemyDataList; // 다양한 적 데이터를 리스트로 관리
    public GameObject emptyEnemyPrefab; // 적 프리팹 (공통된 기본 형태)
    public Transform playerTransform; // 플레이어 위치 참조 (적 스폰 시 플레이어를 기준으로 위치 설정)
    public int MAX_ENEMY_INSTANCES = 100; // 최대 적 인스턴스 수 (풀링 시스템에서 활용)
    private readonly Dictionary<int, List<EnemyDataSo>> enemyPoolDict = new(); // 스테이지 기반으로 적 데이터를 빠르게 조회할 수 있는 딕셔너리
    private readonly Stack<EnemyComme> emptyEnemyInstancePool = new(); // 적 인스턴스 풀링 리스트
    void Awake()
    {
        for (int i = 0; i < MAX_ENEMY_INSTANCES; i++)
        {
            GameObject enemyInstance = Instantiate(emptyEnemyPrefab, transform);
            // 적 인스턴스에 EnemyComme 컴포넌트를 추가하고 초기 설정 (예: 체력, 애니메이션 등)
            EnemyComme enemyComme = enemyInstance.AddComponent<EnemyComme>();
            enemyComme.playerTransform = playerTransform; // 플레이어 위치 참조 설정
            enemyComme.enemyManager = this; // 적 매니저 참조 설정
            // 초기 설정은 나중에 활성화 시점에 해당 적 데이터로 적용할 예정
            ReturnEnemyToPool(enemyComme); // 초기에는 모든 인스턴스를 풀에 반환하여 비활성화 상태로 시작
        }
        EnemyPoolDictInit(); // 스테이지 레벨에 따른 적 데이터 딕셔너리 초기화
    }
    void EnemyPoolDictInit()
    {
        // 예시
        enemyPoolDict[1] = new List<EnemyDataSo> { enemyDataList[0], enemyDataList[1], enemyDataList[2] }; // 슬라임, 고블린, 오크
        enemyPoolDict[2] = new List<EnemyDataSo> { enemyDataList[2], enemyDataList[3] }; // 오크, 트롤
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
                    grandEnemyTransform.gameObject.SetActive(true);
                    Vector2 spawnPosition = (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * 5f; // 플레이어 주변 반경 5 내에서 랜덤 위치
                    grandEnemyTransform.SetParent(null); // 적 인스턴스를 매니저에서 분리하여 독립적으로 움직일 수 있도록 설정
                    grandEnemyTransform.position = spawnPosition;
                    // 적 데이터 적용 (체력, 애니메이션 등)
                    enemyComme.SynchronizeBySo(selectedEnemyData); // 적 인스턴스에 선택된 적 데이터 적용
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
        enemyComme.enemyTransform.gameObject.SetActive(false);
        emptyEnemyInstancePool.Push(enemyComme);
    }
}
