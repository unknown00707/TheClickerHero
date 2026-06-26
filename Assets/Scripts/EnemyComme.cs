using System.Collections;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyComme : MonoBehaviour
{
    public EnemyManager enemyManager;
    public Transform enemyTransform;
    public Rigidbody2D enemyRigidbody;
    public Transform playerTransform;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private BoxCollider2D hitboxCollider;
    private EnemyDataSo enemyData;

    [SerializeField]private float currentHp = 0f;
    private bool isDead = false;
    private Coroutine trackCoroutine; 
    private Vector2 vectorToPlayer ;
    private Vector2 dirToPlayer;
    [SerializeField] private LayerMask playerLayer;
    [Header("Animation")]
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private float attackCoolTime = 0f;
    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);

    void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
    }
    void Update()
    {
        if (currentHp <= 0f && !isDead) 
        {
            Die();
        }
    }
    void FixedUpdate()
    {
        if (enemyData == null || isDead) return; // 적 데이터가 설정되지 않았으면 이동 로직 실행하지 않음
        if (vectorToPlayer .magnitude > enemyData.attackRange) // 적이 플레이어와 일정 거리 이상 떨어져 있을 때만 이동
            enemyRigidbody.linearVelocity = enemyData.moveSpeed * vectorToPlayer .normalized; // 이동 속도 조절 (필요 시)
        else
        {
            AttackSign();
        }
    }

    void AttackSign()
    {
        if (Time.time < attackCoolTime) return;
        
        enemyRigidbody.linearVelocity = Vector2.zero; // 공격 범위 내에 들어오면 이동을 멈춤
        enemyRigidbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        animator.SetTrigger(AttackHash); // 공격 계시
    }

    public void OnAttackHitTrigger()
    {
        // 몬스터 중심에서 공격 방향으로 BoxCast를 발사하여 판정
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,         // 발사 시작 위치
            enemyData.hitboxSize,                    // 박스의 크기
            0f,                         // 회전 각도
            dirToPlayer,     // 발사 방향 (동서남북 중 하나)
            enemyData.attackRange,               // 사정거리
            playerLayer                 // 레이어 마스크
        );

        if (hit.collider != null)
        {
            Debug.Log($"플레이어 히트! 대미지를 입힙니다.");
            // hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(10);
        }
    } 
    public void OnAttackAnimationEnd()
    {
        // 다시 움직일 수 있도록 물리 고정 해제
        enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void SynchronizeBySo(EnemyDataSo data)
    {
        enemyData = data;
        spriteRenderer.sprite = data.enemySprite;
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
        isDead = true;
        spriteRenderer.sprite = enemyData.deadSprite;
        // 몇 초있다가 초기 상태로 리셋하는 로직

        enemyManager.ReturnEnemyToPool(this); // 인스턴스를 풀에 반환 
    }
    
    IEnumerator UpdateTargetDirectionRoutine()
    {
        // 에러 방지를 위해 Start() 등이 끝날 때까지 한 프레임 쉬어주는 게 안전합니다
        yield return null; 

        while (true)
        {
            // 플레이어가 씬에 존재하고 타겟이 지정되어 있을 때만 연산
            if (playerTransform != null)
            {
                vectorToPlayer  = playerTransform.position - enemyTransform.position;
                dirToPlayer = Vector2.zero;
                if (Mathf.Abs(vectorToPlayer.x) > Mathf.Abs(vectorToPlayer.y))
                {
                    // x축 거리가 더 멀다면 -> 좌(왼쪽) 우(오른쪽) 중 하나
                    dirToPlayer.x = dirToPlayer.x > 0 ? 1f : -1f;
                }
                else
                {
                    // y축 거리가 더 멀다면 -> 상(위쪽) 하(아래쪽) 중 하나
                    dirToPlayer.y = dirToPlayer.y > 0 ? 1f : -1f;
                }
                animator.SetFloat(XHash, dirToPlayer.x);
                animator.SetFloat(YHash, dirToPlayer.y);
            }
            
            yield return _waitForSeconds0_1;
        }
    }

    void OnEnable()
    {
        if (trackCoroutine != null) StopCoroutine(trackCoroutine);
            trackCoroutine = StartCoroutine(UpdateTargetDirectionRoutine());
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 코루틴을 확실히 종료
        if (trackCoroutine != null)
        {
            StopCoroutine(trackCoroutine);
            trackCoroutine = null;
        }
    }
}
