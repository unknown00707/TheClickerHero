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
    public float multiplier = 0.0f;

    // 생성자: 처음 스탯을 만들 때 기본값을 쏙 넣어줍니다.
    public Stat(float initialBase)
    {
        baseValue = initialBase;
    }

    // 최종 스탯 계산 기능! 
    // (전체 혜택과 환생 보너스는 외부에서 던져줍니다)
    public float GetFinalValue(float benefitEffect, float reincarnationBonus)
    {
        // 내 업그레이드 배수(20%) + 이로운 효과(10%) + 환생 보너스(30%) = 총 60% 증가
        float totalPercent = multiplier + benefitEffect + reincarnationBonus;

        // 고정 추가치(flatUpgrade)를 더한 기본값에 최종 퍼센트를 덧셈 방식으로 곱해줍니다.
        return (baseValue + flatUpgrade) * (1.0f + totalPercent);
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
    // 스탯 생성 (기본값 설정)
    public Stat health = new(100.0f);
    public Stat attackPower = new(10.0f);
    public Stat attackSpeed = new(1.0f);
    public Stat defense = new(5.0f);
    public Stat criticalChance = new(0.0f);   // 크확은 0%부터 시작하는 게 자연스럽습니다.
    public Stat criticalDamage = new(0.5f);   // 크뎀 기본 보너스 +50% (기본 대미지의 150%)
    public Stat click = new(1.0f);      // 변수 타입을 float으로 통일하여 계산 에러 방지
    public Stat speed = new(1.0f);
    public Stat rareGoodsProbability = new(0.01f);
    
    // 순수 버프 수치 (20%면 0.2f 상태로 저장됨)
    public float benefitEffect = 0.0f;
    public float reincarnationBonus = 0.0f;
    
    public float Health => health.GetFinalValue(benefitEffect, reincarnationBonus);
    public float AttackPower => attackPower.GetFinalValue(benefitEffect, reincarnationBonus);
    public float AttackSpeed => attackSpeed.GetFinalValue(benefitEffect, reincarnationBonus);
    public float Defense => defense.GetFinalValue(benefitEffect, reincarnationBonus);
    public float CriticalChance => criticalChance.GetFinalValue(benefitEffect, reincarnationBonus);
    public float CriticalDamage => criticalDamage.GetFinalValue(benefitEffect, reincarnationBonus);
    public float Click => click.GetFinalValue(benefitEffect, reincarnationBonus);
    public float ClickMultiplier => click.multiplier;
    public float Speed => speed.GetFinalValue(benefitEffect, reincarnationBonus);
    public float RareGoodsProbability => rareGoodsProbability.GetFinalValue(benefitEffect, reincarnationBonus);
    public float ToTalbenefit => benefitEffect + reincarnationBonus;
    public void ResetStatsForReincarnation(bool keepBonus)
    {
        // 1. 초기값으로 되돌릴 스탯들
        health = new Stat(100.0f);
        attackPower = new Stat(10.0f);
        defense = new Stat(5.0f);
    
        criticalChance = new Stat(0.0f);
        criticalDamage = new Stat(1.0f);
        click = new Stat(1);

        speed = new Stat(1.0f);
        // 2. 환생 보너스나 특수 혜택은 유지하거나 누적
        if (!keepBonus)
        {
            rareGoodsProbability = new Stat(0.01f);
            benefitEffect = 0.0f;
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
            { "클릭", () => playerStats.Click },
            { "클릭 배수", () => playerStats.click.multiplier },
            { "이동 속도", () => playerStats.Speed },
            { "희귀 확률", () => playerStats.RareGoodsProbability },
            { "이로운 효과", () => playerStats.benefitEffect },
            { "환생 보너스", () => playerStats.reincarnationBonus },
            { "총 혜택", () => playerStats.ToTalbenefit }
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
        playerStatsText[5].text = playerStats.Click.ToString();
        playerStatsText[6].text = (playerStats.benefitEffect * 100).ToString("F1") + "%";
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
