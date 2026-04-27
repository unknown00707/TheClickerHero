using System;
using UnityEngine;

public class AuraProjectile : MonoBehaviour
{
    public ActivablePlayer activablePlayer;
    public float speed = 1;
    public float damage = 0;
    public float duration = 1;
    public int maxCount = 1;
    public float spreadAngle = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupAura(float damage, float speed, float duration, int maxCount, float spreadAngle)
    {
        this.damage = damage;
        this.speed = speed;
        this.duration = duration;
        this.maxCount = maxCount;
        this.spreadAngle = spreadAngle;
    }

    // 애니메이션 이벤트에서 실행할 함수 (검기가 뿜어져 나가는 프레임에 실행!)
    public void OnFireAura()
    {
        if (activablePlayer.currentWeapon.auraPrefab != null)
        {
            // 2. 검기 프리팹 생성! (이후 움직임은 프리팹 자체 스크립트가 알아서 함)
            GameObject aura = Instantiate(activablePlayer.currentWeapon.auraPrefab, transform.position, transform.rotation);
            
            // 검기에 데미지 정보 세팅 (검기 전용 스크립트에 값 넘겨주기)
            AuraProjectile auraScript = aura.GetComponent<AuraProjectile>();
            float totalDamage = activablePlayer.playerStatsManager.playerStats.AttackPower + 
            (activablePlayer.currentWeapon.baseDamage 
            * activablePlayer.currentWeapon.auraDamageMultiplier 
            * activablePlayer.playerStatsManager.playerStats.AttackPower); // 이미 계산된 데미지 사용
            auraScript.SetupAura(totalDamage, 
                activablePlayer.currentWeapon.auraSpeed, activablePlayer.currentWeapon.auraDuration, 
                activablePlayer.currentWeapon.maxAuraCount, 
                activablePlayer.currentWeapon.auraSpreadAngle);
        }
    }
}
