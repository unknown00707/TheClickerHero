using UnityEngine;

public class ClickBTN : MonoBehaviour
{
    public GoodsManager goodsManager;
    public GameObject[] tapObjects;
    public void ClickBTNClick()
    {
        goodsManager.AddGoods();
    }

    public void OpenTapByIndex(int index)
    {
        for (int i = 0; i < tapObjects.Length; i++)
        {
            if (i == index)
            {
                tapObjects[i].SetActive(true);
            }
            else
            {
                tapObjects[i].SetActive(false);
            }
        }
    }
}
