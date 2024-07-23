using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectIcon : MonoBehaviour
{
    public Image IMG_icon;
    public TMP_Text TMP_Count;

    public void SetIcon(EffectBase effect)
    {
        IMG_icon.sprite = BattleIconManager.Inst.GetEffectIcon(effect.ThisEffectType);
        TMP_Count.text = ((effect.Duration != -1) ? effect.Duration : effect.Stack).ToString();
    }
}
