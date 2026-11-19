using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UnlockAutoOnlineUprade
{
    public int unlockedMaxID; // 현재까지 해금된 AutoOnlineUpradeSo의 최대 ID --> 이 값을 강제 적용
    public int maxEnemyIndex; // 현재까지 나타날 수 있는 적의 최대 인덱스 --> EnemyManager에서 변경
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
    [Header("Manager")]
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조
    public PlayerSkinManager playerSkinManager; // 플레이어 스킨 매니저 참조
    public WeaponManager weaponManager;
    public EnemyManager enemyManager; // 적 매니저 참조
    public DungeonManager dungeonManager;
    public GoodsManager goodsManager;

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

    [Header("Online Reward")]
    public float rewardFunctionMultiplier = 2.0f;
    private int gettenCoin = 0; // 누적
    private int gettenClick = 0; // 누적

    [Header("Auto Battle Data")]
    public AutoOnlineUpradeSo[] autoOnlineUpradeSo;
    public Image autoOnlineUpgradeIMG;
    public TextMeshProUGUI autoOnlineUpgradeTxt;
    public GameObject autoOnlineUpgradeLockBTN; // 잠금 상태일 때 버튼
    private readonly UnlockAutoOnlineUprade unlockAutoOnlineUprade = new();

    private float autoOnlineRewardCycle = 300f; 
    private float xDiffBetweenPlayerAndEnemy = 0; // 절댓값으로 인해 양수만 나오게
    private float rareProbabilityOfEnemy = 0; // 적의 희귀 확률 --> 이 값이 높을 수록 ID 큰 적이 나타남 -> 보상 증가
    
    private readonly string SAVE_UNLOCK_AUTO_ONLINE_UPGRADE_FILE_NAME = "UnlockAutoOnlineUprade.json"; // 저장 파일 이름

    [Header("Auto Battle Animation")]
    public Animator autoPlayerAnimator;
    public Animator autoPlayerWeaponAnimator;
    public Animator autoPlayerWeaponEffectAnimator;
    public string playerLeftAttackAnimeClipName = "Attack_Hand_L";

    private float playerAttackAnimeCycle = 0f;
    private float playerAttackTime = 0f;
    private float enemyDieDurationTime = 0f; // 적이 죽어야할 시간
    private float autoEnemyMoveSpeed = 0f;

    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int AttackSpeedHash = Animator.StringToHash("attackSpeed");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");

    [Header("Auto Battle Objects Manager")]
    public AutoPlayer autoPlayer;
    public GameObject autoEnemyPrefab;
    public Transform autoEnemySpwanTrans; // 몬스터가 소환될 위치
    public Transform autoPlayerStandTrans; // 오토 플레이어의 서 있는 위치
    private readonly Queue<EnemyAuto> _pool = new(); // 적 인스턴스 풀링을 위한 Object Pool
    private readonly Queue<EnemyAuto> _activeEnemies = new(); // 현재 활성화된 적들을 추적하기 위한 큐
    private Coroutine autobattleCoroutine = null;
    private bool isCanHardResetAutoBattleCoroutine = false; // 코루틴을 강제로 재시작할 수 있는지 여부
    [Header("Auto Background")]
    public AutoBackground[] autoBackgrounds; // 오토 배경 스크립트 배열
    [Tooltip("오토 배경 이동 속도 배열 - layer1 부터")]
    [Range(0.1f, 1f)]
    public float[] autoBackgroundMoveSpeed; // 오토 배경 이동 속도 배열  - layer1 부터 
    private Vector3 cachedBottomLeft;
    private float cachedWorldOffsetY;
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
        UpdateAutoOnlineUpgradeUI(unlockAutoOnlineUprade.unlockedMaxID); // UI 업데이트

        // Pooling
        PoolInit(); // 적 인스턴스 풀링 초기화

        // Auto Background
        // 1. 씬 시작 시(가장 넓은 기본 뷰 상태일 때) 화면 경계와 216px 비율을 한 번만 저장
        float camDist = Mathf.Abs(Camera.main.transform.position.z);
        cachedBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, camDist));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camDist));

        float worldHeight = topRight.y - cachedBottomLeft.y;
        cachedWorldOffsetY = 216f * (worldHeight / Screen.height);
    }

    void Start()
    {
        // Online Animation
        SetSameWeaponAnimeOverride();
        ApplyAutoPlayerAnimatorOverride();
        AutoAnimationInit(); // 오토 플레이어 애니메이션 초기화 및 autoOnlineRewardCycle 초기화
        
        isCanHardResetAutoBattleCoroutine = true;
        TotalResetAutoBattleStats(nameof(Start)); // 오토 배틀 스탯 계산 및 적용
    }
    void Update()
    {
       
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
        offlineRewardContent.text = rawText.ReplaceTags("GettenClick", coloredValue);
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
    public void OnKillEnemyByAutoPlayer(int amoutClick, int amoutCoin, bool isCoin)
    {
        gettenClick += amoutClick;
        if(isCoin)
        {
            gettenCoin += amoutCoin;
            goodsManager.AddClickAndCoin(amoutClick, amoutCoin);
            // 코인 획득 애니메이션
            return;
        }
        goodsManager.AddClickAndCoin(amoutClick, 0); // 코인 제외
        // 클릭 획득 애니메이션
    }
    private IEnumerator UpdateAutoBattleRoutine()
    {
        Debug.Log($"{ColorPalette.Cyan}[AutoManager]{ColorPalette.End} UpdateAutoBattleRoutine started with reward cycle: {autoOnlineRewardCycle}, enemy move speed: {autoEnemyMoveSpeed}, enemy die duration: {enemyDieDurationTime}");
        while (true)
        {
            EnemyAuto enemy = GetEnemy(enemyManager.GetEnemyDataSoByIndex(unlockAutoOnlineUprade.maxEnemyIndex)); 
            enemy.StartAutoEnemyCoroutine(); // 적이 죽는 시간 계산 후 자동으로 Die() 호출
            autoPlayer.StartAutoAttack(playerAttackTime); // 플레이어 공격 시작
            
            yield return new WaitForSecondsRealtime(autoOnlineRewardCycle);
        }
    }
//------------------- 오토 UI 관련 ---------------------------------//
    public void UpdateAutoOnlineUpgradeUI(int currentSoID)
    {
        // 현재 적 ID에 따라 적용 가능한 업그레이드 찾기
        AutoOnlineUpradeSo applyUISo = autoOnlineUpradeSo[currentSoID];

        if (currentSoID - unlockAutoOnlineUprade.unlockedMaxID == 1) // 현재 해금된 업그레이드 ID와 비교 , 해금 할 놈
        {
            string text = $"업그레이드 필요 클릭: {applyUISo.needClick}\n" +
                            $"업그레이드 속도: {applyUISo.upgradeSpeedAmount}\n" +
                            $"희귀 확률: {applyUISo.rareProbabilityOfEnemy * 100}%";
            SetAutoOnlineUpgradUI(applyUISo.lockedSprite, true, text);
        }
        else if (currentSoID <= unlockAutoOnlineUprade.unlockedMaxID) // 이미 해금 된 놈
        {
            string text = "업그레이드 완료";
            SetAutoOnlineUpgradUI(applyUISo.unlockSprite, false, text);
        }
        else // 해금이 안되고 단계를 뛰어 넘은 놈
        {
            // autoOnlineApplyBTN.SetActive(false); ==> 구조 바꿀 뜻
            string text = "전 단계 업그레이드 필요";
            SetAutoOnlineUpgradUI(applyUISo.lockedSprite, false, text);
        }
    }
    private void SetAutoOnlineUpgradUI(Sprite sprite, bool isOpen, string text)
    {
        autoOnlineUpgradeIMG.sprite = sprite; // 또는 기본 이미지로 설정
        autoOnlineUpgradeTxt.text = text;
        autoOnlineUpgradeLockBTN.SetActive(isOpen);
    }
    public void UnlockAutoOnlineUpgrade(int currentSoID)
    {
        if (currentSoID > unlockAutoOnlineUprade.unlockedMaxID 
        && goodsManager.SpendClick(autoOnlineUpradeSo[currentSoID].needClick))
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
    private void AutoAnimationInit() // 보상 주기 & 희귀 확률
    {
        // 1. 가져올 업그레이드 개수 지정 (인덱스 보정 및 리스트 범위 초과 방지)
        int takeCount = Mathf.Clamp(unlockAutoOnlineUprade.unlockedMaxID + 1, 0, autoOnlineUpradeSo.Length);
        
        // 2. SO에 있는 차감 시간(-10, -20 등)을 모두 더합니다. 결과는 음수(예: -30)가 됩니다.
        float totalTimeReduction = autoOnlineUpradeSo.Take(takeCount).Sum(so => so.upgradeSpeedAmount);
        
        // 3. 기본 주기(300초)에서 차감 시간을 계산하여 '최종 보상 주기'를 구합니다.
        float baseCycleTime = 300f; // 5분
        float currentRewardCycleTime = baseCycleTime + totalTimeReduction; // 300 + (-30) = 270초
        // ⚠️ 버그 방지 예외 처리: 주기가 0초 이하로 떨어져 무한 루프나 에러가 나는 것을 방지
        currentRewardCycleTime = Mathf.Max(currentRewardCycleTime, 0f); 


        const float MIN_CYCLE_TIME = 0f; // 시스템이 안전하게 돌아갈 최소 시간 (예: 1초)
        autoOnlineRewardCycle = Mathf.Max(300f + totalTimeReduction, MIN_CYCLE_TIME);

        rareProbabilityOfEnemy = autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].rareProbabilityOfEnemy;
    
        autoPlayerAnimator.SetFloat(XHash, -1); // 왼쪽
        autoPlayerAnimator.SetFloat(YHash, 0); // 기본
        autoPlayerAnimator.SetInteger(MoveSpeedHash, 1);
        autoPlayerAnimator.speed = 1f;
        autoPlayerWeaponAnimator.SetFloat(AttackSpeedHash, 1f);
        autoPlayerWeaponEffectAnimator.SetFloat(AttackSpeedHash, 1f);
    }
    //-------------------- 오토 배틀 스탯 계산 --------------------------------//
    public void SetPlayerAttackAnimeCycle() // 공격 애니메이션 길이 계산
    {
        if (autoPlayerAnimator == null || autoPlayerAnimator.runtimeAnimatorController == null) 
            playerAttackAnimeCycle = 0f;

        // 1. 런타임 컨트롤러에서 이름이 일치하는 원본 애니메이션 클립을 찾습니다.
        AnimationClip[] clips = autoPlayerAnimator.runtimeAnimatorController.animationClips;
        float originLength = 0f;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == playerLeftAttackAnimeClipName)
            {
                originLength = clip.length; // 원본 순수 길이 (예: 5.0초)
                break;
            }
        }

        if (originLength == 0f) playerAttackAnimeCycle = 0f;

        // 2. 원본 길이를 현재 전체 속도(배속)로 나누어 실제 걸릴 시간을 미리 구합니다.
        float currentSpeed = autoPlayerAnimator.speed;
        
        // 혹시 모를 0 나누기 방지 예외 처리
        if (currentSpeed <= 0) currentSpeed = 1f; 

        playerAttackAnimeCycle = originLength / currentSpeed; // 예: 5.0초 / 2배속 = 2.5초
        TestLogger.Log($"{ColorPalette.Yellow}[{name}]{ColorPalette.End} attackANimeCycle : {playerAttackAnimeCycle}");
    }
    public void CalculateAutoBattleStats() // 적과 플레이어의 위치 차이 계산 및 적 이동 속도 계산
    {
        xDiffBetweenPlayerAndEnemy = 
            Mathf.Abs(autoPlayerStandTrans.position.x 
            - (autoEnemySpwanTrans.position.x + weaponManager.GetCurrentWeaponData().meleeOffset[^1].x));
        autoEnemyMoveSpeed = xDiffBetweenPlayerAndEnemy / autoOnlineRewardCycle;

        enemyDieDurationTime = autoOnlineRewardCycle - (playerAttackAnimeCycle / 2);
    }
    public void UpdateMaxEnemyIndex(int newMaxIndex)
    {
        if (newMaxIndex > unlockAutoOnlineUprade.maxEnemyIndex)
        {
            unlockAutoOnlineUprade.maxEnemyIndex = newMaxIndex;
            SaveUnlockAutoOnlineUpgrade(); // 변경 사항 저장
            Debug.Log($"최대 적 인덱스 {newMaxIndex}로 업데이트 완료!");
        }
    }
    // -------------------- 오토 애니메이션 오버라이드 적용 --------------------------------//
    public void SetSameWeaponAnimeOverride() // weapon Manager 에 직접 연결
    {
        WeaponDataSo currentWeapon = weaponManager.GetCurrentWeaponData();
        autoPlayerWeaponAnimator.runtimeAnimatorController = currentWeapon.weaponOverrideController;
        autoPlayerWeaponEffectAnimator.runtimeAnimatorController = currentWeapon.weaponEffectOverrideController;
    
        TotalResetAutoBattleStats(nameof(SetSameWeaponAnimeOverride));
    }
    public void ApplyAutoPlayerAnimatorOverride() // playerSkin Manager 에 직접 연결
    {
        autoPlayerAnimator.runtimeAnimatorController = playerSkinManager.GetAnimatorOverrideCurrentEquipped();
        
        TotalResetAutoBattleStats(nameof(ApplyAutoPlayerAnimatorOverride));
    }
    // -------------------- 오토 업그레이드 스탯 적용 --------------------------------//
    private void ApplyAutoOnlineUpgradeStat()
    {
        autoOnlineRewardCycle += autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].upgradeSpeedAmount;
        rareProbabilityOfEnemy = autoOnlineUpradeSo[unlockAutoOnlineUprade.unlockedMaxID].rareProbabilityOfEnemy;
    
        TotalResetAutoBattleStats(nameof(ApplyAutoOnlineUpgradeStat));
    }
    private void ApplyAutoAnimeSpeed()
    {
        playerAttackTime = autoOnlineRewardCycle - playerAttackAnimeCycle;
        
        if (autoOnlineRewardCycle >= playerAttackAnimeCycle)
            return;

        float speedMultiplier = playerAttackAnimeCycle / autoOnlineRewardCycle;
        autoPlayerAnimator.speed =  speedMultiplier;
        autoPlayerWeaponAnimator.SetFloat(AttackSpeedHash, speedMultiplier);
        autoPlayerWeaponEffectAnimator.SetFloat(AttackSpeedHash, speedMultiplier);

    }
    private void TotalResetAutoBattleStats(string where = "Unknown")
    {
        if (!isCanHardResetAutoBattleCoroutine) return; // 코루틴 강제 재시작 가능 여부 확인

        TestLogger.Log($"{ColorPalette.Legend}[AutoManager] HardRestAutoBattle{ColorPalette.End} In {where}()");

        SetPlayerAttackAnimeCycle();    // - playerAttackAnimeCycle
        CalculateAutoBattleStats();     // 오토 배틀 스탯 계산 - autoEnemyMoveSpeed - enemyDieDurationTime
        ApplyAutoAnimeSpeed();

        SaveUnlockAutoOnlineUpgrade(); // 저장

        // 코루틴 강제 재시작
        HardResetAutoBattleCoroutine();
        UpdateAutoBackgroundSpeed(); // 오토 배경 이동 속도 업데이트
        SetBackgroundPosInit(true);
    }

    private void HardResetAutoBattleCoroutine()
    {
        if (autobattleCoroutine != null)
        {
            StopCoroutine(autobattleCoroutine);
            autoPlayer.StopAutoAttack();
            foreach(EnemyAuto enemyAuto in _activeEnemies) // 활성화 된 적 모두 되돌려 보내기
            {
                enemyAuto.InitValues();
                ReleaseEnemy(enemyAuto);
            }

            int activeCount = _activeEnemies.Count;
            for(int i = 0; i < activeCount; i++)
                ReleaseActiveEnemy();
        }
        Debug.Log($"{ColorPalette.Legend}[AutoManager]{ColorPalette.End} Will start new UpdateAutoBattleRoutine coroutine.");
        autobattleCoroutine = null;
        autobattleCoroutine = StartCoroutine(UpdateAutoBattleRoutine());
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
    private void PoolInit(int initialCapacity = 50)
    {
        TestLogger.Log($"{ColorPalette.Legend}[AutoManager]{ColorPalette.End} PoolInit called with initial capacity: {initialCapacity}");
     
        for (int i = 0; i < initialCapacity; i++)
        {
            GameObject perfab = Instantiate(autoEnemyPrefab, transform);
            EnemyAuto obj = perfab.GetComponentInChildren<EnemyAuto>();
            
            obj.enemyTransform.SetParent(transform); // 풀링 오브젝트를 AutoManager의 자식으로 설정
            obj.enemyTransform.position = autoEnemySpwanTrans.position; // 적이 소환될 위치로 초기화

            obj.autoManager = this; // EnemyAuto에 AutoManager 참조 전달
            obj.dungeonManager = dungeonManager;

            obj.enemyTransform.gameObject.name = $"Enemy_Pooled_{i + 1}"; // 이름 변경
            obj.enemyTransform.gameObject.SetActive(false);
            _pool.Enqueue(obj); 
        }

        TestLogger.Log($"{ColorPalette.Legend}[AutoManager]{ColorPalette.End} End of PoolInit. Pool size: {_pool.Count}");
    }
    public EnemyAuto GetEnemy(EnemyDataSo enemyData)
    {
        EnemyAuto obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            GameObject perfab = Instantiate(autoEnemyPrefab, transform);
            obj = perfab.GetComponentInChildren<EnemyAuto>();
            obj.enemyTransform.SetParent(transform);
        }

        Debug.Log($"{ColorPalette.Cyan}[AutoManager]{ColorPalette.End} GetEnemy called. Enemy {enemyData.name} activated");

        float animeSpeed = 1f;
        if (autoOnlineRewardCycle < playerAttackAnimeCycle)
            animeSpeed = playerAttackAnimeCycle / autoOnlineRewardCycle;
        
        obj.SynchronizeBySo(enemyData, autoEnemyMoveSpeed, animeSpeed, enemyDieDurationTime); // 적 데이터 초기화
        _activeEnemies.Enqueue(obj); // 활성화된 적을 추적

        obj.enemyTransform.gameObject.name = $"Enemy_{enemyData.name}_{_activeEnemies.Count}"; // 이름 변경

        obj.enemyTransform.gameObject.SetActive(true);
        obj.enemyTransform.position = autoEnemySpwanTrans.position;

        return obj;
    }
    public void ReleaseEnemy(EnemyAuto obj)
    {
        if (_pool.Contains(obj)) return;

        obj.enemyTransform.gameObject.name = $"Enemy_Released_{_pool.Count + 1}"; // 이름 변경

        obj.enemyTransform.gameObject.SetActive(false);
        obj.enemyTransform = autoEnemySpwanTrans;

        _pool.Enqueue(obj);
    }
    public void ReleaseActiveEnemy()
    {
        _activeEnemies.Dequeue();
    }

