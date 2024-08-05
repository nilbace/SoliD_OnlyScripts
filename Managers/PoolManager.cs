using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    public EffectPool Effect;


    public void Init()
    {
        Effect = new EffectPool();
    }
}


public class EffectPool
{
    private Dictionary<E_EffectType, List<BuffBase>> _effectDictionary;

    /// <summary>
    /// 생성시 각 상태 4개씩 초기화
    /// </summary>
    public EffectPool()
    {
        _effectDictionary = new Dictionary<E_EffectType, List<BuffBase>>();
    }


    public BuffBase GetEffect(E_EffectType effectType, float duration)
    {
        var effectList = _effectDictionary[effectType];
        if (effectList.Count > 0)
        {
            var effect = effectList[0];
            effectList.RemoveAt(0);
            //effect.Duration = duration;
            return effect;
        }
        else
        {
            return CreateEffect(effectType, duration);
        }

    }

    private BuffBase CreateEffect(E_EffectType effectType, float duration)
    {
        BuffBase newEffect = effectType switch
        {
            //E_EffectType.Stun => new StunEffect(duration),
            _ => null,
        };

        return newEffect;
    }

    public void ReturnEffect(E_EffectType effectType, BuffBase effect)
    {
        if (_effectDictionary.ContainsKey(effectType))
        {
            _effectDictionary[effectType].Add(effect);
        }
    }
}
