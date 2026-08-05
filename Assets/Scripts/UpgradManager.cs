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
    public List<NodeInfo> nodeInfos = new();
    public List<bool> upgradGroupUnlockStatus = new(); // 업그레이드 그룹 잠금 해제 상태 리스트
}
[Serializable]
public struct NodeInfo
{
    public string nodeID;
    public int level;
    public bool isUnLock;
    public bool isMaxLevel;
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

public enum StatType { ATK, DEF, HP, CRITCAL_CHANCE, CRITCAL_DAMAGE, CLICK_COUNT, }
public enum CalcType { Add, Multiply }

public class UpgradManager : MonoBehaviour
{
    [Header("Managers")]
    public GoodsManager goodsManager;
    public PlayerStatsManager playerStatsManager;
    [Header("UI References")]
    public Canvas canvas;
    public RectTransform canvasRectTransform;
    public List<GameObject> upgradGroupObjects = new();
    public RectTransform contantTransform;
    public ScrollRect scrollRect;
    public GameObject upgradExplainObject;
    public RectTransform upgradExplainRectTransform;
    public TextMeshProUGUI[] upgradExplainTexts;

    [Header("Settings")]
    // 업그레이드 설명창이 마우스를 따라다니도록 하기 위한 오프셋입니다. 필요에 따라 조정하세요.
    public float expainOffsetX = -370f;
    public float expainOffsetY = 100f;
    // 업그레이드 데이터와 설명 데이터를 관리하는 변수입니다.
    // 세이브 데이터를 하나로 합쳤습니다.
    private readonly SaveData upradeSaveData = new(); 
    private readonly string UPGRAD_SAVE_FILE_NAME = "SaveUpgradData.json";
    private readonly Dictionary<string, NodeInfo> upgradNodeInfoDictionary = new(); // 노드 ID로 노드 정보 조회 딕셔너리
    // 설명 데이터(CSV) 딕셔너리
    private readonly Dictionary<string, Dictionary<int, ExplainData>> upgradExplainDictionary = new();
    // string : 노드의 ID, int : 노드의 레벨, ExpainData : 설명 데이터
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
        LoadUpgradValueData();
        LoadUpgradData();
        OpenAutoUpgradGroup();
    }
    // 게임 시작 시 세이브 데이터에 따라 자동으로 업그레이드 그룹을 여는 함수입니다.
    private void OpenAutoUpgradGroup()
    {
        for(int i = 0; i < upradeSaveData.upgradGroupUnlockStatus.Count; i++)
        {
            if(upradeSaveData.upgradGroupUnlockStatus[i])
                upgradGroupObjects[i].SetActive(true);
        }
        UpdateContentSize();
    }
    //By button
    public void OpenUpgradGroup(string nodeID , int index)
    {
        if(upgradNodeInfoDictionary[nodeID].isUnLock && upgradGroupObjects[index].activeInHierarchy == false)
        {
            upgradGroupObjects[index].SetActive(true);
            upradeSaveData.upgradGroupUnlockStatus[index] = true; // 세이브 데이터에도 잠금 해제 상태 저장
            UpdateContentSize(); 
        }
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
        upgradNodeInfoDictionary.TryGetValue(index, out NodeInfo nodeInfo);
        int level = nodeInfo.level;

        // 설명 텍스트 설정
        string nodePath;
        if(upgradExplainDictionary.ContainsKey(index) && upgradExplainDictionary[index].ContainsKey(level))
            nodePath = upgradExplainDictionary[index][level].upgradExplainID;
        else
        {
            nodePath = "DESC_END_UPGRADE"; // 업그레이드 데이터가 없는 경우
            level = 0; // 설명이 없는 경우 레벨은 0으로 초기화
        }
        
        upgradExplainTexts[0].text = LanguageManager.Instance.GetText(nodePath + "_TITLE");
        upgradExplainTexts[1].text = string.Format(LanguageManager.Instance.GetText(nodePath), upgradExplainDictionary[index][level].values.ToString());
        upgradExplainTexts[2].text = LanguageManager.Instance.GetText("DESC_COST") + upgradExplainDictionary[index][level].needGoods.ToString();
        if(nodePath == "DESC_END_UPGRADE")
            upgradExplainTexts[2].text = "!^ o ^!"; // 업그레이드 데이터가 없는 경우 비용 텍스트 숨기기
        
        // 위치
        Vector2 mousePos = Mouse.current.position.ReadValue();
        // 2. 스크린 좌표를 캔버스 상의 로컬 좌표로 변환합니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera, // Overlay 모드라면 null을 넣어도 됩니다.
            out Vector2 localPoint
        );
    
