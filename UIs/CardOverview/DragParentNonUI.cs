using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragParentNonUI : MonoBehaviour
{
    private float minY = 0f;  // y�� �ּ� ����
    private float maxY = 0.77f;   // y�� �ִ� ����
    private Vector3 offset;
    private Vector3 startParentPosition;

    void OnMouseDown()
    {
        // �θ� ������Ʈ�� ���� ��ġ�� �����մϴ�.
        startParentPosition = transform.parent.position;
        // ���콺�� ������Ʈ ������ �������� ����մϴ�.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = startParentPosition - mousePosition;
    }

    void OnMouseDrag()
    {
        transform.parent.DOKill();
        // ���� ���콺 ��ġ�� �����ɴϴ�.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;

        // �θ� ������Ʈ�� y�� ��ġ�� ������Ʈ�մϴ�.
        transform.parent.position = new Vector3(startParentPosition.x, mousePosition.y, startParentPosition.z);
    }

    void OnMouseUp()
    {
        // �巡�װ� ������ �θ� ������Ʈ�� y�� ��ġ�� Ȯ���ϰ�, ������ ��� ��� ���� ������ �̵�
        float currentY = transform.parent.position.y;

        // ���� ������ ��� ���
        if (currentY < minY || currentY > maxY)
        {
            // y�� ��ġ�� ���ѵ� ��ġ�� �̵�
            float targetY = Mathf.Clamp(currentY, minY, maxY);
            transform.parent.DOMoveY(targetY, 0.5f).SetEase(Ease.InOutQuad);
        }
    }

    private void OnEnable()
    {
        if (UI_CardOverView.Inst == null) return;
        var temp = (UI_CardOverView.Inst.NowCardCount-1) / 5;
        temp++;
        if (temp == 1) { maxY = 0; return; }
        if (temp >= 3) maxY = 0.77f + (temp-2) * 5.32f;
        else maxY = 0.77f;
    }
}
