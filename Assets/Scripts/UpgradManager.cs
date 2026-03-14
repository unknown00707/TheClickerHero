using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 💡 팁: 세이브 데이터는 SO보다 일반 [Serializable] 클래스가 JSON 저장에 훨씬 유리합니다!
[Serializable]
public class SaveData 
{
    public List<bool> upgradUnlockStatus = new() { true, false, false, false, false, false, false, false, false, false };
    
    // 딕셔너리 대신 구조체 리스트를 써야 JSON 저장이 완벽하게 됩니다.
    public List<NodeLevel> nodeLevels = new(); 
}

[Serializable]
public struct NodeLevel
{
    public string nodeID;
    public int level;
}

// 오타 수정: Expain -> Explain
public class ExplainData 
{
    public StatType statTypes;
    public CalcType calcTypes;
    public float values;
    public int needGoods;
    public string upgradExplainID;
}

public enum StatType { ATK, DEF, HP, CLICK_COUNT, CLICK_DMG }
public enum CalcType { Add, Multiply }

public class UpgradManager : MonoBehaviour
{
    public Canvas canvas;
    public List<GameObject> upgradGroupObjects = new();
    public RectTransform contantTransform;
    public ScrollRect scrollRect;
    public GameObject upgradExplainObject;
    public TextMeshProUGUI[] upgradExplainTexts;

    // 업그레이드 설명창이 마우스를 따라다니도록 하기 위한 오프셋입니다. 필요에 따라 조정하세요.
    public float expainOffsetX = -370f;
    public float expainOffsetY = 100f;
    // 업그레이드 데이터와 설명 데이터를 관리하는 변수입니다.
    // 세이브 데이터를 하나로 합쳤습니다.
    private SaveData mySaveData = new SaveData(); 

    // 설명 데이터(CSV) 딕셔너리
    private Dictionary<string, Dictionary<int, ExplainData>> upgradExplainDictionary = new();
    // string : 노드의 ID, int : 노드의 레벨, ExpainData : 설명 데이터
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }

    public void OpenUpgradGroup(int index)
    {
        upgradGroupObjects[index].SetActive(true);
        mySaveData.upgradUnlockStatus[index] = true;
        foreach (var data in mySaveData.upgradUnlockStatus)
        {
            Debug.Log(data);
        }
        UpdateContentSize();
    }
    public void UpdateContentSize()
    {
        if (upgradGroupObjects == null || upgradGroupObjects.Count == 0) return;

        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float maxX = 0;

        foreach (var obj in upgradGroupObjects)
        {
            if (obj.activeSelf)
            {
                RectTransform rect = obj.GetComponent<RectTransform>(); // 루프 밖에서 미리 캐싱하면 더 좋습니다.
                Vector2 pos = rect.anchoredPosition;
                Vector2 size = rect.sizeDelta;
                float pivotY = rect.pivot.y;

                // 각 노드의 실제 끝단 위치 계산
                float topEdge = pos.y + (size.y * (1 - pivotY));
                float bottomEdge = pos.y - (size.y * pivotY);
                float rightEdge = pos.x + size.x;

                if (topEdge > maxY) maxY = topEdge;
                if (bottomEdge < minY) minY = bottomEdge;
                if (rightEdge > maxX) maxX = rightEdge;
            }
        }

        // 여유 공간(Padding)을 약간 더해주는 것이 보기에 좋습니다.
        float padding = 20f;
        contantTransform.sizeDelta = new Vector2(maxX + padding, maxY - minY + padding);
    }

    public void UpgradNodeExplain(string index, bool isEnter)
    {
        int level = mySaveData.nodeLevels.Find(x => x.nodeID == index).level; // 노드 ID로 레벨 찾기
        // 설명 텍스트 설정
        string nodePath = upgradExplainDictionary[index][level].upgradExplainID;

        upgradExplainTexts[0].text = LanguageManager.Instance.GetText(nodePath + "_TITLE");
        upgradExplainTexts[1].text = string.Format(LanguageManager.Instance.GetText(nodePath), level);
        upgradExplainTexts[2].text = upgradExplainDictionary[index][level].needGoods.ToString();

        // 위치
        Vector2 mousePos = Mouse.current.position.ReadValue();
        // 2. 스크린 좌표를 캔버스 상의 로컬 좌표로 변환합니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera, // Overlay 모드라면 null을 넣어도 됩니다.
            out Vector2 localPoint
        );
        upgradExplainObject.GetComponent<RectTransform>().anchoredPosition = localPoint + new Vector2(expainOffsetX, expainOffsetY);
        if(upgradExplainObject.GetComponent<RectTransform>().anchoredPosition.x > canvas.GetComponent<RectTransform>().sizeDelta.x)
            upgradExplainObject.GetComponent<RectTransform>().anchoredPosition = localPoint + new Vector2(-expainOffsetX, expainOffsetY);
        upgradExplainObject.SetActive(isEnter);
    }
    // 🌟 세이브 로직 수정본 (하나의 파일로 깔끔하게)
    public void SaveUpgradData()
    {
        string jsonData = JsonUtility.ToJson(mySaveData, true);
        string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
        File.WriteAllText(path, jsonData);
        Debug.Log("저장 완료!");
    }

    public void LoadUpgradData()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonData, mySaveData);
            Debug.Log("불러오기 완료! 경로: " + path);
        }
        else
        {
            Debug.LogWarning("저장된 데이터가 없습니다. 경로: " + path);
        }
    }

    // 🌟 CSV 읽기 수정본 (인덱스 수정 완료)
    public void LoadUpgradValueData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("UpgradValue");
        if (csvData == null) return;

        string[] lines = csvData.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] row = lines[i].Trim().Split(',');

            string id = row[0];
            int level = int.Parse(row[1]);

            if (!upgradExplainDictionary.ContainsKey(id))
                upgradExplainDictionary[id] = new Dictionary<int, ExplainData>();

            upgradExplainDictionary[id][level] = new ExplainData
            {
                statTypes = (StatType)Enum.Parse(typeof(StatType), row[2]), // 2번부터 시작!
                calcTypes = (CalcType)Enum.Parse(typeof(CalcType), row[3]),
                values = float.Parse(row[4]),
                needGoods = int.Parse(row[5]),
                upgradExplainID = row[6]
            };
        }
    }
}
