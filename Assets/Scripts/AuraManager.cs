using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AuraManager : MonoBehaviour
{
    public static AuraManager Instance { get; private set; }

    private readonly Dictionary<GameObject, IObjectPool<GameObject>> poolDictionary = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 🌟 [통합 공용 함수] 플레이어와 몬스터 모두가 사용하는 부채꼴 가변 발사 로직
    /// </summary>
    public void FireSpreadAura(
        GameObject prefab,      // 발사할 프리팹 원본
        Vector3 spawnPosition,  // 발사 시작 위치
        Vector2 baseDirection,  // 기준 발사 방향 (예: 플레이어가 바라보는 방향 벡터)
        int count,              // 발사 갯수
        float maxSpreadAngle,   // 부채꼴 최대 벌어질 각도
        float damage,           // 최종 계산된 데미지
        float speed,            // 검기 이동 속도
        float duration,         // 검기 지속 시간
        bool isPlayerAttack     // 플레이어의 공격 여부 (피격 판정용)
    )
    {
        if (prefab == null || count <= 0) return;

        // 1. 기준 방향 벡터를 Z축 기준 회전 각도(도 단위)로 변환
        float baseZAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // 2. 전역 딕셔너리에서 해당 프리팹의 풀 가져오기 (없으면 자동 생성)
        IObjectPool<GameObject> targetPool = GetOrCreatePool(prefab);

        // 3. 갯수(count)만큼 부채꼴 분할 루프 실행
        for (int i = 0; i < count; i++)
        {
            float finalZAngle = baseZAngle;

            // 총알이 2개 이상일 때만 양옆으로 각도를 쪼갬
            if (count > 1)
            {
                float progress = (float)i / (count - 1); 
                float startAngle = -maxSpreadAngle / 2f;
                float endAngle = maxSpreadAngle / 2f;
                
                float offsetAngle = Mathf.Lerp(startAngle, endAngle, progress);
                finalZAngle += offsetAngle;
            }

            // 최종 각도를 쿼터니언 회전값으로 변환
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, finalZAngle);

            // 4. 풀에서 꺼내서 배치
            GameObject auraGo = targetPool.Get();
            auraGo.transform.position = spawnPosition;
            auraGo.transform.rotation = spawnRotation;

            // 5. 데이터 주입 및 애니메이터 리셋 실행
            if (auraGo.TryGetComponent<AuraProjectile>(out var projectile))
            {
                projectile.SetupAura(targetPool, damage, speed, duration, isPlayerAttack);
            }
        }
    }

    // 프리팹 내부의 인스펙터 설정을 읽어 맞춤형 풀을 개설하는 내부 함수
    private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (poolDictionary.TryGetValue(prefab, out var existingPool))
        {
            return existingPool;
        }

        int capacity = 10;
        int max = 50;

        if (prefab.TryGetComponent<AuraProjectile>(out var auraScript))
        {
            capacity = auraScript.poolDefaultCapacity;
            max = auraScript.poolMaxSize;
        }

        IObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab), 
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => Destroy(go),
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: max
        );

        poolDictionary.Add(prefab, newPool);
        return newPool;
    }
}
