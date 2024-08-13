using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RelicGO : MonoBehaviour
{
    public RelicBase ThisRelic;
    private Image img_RelicIcon;
    private TMP_Text tmp_Stack;

    void Start()
    {
        img_RelicIcon = GetComponent<Image>();
        tmp_Stack = GetComponentInChildren<TMP_Text>();

        SetUP();
    }

    private void SetUP()
    {
        if (ThisRelic.HasStack) tmp_Stack.text = "0";
        img_RelicIcon.sprite = IconContainer.Inst.GetRelicSprite(ThisRelic.RelicType);
    }

    public void UpdateStack()
    {
        tmp_Stack.text = ThisRelic.Stack.ToString();
    }

    public void TwinkleIcon()
    {
        var twinkletime = 0.2f;
        img_RelicIcon.rectTransform.DOScale(1.5f, twinkletime).SetEase(Ease.InOutSine);

        // Fade the icon to 50% opacity over 0.5 seconds
        img_RelicIcon.DOFade(0.5f, twinkletime).SetEase(Ease.InOutSine);

        // Return to original scale and opacity over another 0.5 seconds
        img_RelicIcon.rectTransform.DOScale(1f, twinkletime).SetEase(Ease.InOutSine).SetDelay(twinkletime);
        img_RelicIcon.DOFade(1f, twinkletime).SetEase(Ease.InOutSine).SetDelay(twinkletime);
    }
}
