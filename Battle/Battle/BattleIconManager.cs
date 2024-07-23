using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_IntentIconType { Attack, Shield, }

public class BattleIconManager : MonoBehaviour
{
    public static BattleIconManager Inst;

    //�̸����� ����
    public List<Sprite> EffectIcons;
    //������ ����(�ӽ�)
    public List<Sprite> IntentIcons;

    private void Awake()
    {
        Inst = this;
    }

    /// <summary>
    /// ����â ������
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

        // �ش� �̸��� ��������Ʈ�� ã�� ���� ��� null ��ȯ
        return null;
    }

    public Sprite GetIntentIcon(E_IntentIconType intent)
    {
        return IntentIcons[(int)intent];
    }
}
