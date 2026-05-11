using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyComme : MonoBehaviour
{
    public EnemyManager enemyManager;
    public Transform enemyTransform;
    public Transform playerTransform;
    private Animator animator;
    private BoxCollider2D hitboxCollider;
    private EnemyDataSo enemyData;

    private float currentHp = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        hitboxCollider = GetComponent<BoxCollider2D>();
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

        currentHp = enemyData.maxHp;
        animator.runtimeAnimatorController = enemyData.enemyAnimatorOverride; // 애니메이터 컨트롤러 동기화
        hitboxCollider.size = enemyData.hitboxSize; // 히트박스 크기 동기화
        hitboxCollider.offset = enemyData.hitboxOffset; // 히트박스 오프셋 동기화
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

    public void Die()
    {
        Debug.Log("적이 사망했습니다!");
        // 사망 처리 (예: 사망 애니메이션 재생, 아이템 드랍 등)
        // 사망 애니메이션이 끝난 후 적 인스턴스를 풀에 반환
        enemyTransform.SetParent(enemyManager.transform); // 적 인스턴스를 매니저의 자식으로 다시 설정하여 풀링 시스템과 호환되도록 함
        enemyManager.ReturnEnemyToPool(this);
    }
    

    void OnEnable()
    {
        enemyTransform = transform.parent; // 적 인스턴스의 부모 오브젝트를 참조 (적 프리팹의 구조에 따라 조정 필요)
    }
}
