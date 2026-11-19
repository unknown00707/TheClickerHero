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
    private readonly GoodsData goodsData = new();
    private readonly string fileName = "SaveGoodsData.json";

    public int GOODS => goodsData.GoodsCount;

    void Start()
    {
        LoadGoods();
        GoodTXTUpdate();
    }

    public void AddGoods()
    {
        goodsData.GoodsCount += (int)playerStatsManager.playerStats.Click;
        GoodTXTUpdate();
    }

    public void AddClickAndCoin(int click, int coin)
    {
        int totalAddClick = click + coin * playerStatsManager.playerStats.coinMulitplier;
        goodsData.GoodsCount += totalAddClick;
        GoodTXTUpdate();

        GameManger.instance.SaveGame();
    }

    public bool SpendClick(int amount)
    {
        if (amount > goodsData.GoodsCount)
            return false;
        else
        {
            Debug.Log($"{ColorPalette.Legend}{name}{ColorPalette.End} spend : {amount}");
            goodsData.GoodsCount -= amount;
            return true;
        }
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
