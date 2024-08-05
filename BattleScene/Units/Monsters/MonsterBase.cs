using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// ������ ���� ����, ��� ���� ���ʹ� ������ ���ϰ� ���� ��Ǹ� �ۼ��ϸ� ��
/// </summary>


public class MonsterBase : UnitBase
{
    private float barrier;
    public int StartHP;
    protected int NowIntentNumber;
    public SpriteRenderer SR_Intent;
    public List<SpriteRenderer> SR_PlayerInks;
    private E_CardColor[] _playerInks;
    private int InkCount;

    public virtual void Start()
    {
        _playerInks = new E_CardColor[2];
        NowHp = MaxHP = StartHP;
    }

    public override float GetBarrier()
    {
        return barrier;
    }

    public override void AddBarrier(float barrier)
    {
        float resultAmount = HasBuff(E_BuffType.Crystallization) ? barrier * 1.5f : barrier;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}�� �� {resultAmount}��ŭ ȹ��");
        this.barrier += resultAmount;
    }

    public override void GetDamage(float amount)
    {
        // ���ط� ���
        float resultAmount = HasBuff(E_BuffType.Vulnerability) ? amount * 1.5f : amount;

        // �踮� �ִ� ���
        if (barrier > 0)
        {
            barrier -= resultAmount;
            if (barrier < 0)
            {
                NowHp += barrier; // ���� �������� HP�� ����
                barrier = 0;
            }
        }
        else // �踮� ���� ���
        {
            NowHp -= resultAmount;
        }

        // HP�� 0 �̸����� �������� �ʵ��� ����
        NowHp = Mathf.Max(NowHp, 0);

        // �ð� ȿ�� ���� �ӽ� �ڵ�� �Ѵ� ���� ����
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {barrier}, HP: {NowHp}");
    }


    public virtual void SetIntent()
    {   }

    public void AddInk(E_CardColor color)
    {
        Color nowColor = GetColor(color);
        
        //���� ó�� ĥ�Ѵٸ�
        if (InkCount == 0)
        {
            SR_PlayerInks[0].color = nowColor;
            _playerInks[0] = color;
            InkCount++;
        }
        //�̹� ���� 1���� ���� �ִٸ�
        else
        {
            //���� ���̶�� ����
            if (color == _playerInks[0]) return;
            var seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                SR_PlayerInks[1].color = nowColor;
                _playerInks[1] = color;
                InkCount++;
            })
                .AppendInterval(0.2f)
                .AppendCallback(InkBurst);
        }
    }

    public void InkBurst()
    {
        Debug.Log($"��{_playerInks[0]}�� {_playerInks[1]} ȿ�� �ߵ�");
        if(_playerInks[0]==E_CardColor.Black || _playerInks[1] == E_CardColor.Black)
        {
            Debug.Log("������ ����");
        }
        else
        {
            InkSkillManager.Inst.UseInkSkill(_playerInks[0]);
            InkSkillManager.Inst.UseInkSkill(_playerInks[1]);
        }

        ClearInk();
    }

    public void ClearInk()
    {
        InkCount = 0;
        SR_PlayerInks[0].color = Color.clear;
        SR_PlayerInks[1].color = Color.clear;
        _playerInks = new E_CardColor[2];
    }

    private Color GetColor(E_CardColor color)
    {
        if (color == E_CardColor.Magenta) return ColorExtensions.magenta;
        if (color == E_CardColor.Cyan) return ColorExtensions.cyan;
        if (color == E_CardColor.Yellow) return Color.yellow;
        if (color == E_CardColor.Black) return Color.black;
        return Color.clear;
    }

    public void Attack(float amount)
    {
        BattleManager.Inst.MonsterAttackPlayer(amount);
    }

    public void ApplyEffectToPlayer(E_EffectType type, float amount)
    {
        BattleManager.Inst.MonsterApplyEffect_To_Player(type, amount);
    }

    public virtual Sequence GetSequenceByIntent()
    {
        return null;
    }
}
