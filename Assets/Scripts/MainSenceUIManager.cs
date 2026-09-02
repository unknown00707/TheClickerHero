using UnityEngine;

public class MainSenceUIManager : MonoBehaviour
{
    public GameObject mainUIObj;
    public GameObject dungeonUIObj;
    public GameObject pauseUIObj;
    public PlayerInput playerInput;
    private bool isPauseUIOpen = false;
    private void Awake()
    {
        OpenMainUI(true); // 초기에는 메인 UI를 활성화하고 던전 UI를 비활성화
        isPauseUIOpen = true; // 초기에는 일시정지 UI를 비활성화
        OpenPauseUI(); // 초기에는 일시정지 UI를 비활성화
    }
    public void OpenMainUI(bool isOpen)
    {
        mainUIObj.SetActive(isOpen);
        dungeonUIObj.SetActive(!isOpen);
    }
    public void OpenPauseUI()
    {
        isPauseUIOpen = !isPauseUIOpen;
        pauseUIObj.SetActive(isPauseUIOpen);
    }
    void OnEnable()
    {
        playerInput.OnPauseAction += OpenPauseUI;
    }
    void OnDisable()
    {
        playerInput.OnPauseAction -= OpenPauseUI;
    }
}
