using UnityEngine;
using System;
using System.Collections.Generic;

public enum Language { KR, EN, JP, CN }

public class LanguageManager : MonoBehaviour
{
    // 2. 싱글톤 패턴: 게임 전체에서 매니저를 쉽게 부르기 위해 사용합니다.
    public static LanguageManager Instance;

    public Language currentLanguage = Language.KR; // 현재 설정된 언어

    // 3. 언어가 바뀌었을 때 UI들에게 "새로고침해!" 라고 알리는 방송국 역할(Event)입니다.
    public event Action OnLanguageChanged;

    // 4. 번역 데이터를 저장할 핵심 사전(Dictionary)입니다. 
    // 구조: [단어 ID, [언어, 실제 텍스트]]
    private Dictionary<string, Dictionary<Language, string>> langTable = new();

    void Awake()
    {
        // 싱글톤 기본 세팅: 매니저가 두 개 생기는 것을 방지하고, 씬이 넘어가도 파괴되지 않게 합니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCSVData(); // 게임이 켜지자마자 CSV를 읽어옵니다.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 5. Resources 폴더에 있는 CSV 파일을 읽어와서 사전에 저장하는 함수
    private void LoadCSVData()
    {
        // Resources 폴더 안의 "LocalizationData" 파일을 TextAsset으로 불러옵니다.
        TextAsset csvData = Resources.Load<TextAsset>("LocalizationData");
        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        // 엔터(\n)를 기준으로 줄을 나눕니다.
        string[] lines = csvData.text.Split('\n');
        
        // 첫 번째 줄(ID, KR, EN)은 제목이므로 1번 줄(두 번째 줄)부터 읽습니다.
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue; // 빈 줄은 무시

            // 쉼표(,)를 기준으로 단어를 쪼갭니다.
            string[] row = lines[i].Trim().Split(',');

            // row[0]은 ID, row[1]은 KR, row[2]는 EN입니다.
            string id = row[0];
            
            // 해당 ID의 방을 새로 만듭니다.
            langTable[id] = new Dictionary<Language, string>();
            langTable[id][Language.KR] = row[1];
            langTable[id][Language.EN] = row[2];
        }
        Debug.Log("다국어 데이터 로드 완료!");
    }

    // 6. 외부(UI)에서 ID를 주면, 현재 언어에 맞는 텍스트를 꺼내주는 함수
    public string GetText(string id)
    {
        // 사전에 ID가 존재하고, 해당 언어의 번역이 있다면?
        if (langTable.ContainsKey(id) && langTable[id].ContainsKey(currentLanguage))
        {
            return langTable[id][currentLanguage];
        }
        // 오타 등으로 못 찾았을 때 에러 대신 ID 자체를 띄워 개발자가 알기 쉽게 합니다.
        return $"[{id}]"; 
    }

    // 7. 언어 변경 버튼을 눌렀을 때 실행될 함수
    public void ChangeLanguage(int id)
    {
        // id가 0이면 KR, 1이면 EN으로 바꿔주는 간단한 로직입니다.
        currentLanguage = (Language)id;
        
        // 방송국에서 알림을 쏩니다! "언어 바뀌었으니 모두 새로고침 하세요!"
        OnLanguageChanged?.Invoke(); 
    }
}