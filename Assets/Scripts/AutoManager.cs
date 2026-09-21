using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UnlockAutoOnlineUprade
{
    public int unlockedMaxID; // 현재까지 해금된 AutoOnlineUpradeSo의 최대 ID --> 이 값을 강제 적용
}
[Serializable]
public class PlayerTime
{
    public float loginTime; // 게임 시작 시간
    public float playTime; // 플레이 시간
    public float logoutTime; // 게임 종료 시간
    public string day; // 게임 시작 날짜
}
[Serializable]
public class TimeLog
{
    public List<PlayerTime> times = new();
}

public class AutoManager : MonoBehaviour
{
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조
    public PlayerSkinManager playerSkinManager; // 플레이어 스킨 매니저 참조

    [Header("Offline")]
    public GameObject offlineRewardObj;
    public TextMeshProUGUI offlineRewardTitle;
    public TextMeshProUGUI offlineRewardContent;

    [Header("Offline Reward")]
    private int totalClickReward; // 오프라인 보상으로 지급할 총 클릭 수
    private readonly TimeLog timeLog = new();
    private DateTime loginTime; // 게임 시작 시간
    private string todayDate; // 오늘 날짜
    private readonly int MAX_OFFLINE_HOURS = 3; // 최대 오프라인 시간 제한 (24시간)
    private readonly string SAVE_TIME_LOG_FILE_NAME = "TimeLog.json"; // 저장 파일 이름

    [Header("Auto Battle Data")]
    public AutoOnlineUpradeSo[] autoOnlineUpradeSo;
    public Image autoOnlineUpgradeIMG;
    public TextMeshProUGUI autoOnlineUpgradeTxt;
    public GameObject autoOnlineUpgradeLockBTN; // 잠금 상태일 때 버튼
    public GameObject autoOnlineApplyBTN; // 단순 Sprite 바꾸는 버튼 --> 배경 꾸미기 용
    private readonly UnlockAutoOnlineUprade unlockAutoOnlineUprade = new();
    private float autoOnlineRewardCycle = 300f; // == attackSpeed --> 공격 속도에 따라 보상 주기 달라짐. 
    private float rareProbabilityOfEnemy = 0; // 적의 희귀 확률 --> 이 값이 높을 수록 ID 큰 적이 나타남 -> 보상 증가
    private int gettenCoin = 0;
    private int gettenClick = 0;
    private readonly string SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME = "UnlockAutoOnlineUprade.json"; // 저장 파일 이름

    [Header("Auto Player Animator")]
    public Animator autoPlayerAnimator;
    public Animator autoPlayerWeaponAnimator;
    public Animator autoPlayerWeaponEffectAnimator;
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int AttackSpeedHash = Animator.StringToHash("attackSpeed");

    [Header("Auto Battle Objects Manager")]
    public EnemyComme autoEnemyPrefab;
    private Queue<EnemyComme> _pool; // 적 인스턴스 풀링을 위한 Object Pool



    void Awake()
    {
        // 게임 시작 시간 기록
        loginTime = DateTime.Now;
        todayDate = loginTime.ToString("yyyy-MM-dd");
        LoadTimeLog(); // 게임 시작 시 시간 로그 로드
        Debug.Log("게임 시작 시간: " + todayDate + " : " + loginTime);
        offlineRewardObj.SetActive(false); // 혹시 모르니.
        OfflineReward(); // 오프라인 보상 지급

        // Online
        LoadUnlockAutoOnlineUpgrade(); // 게임 시작 시 해금된 업그레이드 로드
        ApplyAutoOnlineUpgradeStat(); // 해금된 업그레이드 스탯 적용
        UpdateAutoOnlineUpgradeUI(unlockAutoOnlineUprade.unlockedMaxID); // UI 업데이트

        // Pooling
        PoolInit(); // 적 인스턴스 풀링 초기화
    }

    void Start()
    {
        // 게임 시작 시 현재 장착된 스킨의 AnimatorOverrideController 적용
        ApplyAutoPlayerAnimatorOverride();
        AutoAnimationInit(); // 오토 플레이어 애니메이션 초기화
    }

