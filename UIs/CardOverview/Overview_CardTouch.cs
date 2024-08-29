using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overview_CardTouch : MonoBehaviour
{
    public CardData ThisCardData;
    private void Start()
    {
        ThisCardData = GetComponent<CardGO>().thisCardData;
    }
    private void OnMouseUpAsButton()
    {
        Debug.Log("hi");
        switch (UI_CardOverView.Inst.OverviewType)
        {
            case E_CardOverviewType.DuplicateCard:
                Debug.Log($"{ThisCardData} 복제");
                TrialManager.Inst.AddCard(ThisCardData);
                UI_CardOverView.Inst.ClosePannel();
                break;

            case E_CardOverviewType.RemoveCard:
                Debug.Log($"{ThisCardData} 삭제");
                TrialManager.Inst.RemoveCard(ThisCardData);
                UI_CardOverView.Inst.ClosePannel();
                break;
        }
    }
}
