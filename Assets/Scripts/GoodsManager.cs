using System.IO;
using TMPro;
using UnityEngine;
[CreateAssetMenu(fileName = "GoodsData", menuName = "ScriptableObjects/GoodsData")]
public class GoodsData : ScriptableObject
{
    public int GoodsCount;
}

public class GoodsManager : MonoBehaviour
{
    public TextMeshProUGUI  GoodsCountTxt;
    public GoodsData goodsData;

    void Awake()
    {
        GoodsCountTxt.text =  "Coins: " + goodsData.GoodsCount;
    }

    public void AddGoods()
    {
        goodsData.GoodsCount++;
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
            GoodsCountTxt.text =  "Coins: " + goodsData.GoodsCount;
            Debug.Log("불러오기 완료! 경로: " + path);
        }
        else
        {
            Debug.LogWarning("저장된 데이터가 없습니다. 경로: " + path);
        }
    }
}
