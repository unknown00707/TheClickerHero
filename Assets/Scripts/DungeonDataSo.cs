using UnityEngine;
[CreateAssetMenu(fileName = "DungeonData", menuName = "GameData/DungeonDataSo")]
public class DungeonDataSo : ScriptableObject
{
    public string dungeonName; // localizationData csv 에 따른 이름
    public Sprite dungeonSprite; // 던전 이미지
    public int dungeonMainStageID;
}
