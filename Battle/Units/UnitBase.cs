using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    [HideInInspector]public float _nowHp;
    [HideInInspector] public float MaxHP;
    [HideInInspector] public bool IsInjured;
    [HideInInspector] public bool IsChained;
    private bool _isDead;
    public List<EffectBase> ActiveEffectList;
    public Action EffectUpdateAction;
    public Action OnDead;
    public abstract float GetBarrier();
    public abstract void AddBarrier(float barrier);


    public void Start()
    {
        ActiveEffectList = new List<EffectBase>();
    }

    [ContextMenu("Show Active Effects")]
    private void ShowActiveEffects()
    {
        // ���� Ȱ��ȭ�� ����Ʈ�� ǥ���մϴ�.
        foreach (EffectBase effect in ActiveEffectList)
        {
            Debug.Log($"Effect Type: {effect.ThisEffectType}, Duration: {effect.Duration}, Stack: {effect.Stack}, InfoText: {effect.InfoText}");
        }
    }

    private void ApplyStatusEffect(EffectBase effect)
    {
        effect.ApplyEffect(this);
        EffectUpdateAction?.Invoke();
    }

    public void ApplyStatusEffect(E_EffectType type, float amount)
    {
        ApplyStatusEffect(EffectFactory.GetEffectByType(type,amount));
        VisualEffectManager.Inst.InstantiateEffect(type, this);
    }

    public bool HasEffect(E_EffectType effectType, out EffectBase effect)
    {
        effect = ActiveEffectList.FirstOrDefault(e => e.ThisEffectType == effectType);
        return effect != null;
    }


    public void ClearStatusEffect()
    {
        ActiveEffectList.Clear();
        EffectUpdateAction?.Invoke();
    }

    public float NowHp
    {
        get { return _nowHp; }
        set
        {
            float clampedValue = Mathf.Clamp(value, -100f, MaxHP);

            //HP�� ������ ��
            if (clampedValue < _nowHp)
            {
                _nowHp = clampedValue;
                //ó������ HP�� 0���ϰ� �Ǿ��ٸ� Deadȣ��
                if (_nowHp <= 0 && !_isDead)
                {
                    _isDead = true;
                    Debug.Log("���");
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

    public virtual void Dead() { }

    public abstract void GetDamage(float amount);

    public void Heal(float amount)
    {
        NowHp += amount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
    }

}
