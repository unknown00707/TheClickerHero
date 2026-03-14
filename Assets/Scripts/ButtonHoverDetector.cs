using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string index;
    // 마우스가 버튼 위로 들어올 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManger.instance.upgradManager.UpgradNodeExplain(index, true);
        Debug.Log(gameObject.name + " 위에 마우스가 있음!");
    }

    // 마우스가 버튼에서 나갈 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        GameManger.instance.upgradManager.UpgradNodeExplain(index, false);
        Debug.Log(gameObject.name + "에서 마우스가 나감!");
    }
}
