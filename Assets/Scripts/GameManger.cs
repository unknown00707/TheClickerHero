using System.IO;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;
    public UpgradManager upgradManager;
    public PlayerSkinManager playerSkinManager;
    public PlayerStatsManager playerStatsManager;
    public GoodsManager goodsManager;
    public WeaponManager weaponManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    public void SaveGame()
    {
        upgradManager.SaveUpgradData();
        playerSkinManager.SaveSkins();
        playerStatsManager.SavePlayerStats();
        goodsManager.SaveGoods();
        weaponManager.SaveWeaponData();
    }

    public void SaveData<T>(T data, string fileName)
    {
        string jsonData = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, jsonData);
        Debug.Log($"저장 완료! 경로: {path}");
    }

    public void LoadData<T>(T data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonData, data);
            Debug.Log($"불러오기 완료! 경로: {path}");
        }
        else
        {
            Debug.LogWarning($"저장된 데이터가 없습니다. 경로: {path}");
        }
    }
}
