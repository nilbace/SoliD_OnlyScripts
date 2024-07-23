using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreRelic : MonoBehaviour
{
    public int RelicCost;
    public E_RelicType RelicType;

    private void OnMouseUpAsButton()
    {
        if (CanBuy())
        {
            Debug.Log("���� ����: " + RelicCost);
            GameManager.UserData.UseGold(RelicCost);
            RelicManager.Inst.AddRelic(RelicType);
            Destroy(gameObject); // ī�带 ������ �� ����
        }
        else
        {
            Debug.Log($"ī�� ����{RelicCost}");
        }
    }

    private bool CanBuy()
    {
        return GameManager.UserData.NowGold >= RelicCost;
    }
}
