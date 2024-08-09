using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 모든 몬스터와 캐릭터의 공통 뼈대
/// 베리어, 데미지, 생존 처리등 기본 로직 보유
/// </summary>

public abstract class UnitBase : MonoBehaviour
{
    public string Name;
    protected float _nowHp;
    [HideInInspector] public float MaxHP;
    protected float _barrierAmount;
    [HideInInspector] public bool IsInjured;
    [HideInInspector] public bool IsChained;
    public SpriteRenderer[] BaseSprites;
    private bool _isDead;
    public List<BuffBase> BuffList =new List<BuffBase>();
    public Action EffectUpdateAction;
    public Action OnDead { get; set; }
    public virtual float GetBarrier()
    {
        return _barrierAmount;
    }


    public virtual void AddBarrier(float amount) 
    {
        _barrierAmount += amount;
    }

    public virtual void UpdateBarrier(float amount)
    {
        _barrierAmount = amount;
    }

    public virtual void GetDamage(float amount)
    {
        float resultAmount = HasBuff(E_BuffType.Vulnerability) ? amount * 1.5f : amount;

        float effectiveBarrier = GetBarrier();

        //베리어가 있다면
        if (effectiveBarrier > 0)
        {

            effectiveBarrier -= resultAmount;

            //베리어가 뚫렸다면
            if (effectiveBarrier < 0)
            {
                NowHp += effectiveBarrier;
                effectiveBarrier = 0;
            }
        }
        //베리어가 없다면
        else
        {
            NowHp -= resultAmount;
        }

        // Ensure HP doesn't drop below zero
        NowHp = Mathf.Max(NowHp, 0);

        // Update the barrier in the derived classes
        UpdateBarrier(effectiveBarrier);

        // Visual effect for damage
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {effectiveBarrier}, HP: {NowHp}");
    }



    private void ApplyStatusEffect(BuffBase effect)
    {
        effect.ApplyEffect(this);
        EffectUpdateAction?.Invoke();
    }

    public void ApplyBuff(E_BuffType type, float amount)
    {
        ApplyStatusEffect(BuffFactory.GetBuffByType(type,amount));
        VisualEffectManager.Inst.InstantiateEffect(type, transform.position);
    }

    public void RemoveBuff(E_BuffType type)
    {
        for (int i = 0; i < BuffList.Count; i++)
        {
            if (BuffList[i].BuffType == type)
            {
                // 효과 제거
                BuffList.RemoveAt(i);
                EffectUpdateAction?.Invoke();
                break; // 버프를 하나만 제거한 후 종료
            }
        }
    }

    public void ApplyBuff(E_EffectType type, float amount)
    {
        if (Enum.TryParse(type.ToString(), out E_BuffType newbuff))
        {
            ApplyBuff(newbuff, amount);
        }
        else
        {
            Debug.LogWarning($"Invalid buff type: {type}");
        }
    }


    public bool HasBuff(E_BuffType effectType)
    {
        var effect = BuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }
    public bool HasBuff(E_BuffType effectType, out BuffBase effect)
    {
        effect = BuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }

    public bool HasDebuff()
    {
        // ActiveEffectList에 IsDebuff가 true인 항목이 있는지 확인합니다.
        return BuffList.Any(effect => effect.IsDebuff);
    }


    public void ClearStatusEffect()
    {
        BuffList.Clear();
        EffectUpdateAction?.Invoke();
    }

    public float NowHp
    {
        get { return _nowHp; }
        set
        {
            float clampedValue = Mathf.Clamp(value, 0f, MaxHP);

            //HP가 감소할 때
            if (clampedValue < _nowHp)
            {
                _nowHp = clampedValue;
                //처음으로 HP가 0이하가 되었다면 Dead호출
                if (_nowHp == 0 && !_isDead)
                {
                    _isDead = true;
                    Dead();
                }
            }
            else
            {
                _nowHp = clampedValue;
            }
        }
    }

    public float DamagedAmount()
    {
        return MaxHP - NowHp;
    }
    public bool isAlive()
    {
        return NowHp > 0;
    }

    public virtual void Dead() {
        GetComponent<BoxCollider2D>().enabled = false;
        var seq = DOTween.Sequence();
        foreach(SpriteRenderer SR in BaseSprites)
        {
            //클로저 해결용 매개변수
            SpriteRenderer temp = SR;
            seq.Join(temp.DOFade(0f, 1f));
        }
        seq.AppendCallback(() => OnDead?.Invoke());
    }

    

    public void Heal(float amount)
    {
        var resultAmount = HasBuff(E_BuffType.Blessing) ? amount * 1.5f : amount;
        NowHp += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
    }


    [ContextMenu("Show Active Effects")]
    private void ShowActiveEffects()
    {
        // 현재 활성화된 이펙트를 표시합니다.
        foreach (BuffBase effect in BuffList)
        {
            Debug.Log($"Effect Type: {effect.BuffType}, Duration: {effect.Duration}, Stack: {effect.Stack}, InfoText: {effect.InfoText}");
        }
    }
}
