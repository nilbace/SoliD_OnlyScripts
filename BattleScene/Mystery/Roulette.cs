using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Roulette : MonoBehaviour
{
    public GameObject RouletteBoard;
    public GameObject ExitBTN;
    private bool _isSpinning;
    private int _result;


    public void Spin()
    {
        if (_isSpinning) return; // �̹� ���� �ִٸ� �ߺ����� ���� �ʵ���

        _isSpinning = true;

        // 5000������ 6000�� ������ ���� ���� ����
        int randomAngle = Random.Range(5000, 5000+360*3);
        _result = randomAngle % 360;
        _result = _result / 60;

        // DOTween�� �̿��� ȸ�� �ִϸ��̼� ����
        RouletteBoard.transform.DORotate(new Vector3(0, 0, randomAngle), 5, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic) // ���� �������� ȿ��
            .OnComplete(ShowResult); // �ִϸ��̼��� ������ ShowResult ȣ��
    }

    public void ShowResult()
    {
        _isSpinning = false;
        // ����� ǥ���ϴ� ���� �ۼ�
        Debug.Log($"������ {_result+1}��");
        ExitBTN.SetActive(true);
    }

    private void OnEnable()
    {
        ExitBTN.SetActive(false);
    }
}
