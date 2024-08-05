using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayableUnit : UnitBase
{
    protected static float sharedBarrier = 0;

    public override float GetBarrier()
    {
        return sharedBarrier;
    }

    public override void AddBarrier(float barrier)
    {
        // ����ȭ ȿ���� �ִ� ��� �� ������ ����
        float resultAmount = HasBuff(E_BuffType.Crystallization) ? barrier * 1.5f : barrier;

        // ���� �߰��ϰ� �ð� ȿ�� ����
        sharedBarrier += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}�� �� {resultAmount}��ŭ ȹ��");
    }
    public override void GetDamage(float amount)
    {
        // ��� ȿ���� �ִ� ��� ������ ����
        float resultAmount = HasBuff(E_BuffType.Vulnerability) ? amount * 1.5f : amount;

        // �踮� �ִ� ���
        if (sharedBarrier > 0)
        {
            sharedBarrier -= resultAmount;
            if (sharedBarrier < 0)
            {
                NowHp += sharedBarrier; // ���� �������� HP�� ����
                sharedBarrier = 0;
            }
        }
        else // �踮� ���� ���
        {
            NowHp -= resultAmount;
        }

        // HP�� 0 �̸����� �������� �ʵ��� ����
        NowHp = Mathf.Max(NowHp, 0);

        // �ð� ȿ�� ����
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {sharedBarrier}, HP: {NowHp}");
    }


    public override void Dead()
    {
        Debug.Log($"{gameObject.name} ���");
    }

    [ContextMenu("�� ǥ��")]
    public void ShowBarrier()
    {
        Debug.Log(sharedBarrier);
    }
    protected virtual void Start()
    {
        MaxHP = 20;
        NowHp = 15;
        BattleManager.Inst.PlayerUnits.Add(this);
        if(BattleManager.Inst.PlayerUnits.Count==3)
        {
            var seq = DOTween.Sequence();
            seq.Append(BattleManager.Inst.MoveCharFront(E_CharName.Minju));
        }
    }
}
