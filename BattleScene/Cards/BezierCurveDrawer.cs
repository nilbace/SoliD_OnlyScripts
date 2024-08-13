using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveDrawer : MonoBehaviour
{
    public static BezierCurveDrawer Inst;
    public LineRenderer lineRenderer;
    public int curveResolution = 20; // ��� �ػ�, �� �������� �ε巯�� ��� �˴ϴ�.
    public Vector3 ControlPoint;
    public float startYpoz;

    private void Awake()
    {
        Inst = this;
    }
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = curveResolution; // ���� �� ���� ����
    }


    public void DrawCurveFromScreenBottom()
    {
        Vector3 startPos = new Vector3(Screen.width / 2, startYpoz, 0); // ȭ�� ��� �ϴ�
        startPos = Camera.main.ScreenToWorldPoint(startPos + new Vector3(0, 0, 10)); // ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ

        Vector3 endPos = Input.mousePosition; // ���콺 ��ġ
        endPos = Camera.main.ScreenToWorldPoint(endPos + new Vector3(0, 0, 10)); // ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ

        Vector3 controlPoint = ControlPoint; // ������ ����

        lineRenderer.positionCount = curveResolution;
        for (int i = curveResolution - 1; i >= 0; i--)
        {
            float t = i / (float)(curveResolution - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, endPos, controlPoint, startPos);
            lineRenderer.SetPosition(i, position);
        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // ������ � ����
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // ù ��° ��
        p += 2 * u * t * p1; // �� ��° ��
        p += tt * p2; // �� ��° ��
        p.z = -1;
        return p;
    }

}
