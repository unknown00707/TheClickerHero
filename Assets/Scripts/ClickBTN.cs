using UnityEngine;

public class ClickBTN : MonoBehaviour
{
    public GoodsManager goodsManager;
    public GameObject[] tapObjects;
    public GameObject[] tapLeftObjects;
    public void ClickBTNClick()
    {
        goodsManager.AddGoods();
    }

    public void OpenTapByIndex(int index)
    {
        LoopOpenTapByIndex(tapObjects, index);
    }

    public void OpenTapLeftByIndex(int index)
    {
        LoopOpenTapByIndex(tapLeftObjects, index);
    }
    void LoopOpenTapByIndex(GameObject[] tapArray, int index)
    {
        for (int i = 0; i < tapArray.Length; i++)
        {
            if (i == index)
            {
                tapArray[i].SetActive(true);
            }
            else
            {
                tapArray[i].SetActive(false);
            }
        }
    }
}
