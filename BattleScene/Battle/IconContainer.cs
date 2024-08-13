using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_IntentIconType { attack, bless, bless_attack, bless_shield, curse, curse_attack, curse_shield, shield }

public class IconContainer : MonoBehaviour
{
    public static IconContainer Inst;

    //�̸����� ����
    public List<Sprite> BuffIcons;
    public List<Sprite> IntentIcons;
    public List<Sprite> RelicIcons;

    private void Awake()
    {
        Inst = this;
    }

    /// <summary>
    /// ����â ������
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public Sprite GetBuffIcon(E_EffectType effect)
    {
        foreach (var icon in BuffIcons)
        {
            if (icon.name == effect.ToString())
            {
                return icon;
            }
        }

        // �ش� �̸��� ��������Ʈ�� ã�� ���� ��� null ��ȯ
        return null;
    }

    /// <summary>
    /// ���� �ǵ� ������
    /// </summary>
    /// <param name="intent"></param>
    /// <returns></returns>
    public Sprite GetIntentIcon(E_IntentIconType intent)
    {
        foreach (var icon in IntentIcons)
        {
            if (icon.name == intent.ToString())
            {
                return icon;
            }
        }

        return null;
    }


    public Sprite GetRelicSprite(E_RelicType relic)
    {
        foreach (var icon in RelicIcons)
        {
            if (icon.name == relic.ToString())
            {
                return icon;
            }
        }

        return null;
    }
}
