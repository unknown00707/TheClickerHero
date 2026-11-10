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
    private float animeSpeed = 1f;
    Coroutine myCoroutine;

    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SynchronizeBySo(EnemyDataSo data, float speed, float animeSpeed, float timeToDie)
    {
        bool isNull = data == null;
        Debug.Log($"{ColorPalette.Cyan}[EnemyAuto]{ColorPalette.End} SynchronizeBySo called with data: {data.name ?? "null"} = {isNull}, speed: {speed}, timeToDie: {timeToDie}");
        enemyData = data;
        spriteRenderer.sprite = enemyData.enemySprite;
        deadSprite = enemyData.deadSprite;
        animator.runtimeAnimatorController = enemyData.enemyAnimatorOverride; 
        transform.localPosition = new Vector3(0f, enemyData.transformOffset, 0f);
        
        this.animeSpeed = animeSpeed;

        moveSpeed = speed;
        this.timeToDie = timeToDie;
    }
    
    public void InitValues()
    {
        StopAutoEnemyCoroutine();

        enemyTransform.localPosition = Vector3.zero;
        moveSpeed = 0f;
        timeToDie = 0f;
        enemyData = null;
        deadSprite = null;
        animator.runtimeAnimatorController = null;
        SetNormalStats(false, null); // 애니메이터 비활성화, 스프라이트 초기화
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

        SetNormalStats(false, deadSprite); // Die

        autoManager.OnKillEnemyByAutoPlayer(
            enemyData.clickDrop, enemyData.coinDrop, dungeonManager.RollRareResourceDrop());

        float quarterTime = time / 4f;
        yield return new WaitForSecondsRealtime(quarterTime);

        autoManager.ReleaseActiveEnemy(); // 활성화된 적 큐에서 제거
        autoManager.ReleaseEnemy(this);
    }


    public void StopAutoEnemyCoroutine()
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
    }

    private void SetNormalStats(bool animtorEnabled, Sprite sprite)
    {
        animator.enabled = animtorEnabled; // 애니메이터 활성화
        if (animtorEnabled)
        {
            animator.SetFloat(XHash, -1); // 왼쪽
            animator.SetFloat(YHash, 0); // 기본
            animator.speed = animeSpeed; // 애니메이션 속도 설정
        }
        spriteRenderer.sprite = sprite; // 스프라이트 초기화
    }

    void OnEnable()
    {
        StopAutoEnemyCoroutine();
        SetNormalStats(true, null); // 애니메이터 활성화, 스프라이트 초기화
    }

    void OnDisable()
    {
        StopAutoEnemyCoroutine();
        SetNormalStats(false, null); // 애니메이터 비활성화, 스프라이트 초기화
    }
}