// --------------------- 오토 배경 이동 ---------------------//
    private void UpdateAutoBackgroundSpeed()
    {
        for (int i = 0; i < autoBackgrounds.Length; i++)
        {
            autoBackgrounds[i].SetReady(false);

            if (autoBackgrounds[i] != null && autoBackgrounds[i].Id == i)
            {
                if ( i == 0 )
                {
                    // autoBackgroudnMoveSpeed 는 layer1 부터 시작해서
                    autoBackgrounds[i].SetMoveSpeed(autoEnemyMoveSpeed);
                
                    TestLogger.Log($"{i} AutoBackground move speed {autoEnemyMoveSpeed}");
                }
                else
                {
                    autoBackgrounds[i].SetMoveSpeed(autoEnemyMoveSpeed * autoBackgroundMoveSpeed[i - 1]); 
                    TestLogger.Log($"{i} AutoBackground move speed {autoEnemyMoveSpeed * autoBackgroundMoveSpeed[i - 1]}");   
                }

                autoBackgrounds[i].SetReady(true);
            }
            else
                TestLogger.LogWarning(@$"{ColorPalette.Yellow}[AutoManager]{ColorPalette.End} 
                autoBackgrounds[{i}] - ID : {autoBackgrounds[i].Id} is null. Please check the inspector.");
        }
    }
    private void SetBackgroundPosInit(bool init = false)
    {
        for (int i = 0; i < autoBackgrounds.Count(); i++)
        {
            Bounds bounds = autoBackgrounds[i].spriteRenderer.bounds;

            // 캐싱해 둔 전체 화면 왼쪽 끝 기준으로 스프라이트 크기만큼만 재계산
            float x = cachedBottomLeft.x + bounds.extents.x;
            float y = cachedBottomLeft.y + bounds.extents.y;

            if (i != 0)
            {
                y += cachedWorldOffsetY;
            }

            float offsetx = bounds.size.x;
            Vector3 start = new(x - offsetx, y, 0);
            Vector3 end = new(x + offsetx, y, 0);

            autoBackgrounds[i].InitPos(start, end, offsetx, init);
        }

    }
}
