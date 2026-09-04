using UnityEngine;

[CreateAssetMenu(fileName = "AutoOnlineUprade", menuName = "GameData/AutoOnlineUprade")]
public class AutoOnlineUpradeSo : ScriptableObject
{
    [Header("Auto Online Upgrade Settings")]
    public int needClick; 
    public float upgradeSpeedAmount; // 온라인 보상 주기 속도
    public int canApplyMaxEnemyID; // EnemyManager 에서 이 ID 이하의 적에게만 적용
    public float rareProbabilityOfEnemy; // 확률이 높을 수록 ID 큰 적이 나타남 -> 보상 증가 / 0~1 사이 값
    [Header("Auto Online UI")]
    public Sprite upgradeSprite;
    public Sprite unlockSprite;
}
