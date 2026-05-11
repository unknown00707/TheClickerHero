using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "GameData/Weapon")]
public class WeaponDataSo : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponName;          // 무기 이름 (예: 전설의 나선검)
    public string description;           // 무기 설명 (예: "강력한 나선형 검기로 적을 공격합니다.")
    public int unlockCost;             // 무기 잠금 해제에 필요한 재화 (예: 1000)
    public Sprite unlockSprite;       // 잠금 상태에서 보여줄 스프라이트 (예: 회색 아이콘)
    public Sprite weaponSprite;       // 무기 장착 시 보여줄 스프라이트
    public AnimatorOverrideController weaponOverrideController; // 무기 자체 애니메이션 (예: 검 휘두르는 모션)
    public AnimatorOverrideController weaponEffectOverrideController; // 무기 효과 애니메이션 (예: 검기 발사 모션)

    [Header("근접 타격 판정 (Melee)")]
    public float baseDamage;             // 무기 기본 데미지
    public float meleeAttackRadius;    // OverlapCircle의 반지름 (크기)
    public Vector2[] meleeOffset;        // 플레이어 기준 타격 중심점

    [Header("검기 (Aura) 설정")]
    public GameObject auraPrefab;      // 발사할 검기 프리팹 (직선형, 나선형 등)
    public float auraDamageMultiplier; // 검기 데미지 배율 (예: 0.5f면 근접의 절반 데미지)
    public float auraSpeed;           // 검기 이동 속도
    public float auraDuration;        // 검기 지속 시간 (초)
    public int maxAuraCount;           // 최대 발사 가능한 검기 수 (예: 3개)
    public float auraSpreadAngle;     // 검기 퍼지는 각도 (예: 30도면 중앙 1개, 양옆으로 15도씩 2개)
}