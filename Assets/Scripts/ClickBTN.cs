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
    public void OpenSpecialTap(int index)
    {
        for (int i = tapObjects.Length -1; i >= tapObjects.Length - 2; i--)
        {
            if (index == -1) // index가 -1이면 모든 탭을 닫음
            {
                tapObjects[i].SetActive(false);
            }
            else if (i == index)
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
