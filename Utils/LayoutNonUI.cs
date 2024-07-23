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
        // �ڽ� ������Ʈ ���� ��������
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            // �� �ڽ� ������Ʈ�� Transform ��������
            Transform child = transform.GetChild(i);

            // ���ο� ��ġ ���
            Vector3 newPosition = new Vector3(i * Spacing, 0, 0); // x������ spacing �������� ����
            child.localPosition = newPosition; // �ڽ� ������Ʈ�� ��ġ ����

        }
    }

}
