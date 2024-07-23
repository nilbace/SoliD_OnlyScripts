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
        if (_isSpinning) return; // 이미 돌고 있다면 중복으로 돌지 않도록

        _isSpinning = true;

        // 5000도에서 6000도 사이의 랜덤 각도 생성
        int randomAngle = Random.Range(5000, 5000+360*3);
        _result = randomAngle % 360;
        _result = _result / 60;

        // DOTween을 이용해 회전 애니메이션 설정
        RouletteBoard.transform.DORotate(new Vector3(0, 0, randomAngle), 5, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic) // 점점 느려지는 효과
            .OnComplete(ShowResult); // 애니메이션이 끝나면 ShowResult 호출
    }

    public void ShowResult()
    {
        _isSpinning = false;
        // 결과를 표시하는 로직 작성
        Debug.Log($"보상은 {_result+1}번");
        ExitBTN.SetActive(true);
    }

    private void OnEnable()
    {
        ExitBTN.SetActive(false);
    }
}
