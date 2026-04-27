using System;
using System.IO;
using TMPro;
using UnityEngine;

[Serializable]
public class PlayerStasData
{
    [SerializeField] private float health = 100.0f;
    [SerializeField] public float attackPower = 10.0f;
    [SerializeField] private float defense = 5.0f;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float criticalChance = 0.0f;
    [SerializeField] private float criticalDamage = 0.0f;
    [SerializeField] private int coinByClick = 1;
    [SerializeField] private float rareGoodsProbability = 0.01f;
    [SerializeField] private float benfitEffect = 0.0f;
    [SerializeField] private float reincarnationBonus = 0.0f;
    public float Health
    {
        get { return health; } // 체력 얼마인지 보여줘!
        set 
        { 
            health = Mathf.Max(0, value); // 0과 들어온 값 중 큰 것을 선택
        }
    }
    public float AttackPower
    {
        get { return attackPower; } // 공격력 얼마인지 보여줘!
        set 
        { 
            attackPower = Mathf.Max(0, value); 
        }
    }
    public float Defense
    {
        get { return defense; } // 방어력 얼마인지 보여줘!
        set 
        { 
            defense = Mathf.Max(0, value); 
        }
    }
    public float Speed
    {
        get { return speed; } // 속도 얼마인지 보여줘!
        set 
        { 
            speed = Mathf.Max(1, value); 
        }
    }
    public float CriticalChance
    {
        get { return criticalChance; } // 크리티컬 확률 얼마인지 보여줘!
        set 
        { 
            criticalChance = Mathf.Clamp01(value); 
        }
    }
    public float CriticalDamage
    {
        get { return criticalDamage; } // 크리티컬 데미지 amount
        set 
        { 
            criticalDamage = Mathf.Max(0, value); 
        }
    }
    public int CoinByClick
    {
        get { return coinByClick; } // 클릭당 코인 amount
        set 
        { 
            coinByClick = Mathf.Max(1, value); 
        }
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
}
public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStasData playerStats = new();
    public TextMeshProUGUI[] playerStatsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadPlayerStats();
    }

    // Update is called once per frame
    void Update()
    {
        UpDatePlayerStatsText();
    }

    void UpDatePlayerStatsText()
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

    public void SavePlayerStats()
    {
        string jsonData = JsonUtility.ToJson(playerStats, true);
        string path = Path.Combine(Application.persistentDataPath, "SavePlayerStatsData.json");
        File.WriteAllText(path, jsonData);
        Debug.Log("저장 완료!" + path);
    }

    public void LoadPlayerStats()
    {
        string path = Path.Combine(Application.persistentDataPath, "SavePlayerStatsData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonData, playerStats);
            Debug.Log("불러오기 완료!" + path);
        }
        else
        {
            Debug.LogWarning("저장된 플레이어 스탯 데이터가 없습니다.");
        }
    }
}
