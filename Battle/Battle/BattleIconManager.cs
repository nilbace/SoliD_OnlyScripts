using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_IntentIconType { Attack, Shield, }

public class BattleIconManager : MonoBehaviour
{
    public static BattleIconManager Inst;

    //이름으로 관리
    public List<Sprite> EffectIcons;
    //순서로 관리(임시)
    public List<Sprite> IntentIcons;

    private void Awake()
    {
        Inst = this;
    }

    /// <summary>
    /// 버프창 아이콘
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public Sprite GetEffectIcon(E_EffectType effect)
    {
        foreach (var icon in EffectIcons)
        {
            if (icon.name == effect.ToString())
            {
                return icon;
            }
        }

        // 해당 이름의 스프라이트를 찾지 못한 경우 null 반환
        return null;
    }

    public Sprite GetIntentIcon(E_IntentIconType intent)
    {
        return IntentIcons[(int)intent];
    }
}
