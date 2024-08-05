using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 몬스터의 공통 뼈대, 상속 받은 몬스터는 각자의 패턴과 공격 모션만 작성하면 됨
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
        Debug.Log($"{gameObject.name}이 방어막 {resultAmount}만큼 획득");
        this.barrier += resultAmount;
    }

    public override void GetDamage(float amount)
    {
        // 피해량 계산
        float resultAmount = HasBuff(E_BuffType.Vulnerability) ? amount * 1.5f : amount;

        // 배리어가 있는 경우
        if (barrier > 0)
        {
            barrier -= resultAmount;
            if (barrier < 0)
            {
                NowHp += barrier; // 남은 데미지를 HP에 적용
                barrier = 0;
            }
        }
        else // 배리어가 없는 경우
        {
            NowHp -= resultAmount;
        }

        // HP가 0 미만으로 떨어지지 않도록 설정
        NowHp = Mathf.Max(NowHp, 0);

        // 시각 효과 생성 임시 코드들 둘다 삭제 예정
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {barrier}, HP: {NowHp}");
    }


    public virtual void SetIntent()
    {   }

    public void AddInk(E_CardColor color)
    {
        Color nowColor = GetColor(color);
        
        //색을 처음 칠한다면
        if (InkCount == 0)
        {
            SR_PlayerInks[0].color = nowColor;
            _playerInks[0] = color;
            InkCount++;
        }
        //이미 색이 1종류 묻어 있다면
        else
        {
            //같은 색이라면 무시
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
        Debug.Log($"색{_playerInks[0]}과 {_playerInks[1]} 효과 발동");
        if(_playerInks[0]==E_CardColor.Black || _playerInks[1] == E_CardColor.Black)
        {
            Debug.Log("검은색 증발");
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
