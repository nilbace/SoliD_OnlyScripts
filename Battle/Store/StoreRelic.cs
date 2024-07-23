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
            Debug.Log("유물 구매: " + RelicCost);
            GameManager.UserData.UseGold(RelicCost);
            RelicManager.Inst.AddRelic(RelicType);
            Destroy(gameObject); // 카드를 구매한 후 삭제
        }
        else
        {
            Debug.Log($"카드 가격{RelicCost}");
        }
    }

    private bool CanBuy()
    {
        return GameManager.UserData.NowGold >= RelicCost;
    }
}
