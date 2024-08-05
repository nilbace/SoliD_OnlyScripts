using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public abstract class UnitBase : MonoBehaviour
{
    [HideInInspector]public float _nowHp;
    [HideInInspector] public float MaxHP;
    [HideInInspector] public bool IsInjured;
    [HideInInspector] public bool IsChained;
    public SpriteRenderer[] BaseSprites;
    private bool _isDead;
    public List<BuffBase> BuffList =new List<BuffBase>();
    public Action EffectUpdateAction;
    public Action OnDead { get; set; }
    public abstract float GetBarrier();
    public abstract void AddBarrier(float barrier);


    [ContextMenu("Show Active Effects")]
    private void ShowActiveEffects()
    {
        // ���� Ȱ��ȭ�� ����Ʈ�� ǥ���մϴ�.
        foreach (BuffBase effect in BuffList)
        {
            Debug.Log($"Effect Type: {effect.BuffType}, Duration: {effect.Duration}, Stack: {effect.Stack}, InfoText: {effect.InfoText}");
        }
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
                // ȿ�� ����
                BuffList.RemoveAt(i);
                EffectUpdateAction?.Invoke();
                break; // ������ �ϳ��� ������ �� ����
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
        // ActiveEffectList�� IsDebuff�� true�� �׸��� �ִ��� Ȯ���մϴ�.
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

            //HP�� ������ ��
            if (clampedValue < _nowHp)
            {
                _nowHp = clampedValue;
                //ó������ HP�� 0���ϰ� �Ǿ��ٸ� Deadȣ��
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
            //Ŭ���� �ذ�� �Ű�����
            SpriteRenderer temp = SR;
            seq.Join(temp.DOFade(0f, 1f));
        }
        seq.AppendCallback(() => OnDead?.Invoke());
    }

    public abstract void GetDamage(float amount);

    public void Heal(float amount)
    {
        var resultAmount = HasBuff(E_BuffType.Blessing) ? amount * 1.5f : amount;
        NowHp += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
    }

}
