using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCard : MonoBehaviour
{
    public int CardCost;

    private void OnMouseUpAsButton()
    {
        if (CanBuy())
        {
            Debug.Log("Card purchased for: " + CardCost);
            GameManager.UserData.UseMoonStone(CardCost);
            Destroy(gameObject); // ī�带 ������ �� ����
        }
        else
        {
            Debug.Log($"ī�� ����{CardCost}");
        }
    }

    private bool CanBuy()
    {
        return GameManager.UserData.MoonStoneAmount >= CardCost;
    }
}
