using System;
using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect; 
    public UpgradManager upgradManager;
    public DungeonManager dungeonManager;
    public PlayerSkinManager playerSkinManager;

    [SerializeField] private GameObject tooltipWindow;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private RectTransform tooltiprectTransform;
    
    private static TooltipManager instance;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            Hide();
        }
    }
    public static void GetToolTipID(TooltipDataType tooltipDataType, string id, Vector2 customPivot)
    {
        if(LanguageManager.Instance == null)
        {
            instance.Show("옵빠이", @"어이쿠 손이 미끄러졌네?
            도대체 이런 두꺼운 귀두랑 듬직한 모습을 한 굉장한 자지를 무슨 수로..!
            그냥 보기만 했는 데, 자궁이 패배선언 해버렸어!
            May the Force be with you.
            Carpe diem. Seize the day, boys. Make your lives extraordinary.
            To infinity and beyond!", customPivot);
            return;
        }
        string[] txt = tooltipDataType switch
        {
            TooltipDataType.UpragedManager => instance.upgradManager.UpgradNodeExplain(id),
            TooltipDataType.DungeonManager => instance.dungeonManager.ToTooltipByGetID(id),
            _ => null // default를 의미합니다.
        };

        if (txt != null && txt.Length >= 2)
            instance.Show(txt[0], txt[1], customPivot);
        else
            Debug.LogError($"툴팁 데이터 누락 또는 배열 크기 오류! Type: {tooltipDataType}");
    }
    private void Show(string title, string content, Vector2 customPivot)
    {
        string formattedContent = content
        .Replace("<br>", "\n")
        .Replace("\\n", "\n");

        titleText.text = title;
        contentText.text = formattedContent;

        Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        // 3. Overlay 모드이므로 카메라는 null(마지막 인자)로 넣어서 로컬 좌표로 변환합니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            mouseScreenPos, 
            null, 
            out Vector2 targetLocalPoint
        );

        tooltiprectTransform.pivot = customPivot;

        // 5. 최종 위치 대입
        tooltiprectTransform.anchoredPosition = targetLocalPoint;

        tooltipWindow.SetActive(true);
    }

    public static void Hide()
    {
        if (instance != null)
        {
            instance.tooltipWindow.SetActive(false);
        }
    }
}
