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
        // 결정화 효과가 있는 경우 방어막 증가량 증가
        float resultAmount = HasBuff(E_BuffType.Crystallization) ? barrier * 1.5f : barrier;

        // 방어막을 추가하고 시각 효과 생성
        sharedBarrier += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}이 방어막 {resultAmount}만큼 획득");
    }
    public override void GetDamage(float amount)
    {
        // 취약 효과가 있는 경우 데미지 증가
        float resultAmount = HasBuff(E_BuffType.Vulnerability) ? amount * 1.5f : amount;

        // 배리어가 있는 경우
        if (sharedBarrier > 0)
        {
            sharedBarrier -= resultAmount;
            if (sharedBarrier < 0)
            {
                NowHp += sharedBarrier; // 남은 데미지를 HP에 적용
                sharedBarrier = 0;
            }
        }
        else // 배리어가 없는 경우
        {
            NowHp -= resultAmount;
        }

        // HP가 0 미만으로 떨어지지 않도록 설정
        NowHp = Mathf.Max(NowHp, 0);

        // 시각 효과 생성
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {sharedBarrier}, HP: {NowHp}");
    }


    public override void Dead()
    {
        Debug.Log($"{gameObject.name} 사망");
    }

    [ContextMenu("방어막 표시")]
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
