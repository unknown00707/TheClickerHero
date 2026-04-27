using UnityEngine;

public class EnemyComme : MonoBehaviour
{
    public float maxHp = 100f;
    public float currentHp;


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

    void OnEnable()
    {
        currentHp = maxHp;
    }
}
