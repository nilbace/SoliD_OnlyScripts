using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScienceClassroom : MonoBehaviour
{
    private bool _canClick;

    private Color _resultColor;
    private Color _answerColor;
    private Color magenta = Color.magenta;
    private Color cyan = Color.cyan;
    private Color yellow = Color.yellow;

    public TMPro.TMP_Text TMP_Goal;
    public SpriteRenderer ResultBeaker;

    private void OnEnable()
    {
        _canClick = true;
        _resultColor = Color.white;
        ResultBeaker.color = Color.white;
        MakeAnswerColor();
    }

    private void MakeAnswerColor()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0)
        {
            _answerColor = Color.red;
            TMP_Goal.text = "빨간색 시약을 만들자!";
        }
        else if (rand == 1)
        {
            _answerColor = Color.green;
            TMP_Goal.text = "초록색 시약을 만들자!";
        }

        else
        {
            _answerColor = Color.blue;
            TMP_Goal.text = "파란색 시약을 만들자!";
        }
    }

    public void AddColor(Color inputColor)
    {
        if (!_canClick) return;

        var seq = DOTween.Sequence();
        _canClick = false;
        if (_resultColor == Color.white)
        {
            _resultColor = inputColor;
            seq.Append(ResultBeaker.DOColor(_resultColor, 1f))
            .AppendCallback(() => { _canClick = true; });
        }
        else
        {
            // 두 가지 색상을 혼합
            _resultColor = MixColors(_resultColor, inputColor);
            seq.Append(ResultBeaker.DOColor(_resultColor, 1f))
            .AppendCallback(() => { CheckColorResult();   });
        }
        
    }

    //색상 혼합 결과
    private Color MixColors(Color color1, Color color2)
    {
        //마젠타+노랑 > 빨강
        if ((color1 == magenta && color2 == yellow) || (color1 == yellow && color2 == magenta))
        {
            return Color.red;
        }
        //노랑 + 시안 > 초록
        else if ((color1 == yellow && color2 == cyan) || (color1 == cyan && color2 == yellow))
        {
            return Color.green;
        }
        //시안 + 마젠타 > 파랑
        else if ((color1 == cyan && color2 == magenta) || (color1 == magenta && color2 == cyan))
        {
            return Color.blue;
        }
        else
        {
            return Color.black; // 잘못된 혼합 시 기본값 (없음)
        }
    }


    private void CheckColorResult()
    {
        if(_resultColor == _answerColor)
        {
            Debug.Log("성공");
        }
        else
        {
            Debug.Log("실패");
        }

        var seq = DOTween.Sequence();
        seq.AppendInterval(1f)
            .AppendCallback(()=> { MysteryManager.Inst.ExitMystery(); Destroy(gameObject); });
    }
}
