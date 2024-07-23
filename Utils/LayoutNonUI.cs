using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutNonUI : MonoBehaviour
{
    public float Spacing;

    private void Start()
    {
        ArrangeObjects();
    }

    private void ArrangeObjects()
    {
        // 자식 오브젝트 개수 가져오기
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            // 각 자식 오브젝트의 Transform 가져오기
            Transform child = transform.GetChild(i);

            // 새로운 위치 계산
            Vector3 newPosition = new Vector3(i * Spacing, 0, 0); // x축으로 spacing 간격으로 정렬
            child.localPosition = newPosition; // 자식 오브젝트의 위치 설정

        }
    }

}
