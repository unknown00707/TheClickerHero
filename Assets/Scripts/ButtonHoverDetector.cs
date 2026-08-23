using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
public enum TooltipDataType
{
    UpragedManager,      
    DungeonManager,     
    PlayerStatsManager,    
}

public class ButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TooltipDataType tooltipDataType;
    public string id;
    [SerializeField] private Vector2 tooltipPivot = new(0.5f, 0.5f);

    private readonly float holdTime = 0.5f;
    private Coroutine holdCoroutine;
    private bool isHolding = false;
    // 마우스가 버튼 위로 들어올 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        // 홀드 체크 코루틴 시작
        holdCoroutine = StartCoroutine(CheckHoldTime());
    }

    // 마우스가 버튼에서 나갈 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        // 이미 툴팁이 켜져 있었다면 끔
        if (isHolding)
        {
            isHolding = false;
            TooltipManager.Hide();
        }
    }

    private IEnumerator CheckHoldTime()
    {
        // 사용자가 설정한 holdTime(예: 0.5초)만큼 기다림
        yield return new WaitForSeconds(holdTime);

        // 중간에 마우스가 나가지 않고 여기까지 왔다면 홀드 성공!
        isHolding = true;
        TooltipManager.GetToolTipID(tooltipDataType, id, tooltipPivot);
    }
}