        upgradExplainRectTransform.anchoredPosition = localPoint + new Vector2(expainOffsetX, expainOffsetY);
        if(upgradExplainRectTransform.anchoredPosition.x > canvasRectTransform.sizeDelta.x)
            upgradExplainRectTransform.anchoredPosition = localPoint + new Vector2(-expainOffsetX, expainOffsetY);
        upgradExplainObject.SetActive(isEnter);
    }
    public void UpgradeNode(string index) // index = 노드 ID
    {
        upgradNodeInfoDictionary.TryGetValue(index, out NodeInfo nodeInfo);
        int level = nodeInfo.level;

        if (upgradExplainDictionary.ContainsKey(index) && upgradExplainDictionary[index].ContainsKey(level))
        {
            // 업그레이드 가능 여부 체크
            if (goodsManager.goodsData.GoodsCount >= upgradExplainDictionary[index][level].needGoods)
            {
                // 비용 차감
                goodsManager.goodsData.GoodsCount -= upgradExplainDictionary[index][level].needGoods;
                goodsManager.GoodTXTUpdate();

                // 레벨 업
                if (nodeInfo.isUnLock == false) nodeInfo.isUnLock = true; // 노드가 잠겨있다면 잠금 해제
                nodeInfo.level += 1;

                // 스탯 적용
                ExplainData explainData = upgradExplainDictionary[index][level];
                switch (explainData.statTypes)
                {
                    case StatType.ATK:
                    var (isPercentage_ATK, amount_ATK) = CalculateStatType(explainData.calcTypes, explainData.values);
                        playerStatsManager.playerStats.attackPower.Upgrade(isPercentage_ATK, amount_ATK);
                        break;
                    case StatType.DEF:
                        var (isPercentage_DEF, amount_DEF) = CalculateStatType(explainData.calcTypes, explainData.values);
                        playerStatsManager.playerStats.defense.Upgrade(isPercentage_DEF, amount_DEF);
                        break;
                    case StatType.HP:
                        var (isPercentage_HP, amount_HP) = CalculateStatType(explainData.calcTypes, explainData.values);
                        playerStatsManager.playerStats.health.Upgrade(isPercentage_HP, amount_HP);
                        break;
                    case StatType.CRITCAL_CHANCE:
                        playerStatsManager.playerStats.criticalChance.Upgrade(false, explainData.values);
                        break;
                    case StatType.CRITCAL_DAMAGE:
                        var (isPercentage_CRITCAL_DAMAGE, amount_CRITCAL_DAMAGE) = CalculateStatType(explainData.calcTypes, explainData.values);
                        playerStatsManager.playerStats.criticalDamage.Upgrade(isPercentage_CRITCAL_DAMAGE, amount_CRITCAL_DAMAGE);
                        break;
                    case StatType.CLICK_COUNT:
                        var (isPercentage_CLICK_COUNT, amount_CLICK_COUNT) = CalculateStatType(explainData.calcTypes, explainData.values);
                        playerStatsManager.playerStats.coinByClick.Upgrade(isPercentage_CLICK_COUNT, (int)amount_CLICK_COUNT);
                        break;
                }

                //UI 업데이트 
                playerStatsManager.UpDatePlayerStatsText(); // 플레이어 스탯 텍스트 업데이트
                GameManger.instance.SaveGame(); // 변경된 데이터 저장
                // 해당 노드가 최대 레벨인지 체크
                if (!upgradNodeInfoDictionary[index].isMaxLevel && upgradExplainDictionary.ContainsKey(index) && !upgradExplainDictionary[index].ContainsKey(level + 1))
                {
                    nodeInfo.isMaxLevel = true;
                    // UI에서 최대 레벨 도달 표시 (예시에서는 Debug.Log로 대체)
                    Debug.Log($"노드 {index}가 최대 레벨에 도달했습니다!");
                }
                else
                {
                    Debug.Log($"노드 {index}가 레벨 {level + 1}로 업그레이드되었습니다!");
                }

                // 노드 정보 업데이트
                upgradNodeInfoDictionary[index] = nodeInfo; 
                
                // 게임 데이터 저장
                GameManger.instance.SaveGame();
            }
            else
            {
                // UI로 코인 부족 메시지 띄우기 (예시에서는 Debug.Log로 대체)
                Debug.Log("코인이 부족합니다!");
            }
        }
        else
        {
            Debug.LogError($"업그레이드 데이터가 없습니다! 노드 ID: {index}, 레벨: {level}");
        }
    }
    private (bool isPercentage, float amount) CalculateStatType(CalcType calcType, float value)
    {
        if (calcType == CalcType.Add) return (false, 0);
        if (calcType == CalcType.Multiply) return (true, value);

        return (false, 0); // 기본값 반환
    }
