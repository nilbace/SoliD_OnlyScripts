using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StoreRelic : MonoBehaviour, IPointerClickHandler
{
    public int RelicCost;
    public E_RelicType RelicType;
    public Image IMG_Relic;
    public TMP_Text TMP_RelicCost;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerPress == IMG_Relic.gameObject)
        {
            OnMouseUpAsButton();
        }
    }

    private void OnMouseUpAsButton()
    {
        if (CanBuy())
        {
            Debug.Log("���� ����: " + RelicCost);
            TrialManager.Inst.UseMoonStoneOverTime(RelicCost);
            TrialManager.Inst.AddRelic(RelicType);
            Destroy(gameObject); // ������ ������ �� ����
        }
        else
        {
            Debug.Log($"���� ����: {RelicCost}");
        }
    }

    private bool CanBuy()
    {
        return TrialManager.Inst.MoonStone >= RelicCost;
    }
}
