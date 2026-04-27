using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UpgradManager upgradManager;
    public string index;
    public int selfBTNId; 
    public int canOpenGroupId;
    // 마우스가 버튼 위로 들어올 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManger.instance.upgradManager.UpgradNodeExplain(index, true);
    }

    // 마우스가 버튼에서 나갈 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        GameManger.instance.upgradManager.UpgradNodeExplain(index, false);
    }

    public void BTNClick()
    {
        upgradManager.UpgradeNode(index , selfBTNId); // 자신의 아이디에 따른 업그래이드 
        upgradManager.OpenUpgradGroup(index, canOpenGroupId); // 자신이 열 수 있는 업그래이드 그룹 열기
    }
}
