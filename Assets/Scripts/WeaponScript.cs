using UnityEngine;
public class WeaponScript : MonoBehaviour
{
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조
    public ActivablePlayer activablePlayer; // 무기 효과를 플레이어 애니메이션과 연동하기 위해 참조
    public Animator Animation; // 무기 자체 애니메이션 (검 휘두르는 모션)
    // 애니메이션 이벤트에서 실행할 함수 (검을 휘두르는 타격 프레임에 실행!)
    public void OnMeleeAttackHit() 
    {
        // 1. 근접 타격 판정 (SO에 저장된 크기와 위치 사용!)
        Vector2 hitCenter = (Vector2)activablePlayer.transform.position + activablePlayer.currentWeapon.meleeOffset[FindOffsetIndex()];
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitCenter, 
        activablePlayer.currentWeapon.meleeAttackRadius, 
        layerMask: LayerMask.GetMask("Enemy")); // "Enemy" 레이어에만 충돌 체크

        foreach(Collider2D enemy in hitEnemies)
        {
            float totalDamage = playerStatsManager.playerStats.AttackPower + (playerStatsManager.playerStats.AttackPower * activablePlayer.currentWeapon.baseDamage);
            // 근접 데미지 정보 셋업 후 몬스터에게 전달
            DamageInfo info = new()
            { 
                damage = totalDamage, 
                type = AttackType.Melee,
                hitPoint = enemy.ClosestPoint(hitCenter)
            };
            enemy.GetComponent<EnemyComme>().TakeDamage(info);
          
        }

        Debug.Log("근접 공격! 맞은 적 수: " + hitEnemies.Length);
    }

    int FindOffsetIndex()
    {
        int offsetIndex = 0;
        float offsetX = Animation.GetFloat("x");
        float offsetY = Animation.GetFloat("y");
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
        if (activablePlayer != null && activablePlayer.currentWeapon != null)
        {
            Gizmos.color = Color.red; // 기즈모 색상 설정
            Vector2 hitCenter = (Vector2)activablePlayer.transform.position + activablePlayer.currentWeapon.meleeOffset[FindOffsetIndex()];
            Gizmos.DrawWireSphere(new Vector3(hitCenter.x, hitCenter.y, 0), activablePlayer.currentWeapon.meleeAttackRadius);
        }
    }
    
}
