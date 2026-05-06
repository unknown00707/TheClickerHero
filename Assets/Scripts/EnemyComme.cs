using UnityEngine;
using UnityEngine.Video;

public class EnemyComme : MonoBehaviour
{
    public Transform enemyTransform;
    public Transform playerTransform;
    public EnemyDataSo enemyData = new();

    private float currentHp = 0f;

    void Awake()
    {
        enemyTransform = transform.parent; // 적 인스턴스의 부모 오브젝트를 참조 (적 프리팹의 구조에 따라 조정 필요)
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // 플레이어 태그로 플레이어 위치 참조 (적절한 태그 설정 필요)
    }
    void FixedUpdate()
    {
        if (enemyData == null) return; // 적 데이터가 설정되지 않았으면 이동 로직 실행하지 않음
        var distanceToPlayer = enemyTransform.transform.position - playerTransform.transform.position;
        if (distanceToPlayer.magnitude > enemyData.attackRange) // 적이 플레이어와 일정 거리 이상 떨어져 있을 때만 이동
        {
            // 적이 플레이어를 향하도록 회전
            enemyTransform.position += enemyData.moveSpeed * Time.fixedDeltaTime * distanceToPlayer.normalized; // 이동 속도 조절 (필요 시)
        }
    }

    public void SynchronizeBySo(EnemyDataSo data)
    {
        enemyData = data;
    }

    public void TakeDamage(DamageInfo info)
    {
        Debug.Log("적이 " + info.damage + " 데미지를 받았습니다! 공격 타입: " + info.type);
        // 1. 데미지 적용
        currentHp -= info.damage;

        // 2. 이펙트 분리!
        if (info.type == AttackType.Melee)
        {
            // 묵직한 베기 피격 이펙트 생성 (피 튀김 등)
            //Instantiate(meleeHitEffect, info.hitPoint, Quaternion.identity);
        }
        else if (info.type == AttackType.Aura)
        {
            // 마법적인 폭발 이펙트 생성 (검기 속성 이펙트)
            //Instantiate(auraHitEffect, info.hitPoint, Quaternion.identity);
        }
    }
    

    void OnEnable()
    {
        currentHp = enemyData.maxHp;
    }
}
