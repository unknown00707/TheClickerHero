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
    private readonly string fileName = "SaveGoodsData.json";

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
    public void SaveGoods() => GameManger.instance.SaveData(goodsData, fileName);

    public void LoadGoods()
    {
        GameManger.instance.LoadData(goodsData, fileName);
        GoodTXTUpdate();
    }
}
