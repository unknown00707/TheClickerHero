using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "GameData/Enemy")]
public class EnemyDataSo : ScriptableObject
{
    [Header("Enemy Information")]
    public string enemyName;          // 적 이름 (예: 슬라임)
    public Sprite enemySprite;       // 적 스프라이트
    public float maxHp;             // 최대 체력
    public float attackPower;       // 공격력
    public float moveSpeed;         // 이동 속도
    public int coinDrop;            // 처치 시 드롭하는 코인 양
    [Header("Behavior Settings")]
    public float attackRange;       // 공격 범위
    public float attackCooldown;    // 공격 간격
    [Header("Animation Settings")]
    public AnimatorOverrideController enemyAnimatorOverride; // 적 애니메이션 오버라이드 (예: 슬라임이 점프하는 모션)
    public Vector2 hitboxSize;         // 적의 히트박스 크기 (예: 슬라임은 1x1 크기의 히트박스)
    public Vector2 hitboxOffset;       // 적의 히트박스 오프셋 (예: 슬라임은 중심에서 약간 아래로 히트박스 위치)
}
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
            GameObject enemyInstance = Instantiate(emptyEnemyPrefab);
            enemyInstance.SetActive(false); // 초기에는 비활성화 상태로 풀링
            // 적 인스턴스에 EnemyComme 컴포넌트를 추가하고 초기 설정 (예: 체력, 애니메이션 등)
            EnemyComme enemyComme = enemyInstance.AddComponent<EnemyComme>();
            // 초기 설정은 나중에 활성화 시점에 해당 적 데이터로 적용할 예정
            emptyEnemyInstancePool.Push(enemyComme);
        }
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
                    List<int> enemyProbabilities = CalculateProbabilityByPossibleEnemyCount(possibleEnemies.Count);
                    EnemyDataSo selectedEnemyData = possibleEnemies[Random.Range(0, enemyProbabilities.Count)];
                    // 적 인스턴스 활성화 및 위치 설정 (예: 플레이어 주변 랜덤 위치)
                    enemyComme.gameObject.SetActive(true);
                    Vector2 spawnPosition = (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * 5f; // 플레이어 주변 반경 5 내에서 랜덤 위치
                    enemyComme.transform.position = spawnPosition;
                    // 적 데이터 적용 (체력, 애니메이션 등)
                    // 예시: enemyComme.ApplyEnemyData(selectedEnemyData);
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

    List<int> CalculateProbabilityByPossibleEnemyCount(int totoalPossibleEnemyCount)
    {
        List<int> probaablilityLsit = new();
        int _initialValue = 1;
        // 각 인덱스에 따른 몬스터 등장 확률 계산. 0 -> 1 으로 갈 수록 확률이 반토막 1 2 4 8 16 32 . . .
        for (int i = 1; i < totoalPossibleEnemyCount + 1; i++)
        {
            for(int j = 0; j < i + 1; j++)
            {
                probaablilityLsit.Add(_initialValue); 
            }
            _initialValue += 1;
        }

        return probaablilityLsit;
    }
}
