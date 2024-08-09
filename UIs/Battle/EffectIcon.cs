using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectIcon : MonoBehaviour
{
    public Image IMG_icon;
    public TMP_Text TMP_Count;

    public void SetIcon(BuffBase effect)
    {
        IMG_icon.sprite = IconContainer.Inst.GetEffectIcon(effect.BuffType);
        TMP_Count.text = ((effect.Duration != -1) ? effect.Duration : effect.Stack).ToString();
    }
}
