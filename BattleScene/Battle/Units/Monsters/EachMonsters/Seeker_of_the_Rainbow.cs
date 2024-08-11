using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker_of_the_Rainbow : MonsterBase
{
    public float moveTimeOut;
    public float moveTimeIn;
    public override void Start()
    {
        base.Start();
        ApplyBuff(E_EffectType.LightThirst, 1);
    }

    public override Sequence ApplyBuff(E_EffectType type, float amount)
    {

        // ������ ������ �����ϴ� ȿ�� ������ �׷�ȭ�Ͽ� ó���մϴ�.
        if (type == E_EffectType.Frost ||
            type == E_EffectType.Electrocution ||
            type == E_EffectType.Burn ||
            type == E_EffectType.Posion)
        {
            return LightEat(); // LightEat �������� ��ȯ
        }

        return base.ApplyBuff(type, amount); // �⺻ ���� ��ȯ
    }

    private Sequence LightEat()
    {
        var seq = DOTween.Sequence();

        // DOTween �������� �۾��� �߰��մϴ�.
        seq.Append(Heal(5)).AppendCallback(() => ApplyBuff(E_EffectType.Strength, 5));

        return seq; // �������� ��ȯ
    }

    public override void SetIntent()
    {
        base.SetIntent();
        SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.attack);
    }

    //public override IEnumerator StartNowPattern()
    //{
    //    return AttackSequence();
    //}

    //private Sequence AttackSequence()
    //{
    //    Sequence attackSequence = DOTween.Sequence();

    //    attackSequence.Append(transform.DOMoveX(transform.position.x - 1, moveTimeOut))
    //                     .Append(Attack(5))
    //                     .Append(transform.DOMoveX(transform.position.x, moveTimeIn));

    //    return attackSequence;
    //}
}
