using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
    public static void TwinkleIcon(this Image img, float scaleFactor = 1.5f, float twinkleTime = 0.2f)
    {
        // �̹��� ������ ����
        img.rectTransform.DOScale(scaleFactor, twinkleTime).SetEase(Ease.InOutSine);

        // �̹��� ���̵� �ƿ�
        img.DOFade(0.5f, twinkleTime).SetEase(Ease.InOutSine);

        // ���� �����ϰ� ������ ����
        img.rectTransform.DOScale(1f, twinkleTime).SetEase(Ease.InOutSine).SetDelay(twinkleTime);
        img.DOFade(1f, twinkleTime).SetEase(Ease.InOutSine).SetDelay(twinkleTime);
    }

    public static void UpdateHP(this Slider slider, UnitBase unit)
    {
        if (unit != null)
        {
            slider.value = Mathf.Clamp(((float)unit.GetCurrentHP() / (float)unit.MaxHP), 0.045f, 1f);
        }
    }
}
