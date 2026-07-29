using UnityEngine;

public class MainSenceUIManager : MonoBehaviour
{
    public GameObject mainUIObj;
    public GameObject dungeonUIObj;
    
    private void Awake()
    {
        OpenMainUI(true); // 초기에는 메인 UI를 활성화하고 던전 UI를 비활성화
    }
    public void OpenMainUI(bool isOpen)
    {
        mainUIObj.SetActive(isOpen);
        dungeonUIObj.SetActive(!isOpen);
    }

}
