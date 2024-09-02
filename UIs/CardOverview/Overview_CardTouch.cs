using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overview_CardTouch : MonoBehaviour
{
    public CardData ThisCardData;
    public float Offset;
    public Vector3 StartPoz;
    private void Start()
    {
        ThisCardData = GetComponent<CardGO>().thisCardData;
    }

    private void OnMouseDown()
    {
        StartPoz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DragParentNonUI.Inst.OnMouseDown();
    }

    private void OnMouseDrag()
    {
        DragParentNonUI.Inst.OnMouseDrag();
    }

    private void OnMouseUpAsButton()
    {
        var newPoz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(Vector3.Distance(StartPoz, newPoz) < 0.3f)
        {
            Debug.Log("ī�带 ���� ������ ����");
            switch (UI_CardOverView.Inst.OverviewType)
            {
                case E_CardOverviewType.DuplicateCard:
                    Debug.Log($"{ThisCardData} ����");
                    TrialManager.Inst.AddCard(ThisCardData);
                    UI_CardOverView.Inst.ClosePannel();
                    break;

                case E_CardOverviewType.RemoveCard:
                    Debug.Log($"{ThisCardData} ����");
                    TrialManager.Inst.RemoveCard(ThisCardData);
                    UI_CardOverView.Inst.ClosePannel();
                    break;
            }
        }
        else
        {
            Debug.Log("�׳� �巡��");
            DragParentNonUI.Inst.OnMouseUp();
        }
        
    }
}