    // --------------------- 오프라인 자동 보상 로직 ---------------------//
    float CalculateOfflineTime()
    {
        if (timeLog.times.Count > 0)
        {
            // 가장 최근(마지막) 세션 데이터 추출
            PlayerTime lastSession = timeLog.times[^1];

            // 마지막 세션의 '종료 날짜 + 종료 시각'을 하나의 문자열로 결합 (예: "2026-08-12 15:30:22")
            string lastExitString = $"{lastSession.day} {lastSession.logoutTime}";
            
            if (DateTime.TryParse(lastExitString, out DateTime lastExitTime))
            {
                // [수정] 현재 로그인 시간 - 과거 로그아웃 시간 (과거를 빼야 양수가 나옵니다)
                float offlineSeconds = (float)(loginTime - lastExitTime).TotalSeconds;

                // 음수 예외 처리 (기기 시간 조작 등 방지)
                if (offlineSeconds < 0) offlineSeconds = 0;

                // 최대 오프라인 시간 제한 적용 (초 단위 변환)
                float maxOfflineSeconds = MAX_OFFLINE_HOURS * 3600f;
                if (offlineSeconds > maxOfflineSeconds)
                {
                    offlineSeconds = maxOfflineSeconds;
                }

                // 오프라인 보상 지급 로직 호출
                return offlineSeconds;
            }
            else
            {
                Debug.LogError("마지막 종료 시간 포맷 파싱에 실패했습니다.");
            }
        }
        else
        {
            // 유저가 게임을 아예 처음 가입/실행한 경우
            Debug.Log("최초 접속 유저이므로 오프라인 보상이 없습니다.");
        }

        return 0; // 오프라인 시간이 없거나 계산에 실패한 경우 0 반환
    }
    void OfflineReward()
    {
        float offlineSeconds = CalculateOfflineTime();
        float baseRewardPerSecond = 1f; // 1초당 주는 최소한의 기본 보상 (예시)
        float offlineHours = offlineSeconds / 3600f; // 오프라인 시간을 '시간' 단위로 변환 (소수점 유지, 예: 1시간 30분 = 1.5f)

        // 2. 기본 초당 보상 계산
        float defaultReward = offlineSeconds * baseRewardPerSecond;

        // 3. 플레이어의 노력으로 커지는 시간당 보상 계산 (int를 빼서 소수점까지 정밀 계산)
        float effortRewardPerHour = 100f; // 플레이어 노력 보상의 '기준 시간당 비용' (예시)
        float playerBonusReward = offlineHours 
                                * effortRewardPerHour
                                * playerStatsManager.playerStats.benefitEffect 
                                * playerStatsManager.playerStats.reincarnationBonus;

        // 4. 최종 합산 후 딱 한 번만 int로 변환 (소수점 버그 방지)
        totalClickReward = (int)(defaultReward + playerBonusReward);

        //ui set
        offlineRewardTitle.text = $"{ColorPalette.Rare}{LanguageManager.Instance.GetText("AUTO_OFFLINE_TITLE")}{ColorPalette.End}";
       
        string coloredValue = $"{ColorPalette.Yellow}{totalClickReward}{ColorPalette.End}";
        string rawText = LanguageManager.Instance.GetText("AUTO_OFFLINE_CONTENT");
        offlineRewardContent.text = rawText.ReplaceTags("GettenCoin", coloredValue);
        offlineRewardObj.SetActive(true);
    }
    public void GetOfflineReward()
    {
        playerStatsManager.playerStats.click.Upgrade(false, totalClickReward);
        offlineRewardObj.SetActive(false);
        Debug.Log($"오프라인 보상 지급 완료! 총 클릭 수: {totalClickReward}");
    }
    public void SaveTimeLog()
    {
        DateTime logoutTime = DateTime.Now;
        float sessionSeconds = (float)(logoutTime - loginTime).TotalSeconds;

        // 4. 새로운 독립 세션 데이터 객체 생성
        PlayerTime newSession = new()
        {
            loginTime = (float)loginTime.TimeOfDay.TotalSeconds,
            playTime = sessionSeconds,
            logoutTime = (float)logoutTime.TimeOfDay.TotalSeconds,
            day = todayDate
        };

        // 5. 기존 리스트의 끝에 새로 만든 세션 데이터 추가 (중첩 저장)
        timeLog.times.Add(newSession);

        // 6. JSON 파일로 변환 및 저장
        GameManger.instance.SaveData(timeLog, SAVE_TIME_LOG_FILE_NAME);
    }
    public void LoadTimeLog()
    {
        if(!File.Exists(Path.Combine(Application.persistentDataPath, SAVE_TIME_LOG_FILE_NAME)))
            GameManger.instance.SaveData(timeLog, SAVE_TIME_LOG_FILE_NAME); // 파일이 없으면 새로 저장
        GameManger.instance.LoadData(timeLog, SAVE_TIME_LOG_FILE_NAME);
    }

