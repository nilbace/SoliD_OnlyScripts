using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모든 몬스터와 캐릭터의 공통 뼈대
/// 베리어, 데미지, 생존 처리등 기본 로직 보유
/// </summary>

public abstract class UnitBase : MonoBehaviour
{
    public string Name;
    protected int _nowHp;
    [HideInInspector] public int MaxHP;
    protected int _barrierAmount;
    [HideInInspector] public bool IsInjured;
    [HideInInspector] public bool IsChained;
    private bool _isDead;
    public List<BuffBase> BuffList =new List<BuffBase>();
    public Action EffectUpdateAction;

    public delegate Sequence OnDeadDelegate();
    public OnDeadDelegate OnDead;

    public virtual int GetBarrier()
    {
        return _barrierAmount;
    }


    public virtual void AddBarrier(float amount) 
    {
        int intAmount = (int)amount;
        _barrierAmount += intAmount;
        if(amount>0) VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, transform.position);
    }

    public virtual void UpdateBarrier(float amount)
    {
        int intAmount = (int)amount;
        _barrierAmount = intAmount;
    }

    public void SetUpHP()
    {
        _nowHp = MaxHP;
    }

    public int GetHP()
    {
        return _nowHp;
    }


    public IEnumerator GetDamageCoroutine(float amount)
    {
        yield return new WaitForEndOfFrame(); // Ensure coroutine begins at the next frame

        // Calculate damage considering vulnerability
        int resultAmount = (int)(HasBuff(E_EffectType.Vulnerability) ? amount * 1.5f : amount);
        int effectiveBarrier = GetBarrier();

        // Process damage against barrier
        if (effectiveBarrier > 0)
        {
            effectiveBarrier -= resultAmount;

            if (effectiveBarrier < 0)
            {
                _nowHp += effectiveBarrier; // Reduce HP by the overflow damage
                effectiveBarrier = 0;
            }
        }
        else
        {
            _nowHp -= resultAmount;
        }

        // Update the barrier in the derived classes
        UpdateBarrier(effectiveBarrier);

        // Trigger visual effect for damage
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {effectiveBarrier}, HP: {_nowHp}");

        // Check if the character is dead
        if (_nowHp <= 0)
        {
            Debug.Log("죽음 처리 시작");
            yield return StartCoroutine(DeadCoroutine());
        }

        Debug.Log($"GetDamage 처리 완료, 현재 HP: {_nowHp}");
    }



    private void ApplyStatusEffect(BuffBase effect)
    {
        effect.ApplyEffect(this);
        EffectUpdateAction?.Invoke();
    }

  

    public void RemoveBuff(E_EffectType type)
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

    public virtual Sequence ApplyBuff(E_EffectType type, float amount)
    {
        // 새로운 DOTween 시퀀스를 생성합니다.
        Sequence sequence = DOTween.Sequence();

        // 1초 동안 효과를 적용하는 시퀀스를 정의합니다.
        sequence.AppendCallback(() => ApplyStatusEffect(BuffFactory.GetBuffByType(type, amount)))
                .AppendCallback(() => VisualEffectManager.Inst.InstantiateEffect(type, transform.position))
                .AppendInterval(0.5f); // 추가로 초 대기

        // 시퀀스를 반환합니다.
        return sequence;
    }

    public bool HasBuff(E_EffectType effectType)
    {
        var effect = BuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }
    public bool HasBuff(E_EffectType effectType, out BuffBase effect)
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

 
   
    public bool isAlive()
    {
        return _nowHp > 0;
    }

    public virtual IEnumerator DeadCoroutine()
    {
        Debug.Log(gameObject.name + " 사망");

        // BoxCollider2D 컴포넌트를 안전하게 비활성화
        if (TryGetComponent<BoxCollider2D>(out var collider))
        {
            collider.enabled = false;
        }

        // 자식 오브젝트의 모든 SpriteRenderer에 대한 알파값 조절
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            Debug.Log(spriteRenderer.gameObject.name);

            StartCoroutine(FadeOut(spriteRenderer));
        }

        // 자식 오브젝트의 모든 Image에 대한 알파값 조절
        foreach (Image image in GetComponentsInChildren<Image>())
        {
            StartCoroutine(FadeOut(image));
        }

        // 자식 오브젝트의 모든 TMP_Text에 대한 알파값 조절
        foreach (TMP_Text tmpText in GetComponentsInChildren<TMP_Text>())
        {
            StartCoroutine(FadeOut(tmpText));
        }

        // 모든 페이드 아웃이 끝날 때까지 대기
        yield return new WaitForSeconds(1f);

        Debug.Log("DeadCoroutine 완료");
    }

    // Fade out a SpriteRenderer over time
    private IEnumerator FadeOut(SpriteRenderer spriteRenderer)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Color initialColor = spriteRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
    }

    // Fade out an Image over time
    private IEnumerator FadeOut(Image image)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Color initialColor = image.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / duration);
            image.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        image.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
    }

    // Fade out a TMP_Text over time
    private IEnumerator FadeOut(TMP_Text tmpText)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Color initialColor = tmpText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / duration);
            tmpText.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        tmpText.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
    }



    public Sequence Heal(float amount)
    {
        var seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            var resultAmount = (int)(HasBuff(E_EffectType.Blessing) ? amount * 1.5f : amount);
            _nowHp += resultAmount;
            VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
        }).AppendInterval(0.5f);

        return seq;
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
