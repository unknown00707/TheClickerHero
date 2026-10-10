using UnityEngine;

public class EnemyAuto : MonoBehaviour
{
    [Header("Manager")]
    public AutoManager autoManager;
    [Header("Components")]  
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public BoxCollider2D hitboxCollider;
    public Transform enemyTransform;
    [Header("Enemy Data")]
    private float moveSpeed = 0f;
    private float timeToDie = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SynchronizeBySo(EnemyDataSo data, float speed, float timeToDie)
    {
        EnemyDataSo enemyData = data;
        spriteRenderer.sprite = enemyData.enemySprite;
        animator.runtimeAnimatorController = enemyData.enemyAnimatorOverride; 
        hitboxCollider.size = enemyData.hitboxSize; 
        hitboxCollider.offset = enemyData.hitboxOffset; 
        transform.localPosition = new Vector3(0f, enemyData.transformOffset, 0f);

        moveSpeed = speed;
        this.timeToDie = timeToDie;
    }
    
    void Update()
    {
        enemyTransform.Translate(moveSpeed * Time.deltaTime * Vector3.right);
    }

    public void Die()
    {
        autoManager.ReleaseActiveEnemy(); // 활성화된 적 큐에서 제거
    }

    void OnEnable()
    {
        
    }
}
