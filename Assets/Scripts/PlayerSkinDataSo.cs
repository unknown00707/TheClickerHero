using UnityEngine;
[CreateAssetMenu(fileName = "PlayerSkinData", menuName = "GameData/PlayerSkin")]
public class PlayerSkinDataSo : ScriptableObject
{
    [Header("스킨 정보")]
    public string skinName;           // 스킨 이름 (예: "용사의 갑옷")
    public string description;        // 스킨 설명 (예: "강력한 방어력을 자랑하는 용사의 갑옷입니다.")
    public int unlockCost;            // 스킨 잠금 해제에 필요한 재화 (예: 500)
    public Sprite unlockSprite;      // 잠금 상태에서 보여줄 스프라이트 (예: 회색 아이콘)
    public Sprite skinSprite;        // 스킨 장착 시 보여줄 스프라이트
    public AnimatorOverrideController skinOverrideController; 
    [Header("스킨 장착 시 플레이어 스텟 강화")]
    public bool isAttackMultiplier; // 공격력 배수 적용 여부
    public bool isDefenseMultiplier; // 방어력 배수 적용 여부
    public bool isHealthMultiplier; // 체력 배수 적용 여부
}
