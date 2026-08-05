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
    public GoodsData goodsData = new();
    private readonly string fileName = "SaveGoodsData.json";

    void Start()
    {
        LoadGoods();
        GoodTXTUpdate();
    }

    public void AddGoods()
    {
        goodsData.GoodsCount += (int)playerStatsManager.playerStats.CoinByClick;
        GoodTXTUpdate();
    }
    public void GoodTXTUpdate()
    {
        GoodsCountTxt.text =  "Coins: " + goodsData.GoodsCount;
    }
    public void SaveGoods() => GameManger.instance.SaveData(goodsData, fileName);

    public void LoadGoods()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
            SaveGoods(); // 파일이 없으면 새로 생성
            
        GameManger.instance.LoadData(goodsData, fileName);
        GoodTXTUpdate();
    }
}
