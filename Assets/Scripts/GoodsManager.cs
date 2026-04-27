using System;
using System.IO;
using TMPro;
using UnityEngine;
[Serializable]
public class GoodsData
{
    [SerializeField] private int goodsCount;
    public int GoodsCount
    {
        get { return goodsCount; }
        set { goodsCount = Mathf.Max(0, value); }
    }
}

public class GoodsManager : MonoBehaviour
{
    public PlayerStatsManager playerStatsManager;
    public TextMeshProUGUI  GoodsCountTxt;
    public GoodsData goodsData;

    void Awake()
    {
        LoadGoods();
        GoodTXTUpdate();
    }

    public void AddGoods()
    {
        goodsData.GoodsCount += playerStatsManager.playerStats.CoinByClick;
        GoodTXTUpdate();
    }
    public void GoodTXTUpdate()
    {
        GoodsCountTxt.text =  "Coins: " + goodsData.GoodsCount;
    }
    public void SaveGoods()
    {
        string jsonData = JsonUtility.ToJson(goodsData, true);
        string path = Path.Combine(Application.persistentDataPath, "SaveGoodsData.json");
        File.WriteAllText(path, jsonData);
        Debug.Log("저장 완료! 경로: " + path);
    }

    public void LoadGoods()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveGoodsData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonData, goodsData);
            GoodTXTUpdate();
            Debug.Log("불러오기 완료! 경로: " + path);
        }
        else
        {
            Debug.LogWarning("저장된 데이터가 없습니다. 경로: " + path);
        }
    }
}
