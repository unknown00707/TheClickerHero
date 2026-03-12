using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "UpgradData", menuName = "ScriptableObjects/UpgradData")]
public class UpgradData : ScriptableObject
{
    public List<bool> upgradUnlockStatus = new()
    {
        true, false, false, false, false, false, false, false, false, false
    };

    public string[] upgradExplain = new string[]
    {
        "ad/asd",
        "ad/asd",
        "ad/asd",
        "ad/asd",
        "ad/asd",
    };
}
public class UpgradManager : MonoBehaviour
{
    public Canvas canvas;
    public UpgradData upgradData;
    public List<GameObject> upgradGroupObjects = new();
    public RectTransform contantTransform;
    public ScrollRect scrollRect;
    public GameObject upgradExplainObject;
    public TextMeshProUGUI[] upgradExplainTexts;

    public float expainOffsetX = -370f;
    public float expainOffsetY = 100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }

    public void OpenUpgradGroup(int index)
    {
        upgradGroupObjects[index].SetActive(true);
        upgradData.upgradUnlockStatus[index] = true;
        foreach (var data in upgradData.upgradUnlockStatus)
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

    public void UpgradNodeExplain(int index, bool isEnter)
    {
        string[] nodeInfo = upgradData.upgradExplain[index].Split('/');
        print(nodeInfo[0]);
        print(nodeInfo[1]);
        string nodeName = nodeInfo[0];
        string nodeExplain = nodeInfo[1];
        upgradExplainTexts[0].text = nodeName;
        upgradExplainTexts[1].text = nodeExplain;

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
    public void SaveGoods()
    {
        string jsonData = JsonUtility.ToJson(upgradData, true);
        string path = Path.Combine(Application.persistentDataPath, "SaveUpgradData.json");
        File.WriteAllText(path, jsonData);
        Debug.Log("저장 완료! 경로: " + path);
    }

    public void LoadGoods()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveUpgradData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonData, upgradData);
            Debug.Log("불러오기 완료! 경로: " + path);
        }
        else
        {
            Debug.LogWarning("저장된 데이터가 없습니다. 경로: " + path);
        }
    }
}
