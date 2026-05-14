using System;
using System.IO;
using TMPro;
using UnityEngine;

[Serializable]
public class PlayerStasData
{
    [SerializeField] private float health = 100.0f;
    [SerializeField] private float upgradeHealth = 0.0f; // 업그레이드로 증가한 체력
    [SerializeField] private float multiplierHealth = 1.0f; // 배수로 증가한 체력
    [SerializeField] public float attackPower = 10.0f;
    [SerializeField] private float upgradeAttackPower = 0.0f; // 업그레이드로 증가한 공격력
    [SerializeField] private float multiplierAttackPower = 1.0f; // 배수로 증가한 공격력
    [SerializeField] private float defense = 5.0f;
    [SerializeField] private float upgradeDefense = 0.0f; // 업그레이드로 증가한 방어력
    [SerializeField] private float multiplierDefense = 1.0f; // 배수로 증가한 방어력
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float criticalChance = 0.0f;
    [SerializeField] private float criticalDamage = 0.0f;
    [SerializeField] private float upgradeCriticalDamage = 0.0f; // 업그레이드로 증가한 크리티컬 데미지
    [SerializeField] private float multiplierCriticalDamage = 1.0f; // 배수로 증가한 크리티컬 데미지
    [SerializeField] private int coinByClick = 1;
    [SerializeField] private int upgradeCoinByClick = 0; // 업그레이드로 증가한 클릭당 코인
    [SerializeField] private float multiplierCoinByClick = 1.0f; // 배수로 증가한 클릭당 코인
    [SerializeField] private float rareGoodsProbability = 0.01f;
    [SerializeField] private float benfitEffect = 0.0f;
    [SerializeField] private float reincarnationBonus = 0.0f;
    public float Health
    {
        get { return  (health + upgradeHealth) * multiplierHealth * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 체력 얼마인지 보여줘!
    }
    public void UpgradeHealth(bool isPercentage, float amount)
    {
        if (isPercentage)
            multiplierHealth += amount / 100.0f; // 퍼센트로 증가
        
        else
            upgradeHealth += amount; // 고정값으로 증가
    }
    public float AttackPower
    {
        get { return  (attackPower + upgradeAttackPower) * multiplierAttackPower * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 공격력 얼마인지 보여줘!
    }
    public void UpgradeAttackPower(bool isPercentage, float amount)
    {
        if (isPercentage)
            multiplierAttackPower += amount / 100.0f; // 퍼센트로 증가
        else
            upgradeAttackPower += amount; // 고정값으로 증가
    }
    public float Defense
    {
        get { return  (defense + upgradeDefense) * multiplierDefense * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 방어력 얼마인지 보여줘!
    }
    public void UpgradeDefense(bool isPercentage, float amount)
    {
        if (isPercentage)
            multiplierDefense += amount / 100.0f; // 퍼센트로 증가
        else
            upgradeDefense += amount; // 고정값으로 증가
    }
    public float Speed
    {
        get { return speed * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 속도 얼마인지 보여줘!
        set 
        { 
            speed = Mathf.Max(1, value); 
        }
    }
    public float CriticalChance
    {
        get { return criticalChance * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 크리티컬 확률 얼마인지 보여줘!
        set 
        { 
            criticalChance = Mathf.Clamp01(value); 
        }
    }
    public float CriticalDamage
    {
        get { return  (criticalDamage + upgradeCriticalDamage) * multiplierCriticalDamage * (1.0f + benfitEffect) * (1.0f + reincarnationBonus); } // 크리티컬 데미지 amount
    }
    public void UpgradeCriticalDamage(bool isPercentage, float amount)
    {
        if (isPercentage)
            multiplierCriticalDamage += amount / 100.0f; // 퍼센트로 증가
        else
            upgradeCriticalDamage += amount; // 고정값으로 증가
    }
    public int CoinByClick
    {
        get { return (int)((coinByClick + upgradeCoinByClick) * multiplierCoinByClick * (1.0f + benfitEffect) * (1.0f + reincarnationBonus)); } // 클릭당 코인 amount
    }
    public void UpgradeCoinByClick(bool isPercentage, int amount)
    {
        if (isPercentage)
            multiplierCoinByClick += amount / 100.0f; // 퍼센트로 증가
        else
            upgradeCoinByClick += amount; // 고정값으로 증가
    }
    public float RareGoodsProbability
    {
        get { return rareGoodsProbability; } // 희귀 아이템 확률
        set 
        { 
            rareGoodsProbability = Mathf.Clamp01(value); 
        }
    }
    public float BenfitEffect
    {
        get { return benfitEffect; } // 혜택 효과
        set 
        { 
            benfitEffect = Mathf.Max(0, value); 
        }
    }
    public float ReincarnationBonus
    {
        get { return reincarnationBonus; } // 환생 보너스
        set 
        { 
            reincarnationBonus = Mathf.Max(0, value); 
        }
    }

    public void ResetStatsForReincarnation(bool keepBonus)
    {
        // 1. 초기값으로 되돌릴 스탯들
        health = 100.0f;
        upgradeHealth = 0.0f;
        multiplierHealth = 1.0f;
        attackPower = 10.0f;
        upgradeAttackPower = 0.0f;
        multiplierAttackPower = 1.0f;
        defense = 5.0f;
        upgradeDefense = 0.0f;
        multiplierDefense = 1.0f;
        speed = 1.0f;
        criticalChance = 0.0f;
        criticalDamage = 0.0f;
        upgradeCriticalDamage = 0.0f;
        multiplierCriticalDamage = 1.0f;
        coinByClick = 1;
        upgradeCoinByClick = 0;
        multiplierCoinByClick = 1.0f;

        // 2. 환생 보너스나 특수 혜택은 유지하거나 누적
        if (!keepBonus)
        {
            rareGoodsProbability = 0.01f;
            benfitEffect = 0.0f;
            reincarnationBonus = 0.0f;
        }
    }
}
public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStasData playerStats = new();
    public TextMeshProUGUI[] playerStatsText;
    private readonly String SAVE_FILE_NAME = "SavePlayerStatsData.json";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadPlayerStats();
    }

    public void UpDatePlayerStatsText()
    {
        playerStatsText[0].text = playerStats.AttackPower.ToString("F0") + " + " + "무기 데미지 들어갈 예정";
        playerStatsText[1].text = playerStats.Defense.ToString("F0");
        playerStatsText[2].text = playerStats.Health.ToString("F0");
        playerStatsText[3].text = (playerStats.CriticalChance * 100).ToString("F1") + " + " + "무기 보정" +"%";
        playerStatsText[4].text = (playerStats.CriticalDamage * 100).ToString("F1") + " + " + "무기 보정" +"%";
        playerStatsText[5].text = playerStats.CoinByClick.ToString();
        playerStatsText[6].text = (playerStats.BenfitEffect * 100).ToString("F1") + "%";
        playerStatsText[7].text = playerStats.Speed.ToString("F1");
        playerStatsText[8].text = (playerStats.RareGoodsProbability * 100).ToString("F1") + "%";
        playerStatsText[9].text = (playerStats.ReincarnationBonus * 100).ToString("F1") + "%";
    }

    public void SavePlayerStats() => GameManger.instance.SaveData(playerStats, SAVE_FILE_NAME);
    public void LoadPlayerStats() => GameManger.instance.LoadData(playerStats, SAVE_FILE_NAME);
}
