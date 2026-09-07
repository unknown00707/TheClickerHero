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
    public Stat health;
    public Stat attackPower;
    public Stat attackSpeed;
    public Stat defense;
    public Stat criticalChance;   // 크확은 0%부터 시작하는 게 자연스럽습니다.
    public Stat criticalDamage;   // 크뎀 기본 보너스 +50% (기본 대미지의 150%)
    public Stat click;      // 변수 타입을 float으로 통일하여 계산 에러 방지
    public Stat speed;
    public Stat rareGoodsProbability;
    
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
    public PlayerStasData(Dictionary<string, float> baseStats)
    {
        InitializeStats(baseStats);
    }
    private void InitializeStats(Dictionary<string, float> baseStats)
    {
        // 딕셔너리에서 값을 찾고, 없으면 예외 방지를 위해 기본 디폴트값을 제공합니다.
        health               = new Stat(baseStats.GetValueOrDefault("Health", 100f));
        attackPower          = new Stat(baseStats.GetValueOrDefault("AttackPower", 10f));
        attackSpeed          = new Stat(baseStats.GetValueOrDefault("AttackSpeed", 1f));
        defense              = new Stat(baseStats.GetValueOrDefault("Defense", 5f));
        criticalChance       = new Stat(baseStats.GetValueOrDefault("CriticalChance", 0f));
        criticalDamage       = new Stat(baseStats.GetValueOrDefault("CriticalDamage", 0.5f));
        click                = new Stat(baseStats.GetValueOrDefault("Click", 1f));
        speed                = new Stat(baseStats.GetValueOrDefault("Speed", 0.5f));
        rareGoodsProbability = new Stat(baseStats.GetValueOrDefault("RareGoodsProbability", 0.01f));
    }
    public void ResetStatsForReincarnation(Dictionary<string, float> baseStats, bool keepBonus)
    {
        InitializeStats(baseStats);

        if (!keepBonus)
        {
            benefitEffect = 0.0f;
            reincarnationBonus = 0.0f;
        }
    }
}
public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStasData playerStats;
    public TextMeshProUGUI[] playerStatsText;
    private Dictionary<string, Func<float>> statGetters = new();
    private readonly Dictionary<string, float> baseStatsContainer = new(StringComparer.OrdinalIgnoreCase);
    private readonly string SAVE_FILE_NAME = "SavePlayerStatsData.json";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadBaseStatsFromCSV();
        playerStats = new PlayerStasData(baseStatsContainer);

        LoadPlayerStats();

        InitStatGettersDict();
        
        CheckInspectorErrors();
        UpDatePlayerStatsText();
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
        foreach (var startTxt in playerStatsText)
        {
            if (startTxt == null) continue;

            string statKey = startTxt.gameObject.name;
            float rawValue = GetStatValueInDict(statKey);

            // switch문은 내부적으로 == 연산자처럼 해시값 비교를 수행하여 초고속으로 작동합니다.
            startTxt.text = statKey switch
            {
                // 1. 퍼센트(%)로 표시할 스탯들만 묶어서 관리
                "크리티컬 확률" or "희귀 확률" or "이로운 효과" or "환생 보너스" or "총 혜택" => $"{rawValue * 100f:F1}%",
                // 2. 소수점 한 자리만 보여줄 특정 스탯
                "이동 속도" => rawValue.ToString("F1"),
                // 3. 예외 처리가 필요한 특수 스탯
                "공격력" => $"{rawValue:F0} + 무기 데미지 들어갈 예정",
                // 4. 그 외 나머지 모든 스탯 (체력, 방어력, 클릭 등)은 정수로 처리
                _ => rawValue.ToString("F0"),
            };
        }
    }
    private void CheckInspectorErrors()
    {
        if (playerStatsText == null) return;

        foreach (var startTxt in playerStatsText)
        {
            if (startTxt == null) continue;

            string objName = startTxt.gameObject.name;

            // 딕셔너리에 등록되지 않은 이름이 인스펙터에 있다면 에러 출력!
            if (!statGetters.ContainsKey(objName))
            {
                Debug.LogError($"❌ [StatsManager 비상!] UI 오브젝트 이름 '{objName}'이(가) 스탯 딕셔너리에 존재하지 않습니다! 인스펙터 이름을 확인하세요.", startTxt.gameObject);
            }
        }
    }
    public void ReincarnatePlayer(bool keepBonus)
    {
        playerStats.ResetStatsForReincarnation(baseStatsContainer, keepBonus);
        UpDatePlayerStatsText();
        SavePlayerStats();
    }


//------Save & Load Player Stats------//
    private void LoadBaseStatsFromCSV()
    {
        // 확장자(.csv)를 떼고 파일 이름만 적습니다. (Assets/Resources/PlayerBaseStats.csv)
        TextAsset csvFile = Resources.Load<TextAsset>("PlayerBaseStats");

        if (csvFile == null)
        {
            Debug.LogError("[StatsManager] CSV 파일을 Resources 폴더에서 찾을 수 없습니다!");
            return;
        }

        // 줄바꿈 기준으로 데이터를 쪼갭니다. (\r\n 및 \n 대응)
        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // i = 1 부터 시작하여 첫 줄(헤더: StatName,BaseValue)은 건너뜁니다.
        for (int i = 1; i < lines.Length; i++)
            {
            string[] columns = lines[i].Split(',');

            if (columns.Length >= 2)
            {
                string statName = columns[0].Trim();
                if (float.TryParse(columns[1].Trim(), out float baseValue))
                {
                    baseStatsContainer[statName] = baseValue;
                }
            }
        }
    }
    public void SavePlayerStats() => GameManger.instance.SaveData(playerStats, SAVE_FILE_NAME);
    public void LoadPlayerStats()
    {
        GameManger.instance.LoadData(playerStats, SAVE_FILE_NAME);
    } 
}
