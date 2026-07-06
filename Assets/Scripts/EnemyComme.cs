using System.Collections;
using UnityEngine;

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
    public BoxCollider2D groundCol2D;
    public Transform auraSpwanTransform;
    private BoxCollider2D hitboxCollider;
    [SerializeField] private EnemyDataSo enemyData;

    [SerializeField] private float currentHp = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private Coroutine trackCoroutine; 
    [SerializeField] private Vector2 vectorToPlayer = new();
    [SerializeField] private Vector2 dirToPlayer = new();
    [SerializeField] private LayerMask playerLayer;
    [Header("Animation")]
    [SerializeField] private float deadTime = 3f;
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private float attackCoolTime = 0; 
    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");

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
        if (enemyData == null || isDead || isAttacking) return;
        
        // 쿨타임 중이거나 사정거리 밖에 있으면 플레이어를 추적하여 이동합니다.
        if (vectorToPlayer.magnitude > enemyData.attackRange || Time.time < attackCoolTime)
        {
            enemyRigidbody.linearVelocity = enemyData.moveSpeed * vectorToPlayer.normalized; 
        }
        else
        {
            AttackSign();
        }
    }

    void AttackSign()
    {
        // 쿨타임 중이거나 이미 공격 중이면 연타 방지
        if (Time.time < attackCoolTime || isAttacking) return;

        isAttacking = true;
        FreezePos();
        animator.SetTrigger(AttackHash); 
    }

    public void OnAttackHitTrigger()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer가 참조되지 않았습니다!");
            return;
        }

        Vector2 centerPos = spriteRenderer.bounds.center;
        Vector2 finalHitboxSize; 
        Vector2 finalAttackPos = centerPos;

        Vector2 direction = dirToPlayer == Vector2.zero ? Vector2.down : dirToPlayer.normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            direction = new Vector2(Mathf.Sign(direction.x), 0f); 
        else
            direction = new Vector2(0f, Mathf.Sign(direction.y)); 

        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            finalHitboxSize = new Vector2(enemyData.attackBoxSize.y, enemyData.attackBoxSize.x);
            finalAttackPos.y += direction.y * enemyData.attackOffsetPos;
        }
        else
        {
            finalHitboxSize = new Vector2(enemyData.attackBoxSize.x, enemyData.attackBoxSize.y);
            finalAttackPos.x += direction.x * enemyData.attackOffsetPos;
        }

        RaycastHit2D hit = Physics2D.BoxCast(
            finalAttackPos,         
            finalHitboxSize,        
            0f,                         
            direction,     
            0.01f,                  
            playerLayer                 
        );

        if (hit.collider != null)
        {
            Debug.Log($"플레이어 히트! 대미지를 입힙니다.");
        }
    }
    public void OnRangedAttack()
    {
        float totalDamage = enemyData.attackPower * enemyData.auraDamageToAttackMultipule;
        AuraManager.Instance.FireSpreadAura(
        enemyData.auraPrefab,
        auraSpwanTransform.position,
        dirToPlayer,
        enemyData.auraCount,
        enemyData.auraSpreadAngle,
        totalDamage,
        enemyData.auraSpeed,
        enemyData.auraDuration,
        false // isPlayerAttack?
       );
    }
    void FreezePos()
    {
        enemyRigidbody.linearVelocity = Vector2.zero; 
        enemyRigidbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    }

    public void OnAttackAnimationEnd()
    {
        enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        attackCoolTime = Time.time + enemyData.attackCooldown;
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (enemyData == null || spriteRenderer == null) return;

        Vector2 direction = dirToPlayer == Vector2.zero ? Vector2.down : dirToPlayer.normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            direction = new Vector2(Mathf.Sign(direction.x), 0f); 
        else
            direction = new Vector2(0f, Mathf.Sign(direction.y)); 

        Vector2 centerPos = spriteRenderer.bounds.center;
        Vector2 finalHitboxSize;
        Vector2 finalAttackPos = centerPos;

        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            finalHitboxSize = new Vector2(enemyData.attackBoxSize.y, enemyData.attackBoxSize.x);
            finalAttackPos.y += direction.y * enemyData.attackOffsetPos;
        }
        else
        {
            finalHitboxSize = new Vector2(enemyData.attackBoxSize.x, enemyData.attackBoxSize.y);
            finalAttackPos.x += direction.x * enemyData.attackOffsetPos;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerPos, centerPos + (direction * enemyData.attackRange));

        Vector2 castCenterPos = finalAttackPos + (direction * 0.005f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
        Gizmos.DrawCube(castCenterPos, finalHitboxSize);

        Gizmos.color = Color.red; 
        Gizmos.DrawWireCube(castCenterPos, finalHitboxSize);
    }

    public void SynchronizeBySo(EnemyDataSo data)
    {
        enemyData = data;
        spriteRenderer.sprite = enemyData.enemySprite;
        currentHp = enemyData.maxHp;
        animator.runtimeAnimatorController = enemyData.enemyAnimatorOverride; 
        hitboxCollider.size = enemyData.hitboxSize; 
        hitboxCollider.offset = enemyData.hitboxOffset; 
        groundCol2D.size = enemyData.groundSize; 
        groundCol2D.offset = enemyData.groundOffset; 
        transform.localPosition = new Vector3(0f, enemyData.transformOffset, 0f);
    }

    public void TakeDamage(DamageInfo info)
    {
        Debug.Log("적이 " + info.damage + " 데미지를 받았습니다! 공격 타입: " + info.type);
        currentHp -= info.damage;
    }

    public void Die()
    {
        FreezePos();
        isDead = true;
        isAttacking = false; // 사망 시 플래그 초기화
        groundCol2D.enabled = false;
        animator.enabled = false; 
        spriteRenderer.sortingOrder = -10;
        spriteRenderer.sprite = enemyData.deadSprite;
        enemyManager.ReturnEnemyToPool(this); 
        Invoke(nameof(DisableSelf), deadTime);
    }
    
    void DisableSelf()
    {
        enemyTransform.gameObject.SetActive(false);
    }

    IEnumerator UpdateTargetDirectionRoutine()
    {
        // 안전하게 애니메이터 초기화 대기
        yield return null; 
        yield return null; 

        while (true)
        {
            if (playerTransform != null && spriteRenderer != null && !isAttacking)
            {
                Vector2 enemyCenter = spriteRenderer.bounds.center;
                Vector2 playerCenter = (Vector2)playerTransform.position + new Vector2(0f, 0.5f);

                vectorToPlayer = playerCenter - enemyCenter;
                
                float finalX = 0f;
                float finalY = 0f;

                if (Mathf.Abs(vectorToPlayer.x) > Mathf.Abs(vectorToPlayer.y))
                {
                    // 가로축 정형화 (Float 값 1.0f 또는 -1.0f 주입)
                    finalX = vectorToPlayer.x > 0 ? 1f : -1f;
                    finalY = 0f;
                }
                else
                {
                    // 세로축 정형화 (Float 값 1.0f 또는 -1.0f 주입)
                    finalX = 0f;
                    finalY = vectorToPlayer.y > 0 ? 1f : -1f;
                }
                
                // [핵심 변경] 다시 Float형 파라미터 전송 방식으로 복구!
                animator.SetFloat(XHash, finalX);
                animator.SetFloat(YHash, finalY);

                // 물리 박스 방향 연동용 데이터 동기화
                dirToPlayer = new Vector2(finalX, finalY);
            }
            
            yield return _waitForSeconds0_1;
        }
    }



    void OnEnable()
    {
        isDead = false;
        isAttacking = false; // 풀에서 재활용될 때 플래그 리셋 필수
        groundCol2D.enabled = true;
        animator.enabled = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 0;
        }
        enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (trackCoroutine != null) StopCoroutine(trackCoroutine);
        trackCoroutine = StartCoroutine(UpdateTargetDirectionRoutine()); 
    }

    void OnDisable()
    {
        if (trackCoroutine != null)
        {
            StopCoroutine(trackCoroutine);
            trackCoroutine = null;
        }
    }
}