    // --------------------- 플레이어와 몬스터의 자동 사냥 ----------------------//
    public void OnKillEnemyByAutoPlayer()
    {
        gettenCoin++;
        gettenClick++;
    }
//------------------- 오토 UI 관련 ---------------------------------//
    public void UpdateAutoOnlineUpgradeUI(int currentSoID)
    {
        // 현재 적 ID에 따라 적용 가능한 업그레이드 찾기
        AutoOnlineUpradeSo applyUISo = autoOnlineUpradeSo[currentSoID];

        if (unlockAutoOnlineUprade.unlockedMaxID >= currentSoID) // 현재 해금된 업그레이드 ID와 비교
        {
            autoOnlineUpgradeIMG.sprite = applyUISo.unlockSprite;
            autoOnlineUpgradeTxt.text = $"업그레이드 필요 클릭: {applyUISo.needClick}\n" +
                                        $"업그레이드 속도: {applyUISo.upgradeSpeedAmount}\n" +
                                        $"희귀 확률: {applyUISo.rareProbabilityOfEnemy * 100}%";
            autoOnlineApplyBTN.SetActive(true);
            autoOnlineUpgradeLockBTN.SetActive(false);
        }
        else
        {
            autoOnlineUpgradeIMG.sprite = applyUISo.lockedSprite; // 또는 기본 이미지로 설정
            autoOnlineUpgradeTxt.text = "적용 가능한 업그레이드 없음";
            autoOnlineApplyBTN.SetActive(false);
            autoOnlineUpgradeLockBTN.SetActive(true);
        }
    }
    public void UnlockAutoOnlineUpgrade(int currentSoID)
    {
        if (currentSoID > unlockAutoOnlineUprade.unlockedMaxID)
        {
            unlockAutoOnlineUprade.unlockedMaxID = currentSoID;
            SaveUnlockAutoOnlineUpgrade(); // 변경 사항 저장
            ApplyAutoOnlineUpgradeStat(); // 스탯 적용
            UpdateAutoOnlineUpgradeUI(currentSoID); // UI 업데이트
            Debug.Log($"AutoOnlineUprade ID {currentSoID} 해금 완료!");
        }
        else
        {
            Debug.Log($"AutoOnlineUprade ID {currentSoID}는 이미 해금되어 있습니다.");
        }
    }
//-------------------- 오토 애니메이션 --------------------------------//
    private void AutoAnimationInit()
    {
        autoOnlineRewardCycle = autoOnlineUpradeSo
                                .Take(unlockAutoOnlineUprade.unlockedMaxID + 1)
                                .Sum(so => so.upgradeSpeedAmount);
        rareProbabilityOfEnemy = autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].rareProbabilityOfEnemy;
    
        autoPlayerAnimator.SetFloat(XHash, -1); // 왼쪽
        autoPlayerAnimator.SetFloat(YHash, 0); // 기본
        autoPlayerWeaponAnimator.SetFloat(MoveSpeedHash, 1); // 항상 움직여야되서 !=0 만 만족하면 됌.
        
        autoPlayerAnimator.SetFloat(AttackSpeedHash, autoOnlineRewardCycle);
        autoPlayerWeaponAnimator.SetFloat(AttackSpeedHash, autoOnlineRewardCycle);
        autoPlayerWeaponEffectAnimator.SetFloat(AttackSpeedHash, autoOnlineRewardCycle);
    }
    public void SetSameAnimeOverride(WeaponDataSo currentWeapon)
    {
        // 플레이어는 나중에
        autoPlayerWeaponAnimator.runtimeAnimatorController = currentWeapon.weaponOverrideController;
        autoPlayerWeaponEffectAnimator.runtimeAnimatorController = currentWeapon.weaponEffectOverrideController;
    }
    public void ApplyAutoPlayerAnimatorOverride() // playerSkin Equipped 버튼에 적용
    {
        autoPlayerAnimator.runtimeAnimatorController = playerSkinManager.GetAnimatorOverrideCurrentEquipped();
    }
    private void ApplyAutoOnlineUpgradeStat()
    {
        autoOnlineRewardCycle += autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].upgradeSpeedAmount;
        rareProbabilityOfEnemy = autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].rareProbabilityOfEnemy;
    }
//---------------------- 오토 세이브 -----------------------------------//
    public void SaveUnlockAutoOnlineUpgrade()
    {
        GameManger.instance.SaveData(unlockAutoOnlineUprade, SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME);
    }
    public void LoadUnlockAutoOnlineUpgrade()
    {
        if(!File.Exists(Path.Combine(Application.persistentDataPath, SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME)))
            GameManger.instance.SaveData(unlockAutoOnlineUprade, SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME); // 파일이 없으면 새로 저장
        GameManger.instance.LoadData(unlockAutoOnlineUprade, SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME);
    
    }

// --------------------- 오토 배틀 적 인스턴스 풀링 ---------------------//
    private void PoolInit(int initialCapacity = 10)
    {
        for (int i = 0; i < initialCapacity; i++)
        {
            EnemyComme obj = Instantiate(autoEnemyPrefab, transform);
            obj.enemyTransform.gameObject.SetActive(false);
            _pool.Enqueue(obj); 
        }
    }
    public EnemyComme GetEnemy()
    {
        EnemyComme obj;

        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Instantiate(autoEnemyPrefab, transform);
        }

        obj.enemyTransform.gameObject.SetActive(true);
        return obj;
    }
    public void ReleaseEnemy(EnemyComme obj)
    {
        if (_pool.Contains(obj)) return;

        obj.enemyTransform.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
