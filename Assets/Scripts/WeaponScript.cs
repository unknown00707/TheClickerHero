using UnityEngine;
public class WeaponScript : MonoBehaviour
{
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조
    public WeaponManager weaponManager; // 무기 매니저 참조
    public Transform plaerTransform; // 플레이어 위치 참조 (근접 공격 판정에 사용)
    public ActivablePlayer activablePlayer; // 무기 효과를 플레이어 애니메이션과 연동하기 위해 참조
    public Animator Animation; // 무기 자체 애니메이션 (검 휘두르는 모션)
    public LayerMask enemyLayerMask; // 적 레이어 마스크 (적 레이어 설정 필요)
    private readonly Collider2D[] hitResults = new Collider2D[200]; // 최대 10명까지 적을 감지할 수 있는 배열 (필요 시 크기 조절) 
    ContactFilter2D filter = new();
    void Awake()
    {
        filter.useLayerMask = true;
        filter.layerMask = enemyLayerMask;
        filter.useTriggers = true; // 트리거는 제외할지 선택
    }
    // 애니메이션 이벤트에서 실행할 함수 (검을 휘두르는 타격 프레임에 실행!)
    public void OnMeleeAttackHit() 
    {
        var currentWeapon = weaponManager.GetCurrentWeaponData();
        // 1. 근접 타격 판정 (SO에 저장된 크기와 위치 사용!)
        Vector2 hitCenter = (Vector2)plaerTransform.transform.position + currentWeapon.meleeOffset[FindOffsetIndex()];
        int hitCount = Physics2D.OverlapCircle(hitCenter, currentWeapon.meleeAttackRadius, filter, hitResults);
        Collider2D[] hitEnemies = new Collider2D[hitCount];
        for (int i = 0; i < hitCount; i++) 
        {
            if (hitResults[i].TryGetComponent<EnemyComme>(out var monster)) 
            {
                float totalDamage = playerStatsManager.playerStats.AttackPower + (playerStatsManager.playerStats.AttackPower * currentWeapon.baseDamage);
                // 근접 데미지 정보 셋업 후 몬스터에게 전달
                DamageInfo info = new()
                { 
                    damage = totalDamage, 
                    type = AttackType.Melee,
                };
                monster.TakeDamage(info);
            }
        }

        
    }

    int FindOffsetIndex()
    {
        int offsetIndex = 0;
        float offsetX = Animation.GetFloat(XHash);
        float offsetY = Animation.GetFloat(YHash);
        if (offsetX == 0 && offsetY == 0)
        {
            offsetIndex = 0;
        }
        else if (offsetX != 0 && offsetY == 0)
        {
            if (offsetX > 0)
                offsetIndex = 2; // 오른쪽 공격이면 1
            else
                offsetIndex = 3; // 왼쪽 공격이면 2
        }
        else if (offsetX == 0 && offsetY != 0)
        {
            if (offsetY > 0)
                offsetIndex = 1; // 위쪽 공격이면 3
            else
                offsetIndex = 0; // 아래쪽 공격이면 4
        }
        return offsetIndex;
    }

    void OnDrawGizmos()
    {
        var currentWeapon = weaponManager.GetCurrentWeaponData();
        if (activablePlayer != null && currentWeapon != null)
        {
            Gizmos.color = Color.red; // 기즈모 색상 설정
            Vector2 hitCenter = (Vector2)activablePlayer.transform.position + currentWeapon.meleeOffset[FindOffsetIndex()];
            Gizmos.DrawWireSphere(hitCenter, currentWeapon.meleeAttackRadius);
        }
    }
    
    public void OnRangeAttack()
    {
        var weapon = weaponManager.GetCurrentWeaponData();
        if (weapon == null) return;

        // 플레이어의 최종 데미지 계산
        float totalDamage = (activablePlayer.playerStatsManager.playerStats.AttackPower + weapon.baseDamage) * weapon.auraDamageMultiplier;

        Vector2 lookDirection = activablePlayer.ReturnDirPlayerVec; 

        // 매니저 인스턴스를 통해 바로 호출!
        AuraManager.Instance.FireSpreadAura(
            weapon.auraPrefab,
            transform.position,
            lookDirection,
            weapon.maxAuraCount,
            weapon.auraSpreadAngle,
            totalDamage,
            weapon.auraSpeed,
            weapon.auraDuration,
            isPlayerAttack: true
        );
    }
}
