using UnityEngine;

public class ClickBTN : MonoBehaviour
{
    public GoodsManager goodsManager;
    public void ClickBTNClick()
    {
        goodsManager.AddGoods();
        Debug.Log("ClickBTN");
    }
}
