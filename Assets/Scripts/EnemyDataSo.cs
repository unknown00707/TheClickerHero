using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "GameData/Enemy")]
public class EnemyDataSo : ScriptableObject
{
    [Header("Enemy Information")]
    public string enemyName;          // 적 이름 (예: 슬라임)
    public Sprite enemySprite;       // 적 스프라이트
    public Sprite deadSprite;       // 죽을 때 보일 스프라이트
    public float maxHp;             // 최대 체력
    public float attackPower;       // 공격력
    public float moveSpeed;         // 이동 속도
    public int coinDrop;            // 처치 시 드롭하는 코인 양
    public int clickDrop;           // 처치 시 드롭하는 클릭 양
    [Header("Behavior Settings")]
    public float attackRange;       // 공격 범위
    public float attackCooldown;    // 공격 간격
    public Vector2 attackBoxSize;   // 공격 박스 범위
    public float attackOffsetPos; // 공격 시작 위치
    [Header("Animation Settings")]
    public AnimatorOverrideController enemyAnimatorOverride; // 적 애니메이션 오버라이드 (예: 슬라임이 점프하는 모션)
    public float transformOffset;
    public Vector2 hitboxSize;         // 적의 히트박스 크기 (예: 슬라임은 1x1 크기의 히트박스)
    public Vector2 hitboxOffset;       // 적의 히트박스 오프셋 (예: 슬라임은 중심에서 약간 아래로 히트박스 위치)
    public Vector2 groundSize;         // 적의 물리충돌박스 크기
    public Vector2 groundOffset;       // 적의 물리충돌박스 오프셋
    [Header("Aura Settings")]
    public GameObject auraPrefab;       // 적이 발사하는 오라 프리팹 
    public float auraDamageToAttackMultipule; // 오라 데미지 및 공격 데미지에 대한 
    public float auraDuration;       // 오라 지속 시간
    public float auraSpeed;          // 오라 이동 속도
    public int auraCount;         // 오라 개수 (동시 발사 수)
    public float auraSpreadAngle; // 오라 퍼지는 최대 각도
}
