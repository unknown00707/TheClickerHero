using UnityEngine;
using UnityEngine.Pool;

public class AuraProjectile : MonoBehaviour
{
    [Header("오브젝트 풀 설정 (인스펙터 조절용)")]
    public int poolDefaultCapacity = 10; // 처음에 미리 만들어둘 개수
    public int poolMaxSize = 50;         // 풀에 최대로 쌓아둘 개수

    private float speed;
    private float damage;
    private float duration;
    private float currentLifetime;
    private bool isPlayerAttack;

    private IObjectPool<GameObject> myPool;
    private Animator animator;

    private void Awake()
    {
        // 최적화를 위해 애니메이터 컴포넌트 사전 캐싱
        animator = GetComponent<Animator>();
    }

    public void SetupAura(IObjectPool<GameObject> pool, float damage, float speed, float duration, bool isPlayerAttack)
    {
        this.myPool = pool;
        this.damage = damage;
        this.speed = speed;
        this.duration = duration;
        this.isPlayerAttack = isPlayerAttack;
        this.currentLifetime = 0f;

        // 🌟 [기능 구현] 애니메이터 리셋 (처음 프레임부터 다시 재생)
        if (animator != null)
        {
            // "Base Layer"의 0번째 시간(처음)으로 강제 되돌림
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
        }
    }

    void Update()
    {
        // 유니티 2D에서는 대개 transform.right(오른쪽 화살표 방향) 방향을 정면(0도)으로 잡고 회전시킵니다.
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        currentLifetime += Time.deltaTime;
        if (currentLifetime >= duration)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerAttack && other.CompareTag("EnemyHit"))
        {
            if (other.TryGetComponent<EnemyComme>(out var enemyComme))
            {
                DamageInfo dmgInfo = new() { damage = this.damage, type = AttackType.Aura };
                enemyComme.TakeDamage(dmgInfo);
                ReturnToPool();
            }
        }
        else if (!isPlayerAttack && other.CompareTag("Player"))
        {
            // 플레이어 피격 로직 처리...
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (myPool != null) myPool.Release(this.gameObject);
        else Destroy(gameObject);
    }
}
