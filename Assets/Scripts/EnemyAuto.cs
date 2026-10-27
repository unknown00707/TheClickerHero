using System.Collections;
using UnityEngine;

public class EnemyAuto : MonoBehaviour
{
    [Header("Manager")]
    public AutoManager autoManager;
    public DungeonManager dungeonManager;
    [Header("Components")]  
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform enemyTransform;
    [Header("Enemy Data")]
    private EnemyDataSo enemyData;
    private Sprite deadSprite;
    private float moveSpeed = 0f;
    private float timeToDie = 0f;
    Coroutine myCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SynchronizeBySo(EnemyDataSo data, float speed, float timeToDie)
    {
        enemyData = data;
        spriteRenderer.sprite = enemyData.enemySprite;
        deadSprite = enemyData.deadSprite;
        animator.runtimeAnimatorController = enemyData.enemyAnimatorOverride; 
        transform.localPosition = new Vector3(0f, enemyData.transformOffset, 0f);

        moveSpeed = speed;
        this.timeToDie = timeToDie;
    }
    
    void Update()
    {
        enemyTransform.Translate(moveSpeed * Time.deltaTime * Vector3.right);
    }
    public void StartAutoEnemyCoroutine()
    {
        myCoroutine = StartCoroutine(DieCoroutine(timeToDie));
    }
    IEnumerator DieCoroutine(float time)
    {
        yield return new WaitForSecondsRealtime(time);

        Die();
        autoManager.OnKillEnemyByAutoPlayer(
            enemyData.clickDrop, enemyData.coinDrop, dungeonManager.RollRareResourceDrop());

        float quarterTime = time / 4f;
        yield return new WaitForSecondsRealtime(quarterTime);

        autoManager.ReleaseEnemy(this);
    }

    private void Die()
    {
        autoManager.ReleaseActiveEnemy(); // 활성화된 적 큐에서 제거
        animator.enabled = false; 
        spriteRenderer.sprite = deadSprite;
    }

    public void StopAutoEnemyCoroutine()
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
    }

    void OnEnable()
    {
        myCoroutine = null;
        animator.enabled = true; // 애니메이터 활성화
        spriteRenderer.sprite = null; // 스프라이트 초기화
    }
}
