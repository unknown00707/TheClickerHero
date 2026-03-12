using UnityEngine;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;
    public UpgradManager upgradManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
