using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
    public int totalClickReward; // 오프라인 보상으로 지급할 총 클릭 수
    private readonly TimeLog timeLog = new();
    private DateTime loginTime; // 게임 시작 시간
    private string todayDate; // 오늘 날짜
    private readonly int MAX_OFFLINE_HOURS = 3; // 최대 오프라인 시간 제한 (24시간)
    private readonly string SAVE_FILE_NAME = "TimeLog.json"; // 저장 파일 이름

    void Awake()
    {
        // 게임 시작 시간 기록
        loginTime = DateTime.Now;
        todayDate = loginTime.ToString("yyyy-MM-dd");
        LoadTimeLog(); // 게임 시작 시 시간 로그 로드
        Debug.Log("게임 시작 시간: " + todayDate + " : " + loginTime);
        OfflineReward(); // 오프라인 보상 지급
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
                                * playerStatsManager.playerStats.benfitEffect 
                                * playerStatsManager.playerStats.reincarnationBonus;

        // 4. 최종 합산 후 딱 한 번만 int로 변환 (소수점 버그 방지)
        totalClickReward = (int)(defaultReward + playerBonusReward);
    }

    public void GetOfflineReward()
    {
        playerStatsManager.playerStats.coinByClick.Upgrade(false, totalClickReward);
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
        GameManger.instance.SaveData(timeLog, SAVE_FILE_NAME);
    }

    public void LoadTimeLog()
    {
        if(!File.Exists(Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME)))
            GameManger.instance.SaveData(timeLog, SAVE_FILE_NAME); // 파일이 없으면 새로 저장
        GameManger.instance.LoadData(timeLog, SAVE_FILE_NAME);
    }

    // --------------------- 플레이어와 몬스터의 자동 사냥 ----------------------//
}
