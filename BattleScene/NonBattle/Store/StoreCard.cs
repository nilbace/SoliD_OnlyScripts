using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCard : MonoBehaviour
{
    public int CardCost;
    public TMPro.TMP_Text TMP_CardCost;

    private void OnMouseUpAsButton()
    {
        if (CanBuy())
        {
            Debug.Log("Card purchased for: " + CardCost);
            TrialManager.Inst.UseMoonStoneOverTime(CardCost);
            Destroy(gameObject); // ī�带 ������ �� ����
        }
        else
        {
            Debug.Log($"ī�� ����{CardCost}");
        }
    }

    private bool CanBuy()
    {
        return TrialManager.Inst.MoonStone >= CardCost;
    }
}
