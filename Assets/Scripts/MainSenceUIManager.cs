using UnityEngine;

public class MainSenceUIManager : MonoBehaviour
{
    public GameObject mainUIObj;
    public GameObject dungeonUIObj;
    

    public void OpenMainUI(bool isOpen)
    {
        mainUIObj.SetActive(isOpen);
        dungeonUIObj.SetActive(!isOpen);
    }

}
