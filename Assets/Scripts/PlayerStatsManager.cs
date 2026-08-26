using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
[Serializable]
public class Stat
{
    public readonly float baseValue;
    public float flatUpgrade;
    public float multiplier = 1.0f;

    // 생성자: 처음 스탯을 만들 때 기본값을 쏙 넣어줍니다.
    public Stat(float initialBase)
    {
        baseValue = initialBase;
    }

    // 최종 스탯 계산 기능! 
    // (전체 혜택과 환생 보너스는 외부에서 던져줍니다)
    public float GetFinalValue(float benefitEffect, float reincarnationBonus)
    {
        return (baseValue + flatUpgrade) * multiplier * (1.0f + benefitEffect) * (1.0f + reincarnationBonus);
    }

    // 업그레이드 기능!
    public void Upgrade(bool isPercentage, float amount)
    {
        if (isPercentage)
            multiplier += amount / 100.0f;
        else
            flatUpgrade += amount;
    }
}

[Serializable]
public class PlayerStasData
{
    public Stat health = new(100.0f);
    public Stat attackPower = new(10.0f);
    public Stat defense = new(5.0f);
    public Stat criticalChance = new(0.001f);
    public Stat criticalDamage = new(1.0f);
    public Stat coinByClick = new(1);
    public Stat speed = new(1.0f);
    public Stat rareGoodsProbability = new(0.01f);
    public float benfitEffect = 0.0f;
    public float reincarnationBonus = 0.0f;
    
    public float Health => health.GetFinalValue(benfitEffect, reincarnationBonus);
    public float AttackPower => attackPower.GetFinalValue(benfitEffect, reincarnationBonus);
    public float Defense => defense.GetFinalValue(benfitEffect, reincarnationBonus);
    public float CriticalChance => criticalChance.GetFinalValue(benfitEffect, reincarnationBonus);
    public float CriticalDamage => criticalDamage.GetFinalValue(benfitEffect, reincarnationBonus);
    public float CoinByClick => coinByClick.GetFinalValue(benfitEffect, reincarnationBonus);
    public float Speed => speed.GetFinalValue(benfitEffect, reincarnationBonus);
    public float RareGoodsProbability => rareGoodsProbability.GetFinalValue(benfitEffect, reincarnationBonus);
    public float ToTalbenfit => 1f + (benfitEffect * reincarnationBonus);
    public void ResetStatsForReincarnation(bool keepBonus)
    {
        // 1. 초기값으로 되돌릴 스탯들
        health = new Stat(100.0f);
        attackPower = new Stat(10.0f);
        defense = new Stat(5.0f);
    
        criticalChance = new Stat(0.0f);
        criticalDamage = new Stat(1.0f);
        coinByClick = new Stat(1);

        speed = new Stat(1.0f);
        // 2. 환생 보너스나 특수 혜택은 유지하거나 누적
        if (!keepBonus)
        {
            rareGoodsProbability = new Stat(0.01f);
            benfitEffect = 0.0f;
            reincarnationBonus = 0.0f;
        }
    }
}
public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStasData playerStats = new();
    public TextMeshProUGUI[] playerStatsText;
    private Dictionary<string, Func<float>> statGetters = new();
    private readonly string SAVE_FILE_NAME = "SavePlayerStatsData.json";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadPlayerStats();
        UpDatePlayerStatsText();
        InitStatGettersDict();
    }
    private void InitStatGettersDict()
    {
        // 딕셔너리 초기화 (대소문자 무시 설정 추가)
        statGetters = new Dictionary<string, Func<float>>(StringComparer.OrdinalIgnoreCase)
        {
            { "공격력", () => playerStats.AttackPower },
            { "체력", () => playerStats.Health },
            { "방어력", () => playerStats.Defense },
            { "크리티컬 확률", () => playerStats.CriticalChance },
            { "크리티컬 데미지", () => playerStats.CriticalDamage },
            { "클릭당 코인", () => playerStats.CoinByClick },
            { "클릭 배수", () => playerStats.coinByClick.multiplier },
            { "이동 속도", () => playerStats.Speed },
            { "희귀 확률", () => playerStats.RareGoodsProbability },
            { "이로운 효과", () => playerStats.benfitEffect },
            { "환생 보너스", () => playerStats.reincarnationBonus },
            { "총 혜택", () => playerStats.ToTalbenfit }
        };
    }
    public float GetStatValueInDict(string statName)
    {
        if (statGetters.TryGetValue(statName, out var getter))
        {
            return getter(); // 연결된 프로퍼티 값 바로 반환
        }

        Debug.LogWarning($"[StatsManager] '{statName}'에 해당하는 스탯을 찾을 수 없습니다.");
        return 0f;
    }
    public void UpDatePlayerStatsText()
    {
        playerStatsText[0].text = playerStats.AttackPower.ToString("F0") + " + " + "무기 데미지 들어갈 예정";
        playerStatsText[1].text = playerStats.Defense.ToString("F0");
        playerStatsText[2].text = playerStats.Health.ToString("F0");
        playerStatsText[3].text = (playerStats.CriticalChance * 100).ToString("F1") + " + " + "무기 보정" +"%";
        playerStatsText[4].text = (playerStats.CriticalDamage * 100).ToString("F1") + " + " + "무기 보정" +"%";
        playerStatsText[5].text = playerStats.CoinByClick.ToString();
        playerStatsText[6].text = (playerStats.benfitEffect * 100).ToString("F1") + "%";
        playerStatsText[7].text = playerStats.Speed.ToString("F1");
        playerStatsText[8].text = (playerStats.RareGoodsProbability * 100).ToString("F1") + "%";
        playerStatsText[9].text = (playerStats.reincarnationBonus * 100).ToString("F1") + "%";
    }

    public void SavePlayerStats() => GameManger.instance.SaveData(playerStats, SAVE_FILE_NAME);
    public void LoadPlayerStats()
    {
        GameManger.instance.LoadData(playerStats, SAVE_FILE_NAME);
    } 
}
