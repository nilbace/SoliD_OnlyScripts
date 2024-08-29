using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragParentNonUI : MonoBehaviour
{
    private float minY = 0f;  // y축 최소 제한
    private float maxY = 0.77f;   // y축 최대 제한
    private Vector3 offset;
    private Vector3 startParentPosition;

    void OnMouseDown()
    {
        // 부모 오브젝트의 시작 위치를 저장합니다.
        startParentPosition = transform.parent.position;
        // 마우스와 오브젝트 사이의 오프셋을 계산합니다.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = startParentPosition - mousePosition;
    }

    void OnMouseDrag()
    {
        transform.parent.DOKill();
        // 현재 마우스 위치를 가져옵니다.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;

        // 부모 오브젝트의 y축 위치를 업데이트합니다.
        transform.parent.position = new Vector3(startParentPosition.x, mousePosition.y, startParentPosition.z);
    }

    void OnMouseUp()
    {
        // 드래그가 끝나면 부모 오브젝트의 y축 위치를 확인하고, 범위를 벗어난 경우 제한 범위로 이동
        float currentY = transform.parent.position.y;

        // 제한 범위를 벗어난 경우
        if (currentY < minY || currentY > maxY)
        {
            // y축 위치를 제한된 위치로 이동
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
