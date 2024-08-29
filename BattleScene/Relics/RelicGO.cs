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
        img_RelicIcon.TwinkleIcon();
    }
}