// ------------------- 세이브 & CSV 로드 ------------------ //
    // 🌟 세이브 로직 수정본 (하나의 파일로 깔끔하게)
    public void SaveUpgradData()
    {
        upradeSaveData.nodeInfos.Clear();
        foreach (var kvp in upgradNodeInfoDictionary.Values)        
        {
            upradeSaveData.nodeInfos.Add(kvp); // 딕셔너리의 노드 정보를 리스트에 추가
        }
        GameManger.instance.SaveData(upradeSaveData, UPGRAD_SAVE_FILE_NAME);
    }

    public void LoadUpgradData()
    {
        if(File.Exists(Path.Combine(Application.persistentDataPath, UPGRAD_SAVE_FILE_NAME)))
        {
            GameManger.instance.LoadData(upradeSaveData, UPGRAD_SAVE_FILE_NAME);
            // 세이브 데이터에서 딕셔너리로 변환
            foreach (var nodeInfo in upradeSaveData.nodeInfos)
            {
                upgradNodeInfoDictionary[nodeInfo.nodeID] = nodeInfo; // 노드 ID로 노드 정보 업데이트
            }
        }
        else
        {
            // 초기 데이터 설정
            foreach (string nodeID in upgradExplainDictionary.Keys)
            {
                bool index = false;
                if (upgradNodeInfoDictionary.ContainsKey(nodeID)) continue; // 이미 초기화된 노드는 건너뜁니다.
                if (nodeID == "FIRST_UPGRADE") index = true;

                upgradNodeInfoDictionary[nodeID] = new NodeInfo
                {
                    nodeID = nodeID,
                    level = 0,
                    isUnLock = index,
                    isMaxLevel = false
                };
                upradeSaveData.nodeInfos.Add(upgradNodeInfoDictionary[nodeID]);
            }
            
            upradeSaveData.upgradGroupUnlockStatus = new List<bool>(new bool[upgradGroupObjects.Count])
            {
                [0] = true // 첫 번째 그룹은 기본적으로 열려있도록 설정
            };

            GameManger.instance.SaveData(upradeSaveData, UPGRAD_SAVE_FILE_NAME); // 초기 데이터 저장
            GameManger.instance.LoadData(upradeSaveData, UPGRAD_SAVE_FILE_NAME); // 저장된 초기 데이터 로드
        }
    }
    // 🌟 CSV 읽기 수정본 (인덱스 수정 완료)
    public void LoadUpgradValueData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("UpgradeValue");
        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

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
                statTypes = Enum.Parse<StatType>(row[2]),
                calcTypes = Enum.Parse<CalcType>(row[3]),
                values = float.Parse(row[4]),
                needGoods = int.Parse(row[5]),
                upgradExplainID = row[6]
            };
        }
    }
}
