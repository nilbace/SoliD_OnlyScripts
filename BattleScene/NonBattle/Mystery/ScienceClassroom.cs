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
            TMP_Goal.text = "������ �þ��� ������!";
        }
        else if (rand == 1)
        {
            _answerColor = Color.green;
            TMP_Goal.text = "�ʷϻ� �þ��� ������!";
        }

        else
        {
            _answerColor = Color.blue;
            TMP_Goal.text = "�Ķ��� �þ��� ������!";
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
            // �� ���� ������ ȥ��
            _resultColor = MixColors(_resultColor, inputColor);
            seq.Append(ResultBeaker.DOColor(_resultColor, 1f))
            .AppendCallback(() => { CheckColorResult();   });
        }
        
    }

    //���� ȥ�� ���
    private Color MixColors(Color color1, Color color2)
    {
        //����Ÿ+��� > ����
        if ((color1 == magenta && color2 == yellow) || (color1 == yellow && color2 == magenta))
        {
            return Color.red;
        }
        //��� + �þ� > �ʷ�
        else if ((color1 == yellow && color2 == cyan) || (color1 == cyan && color2 == yellow))
        {
            return Color.green;
        }
        //�þ� + ����Ÿ > �Ķ�
        else if ((color1 == cyan && color2 == magenta) || (color1 == magenta && color2 == cyan))
        {
            return Color.blue;
        }
        else
        {
            return Color.black; // �߸��� ȥ�� �� �⺻�� (����)
        }
    }


    private void CheckColorResult()
    {
        if(_resultColor == _answerColor)
        {
            Debug.Log("����");
        }
        else
        {
            Debug.Log("����");
        }

        var seq = DOTween.Sequence();
        seq.AppendInterval(1f)
            .AppendCallback(()=> { MysteryManager.Inst.ExitMystery(); Destroy(gameObject); });
    }
}
