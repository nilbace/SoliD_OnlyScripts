using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
/// <summary>
/// ������ ���� ����, ��� ���� ���ʹ� ������ ���ϰ� ���� ��Ǹ� �ۼ��ϸ� ��
/// </summary>

public abstract class MonsterBase : UnitBase
{
    public int StartHP;
    public int NowIntentNumber;
    public SpriteRenderer SR_Intent;
    public List<SpriteRenderer> SR_PlayerInks;
    private E_CardColor[] _playerInks;
    private int _inkCount;

    public virtual void Start()
    {
        _playerInks = new E_CardColor[2];
        MaxHP = StartHP; SetUpHP();
    }

    public virtual void SetIntent()
    {
        SR_Intent.DOFade(1, 0.4f);
    }

    public void HideIntent()
    {
        SR_Intent.DOFade(0, 0.4f);
    }

    public void AddInk(E_CardColor color)
    {
        Color nowColor = GetColor(color);
        
        //���� ó�� ĥ�Ѵٸ�
        if (_inkCount == 0)
        {
            SR_PlayerInks[0].color = nowColor;
            _playerInks[0] = color;
            _inkCount++;
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
                _inkCount++;
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
        _inkCount = 0;
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

    public IEnumerator Attack(float amount)
    {
        return BattleManager.Inst.MonsterAttackPlayer(amount);
    }

    public IEnumerator ApplyEffectToPlayer(E_EffectType type, float amount)
    {
        return BattleManager.Inst.MonsterApplyEffect_To_Player(type, amount);
    }

    public virtual IEnumerator StartNowPattern()
    {
        return null;
    }
  }

