using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ��� ���Ϳ� ĳ������ ���� ����
/// ������, ������, ���� ó���� �⺻ ���� ����
/// </summary>

public abstract class UnitBase : MonoBehaviour
{
    public string Name;
    protected int _nowHp;
    [HideInInspector] public int MaxHP;
    protected int _barrierAmount;
    [HideInInspector] public bool IsInjured;
    [HideInInspector] public bool IsChained;
    public List<BuffBase> BuffList =new List<BuffBase>();
    public Action EffectUpdateAction;

    public delegate Sequence OnDeadDelegate();
    public OnDeadDelegate OnDead;

    public virtual int GetBarrier()
    {
        return _barrierAmount;
    }


    public virtual IEnumerator AddBarrierCoroutine(float amount)
    {
        int intAmount = (int)amount;
        _barrierAmount += intAmount;

        // Visual effect or delay logic
        if (amount > 0)
        {
            // Instantiate visual effect
            VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);

            yield return new WaitForSeconds(0.5f); // Adjust the time as needed
        }
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
            yield return StartCoroutine(DeadCoroutine());
        }
    }



    private void ApplyBuff(BuffBase effect)
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
                // ȿ�� ����
                BuffList.RemoveAt(i);
                EffectUpdateAction?.Invoke();
                break; // ������ �ϳ��� ������ �� ����
            }
        }
    }

    public virtual IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        Debug.Log($"{gameObject.name} ���� {type}�� {amount} ��ŭ �ο�");
        ApplyBuff(BuffFactory.GetBuffByType(type, amount));
        VisualEffectManager.Inst.InstantiateEffect(type, this);
        yield return new WaitForSeconds(0.5f);
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
        // ActiveEffectList�� IsDebuff�� true�� �׸��� �ִ��� Ȯ���մϴ�.
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
        Debug.Log(gameObject.name + " ���");

        // BoxCollider2D ������Ʈ�� �����ϰ� ��Ȱ��ȭ
        if (TryGetComponent<BoxCollider2D>(out var collider))
        {
            collider.enabled = false;
        }

        // �ڽ� ������Ʈ�� ��� SpriteRenderer�� ���� ���İ� ����
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            if (spriteRenderer.GetComponent<DestroyAfterAnimation>() != null) continue;
            StartCoroutine(FadeOut(spriteRenderer));
        }

        // �ڽ� ������Ʈ�� ��� Image�� ���� ���İ� ����
        foreach (Image image in GetComponentsInChildren<Image>())
        {
            StartCoroutine(FadeOut(image));
        }

        // �ڽ� ������Ʈ�� ��� TMP_Text�� ���� ���İ� ����
        foreach (TMP_Text tmpText in GetComponentsInChildren<TMP_Text>())
        {
            StartCoroutine(FadeOut(tmpText));
        }

        // ��� ���̵� �ƿ��� ���� ������ ���
        yield return new WaitForSeconds(1f);
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


    public IEnumerator HealCoroutine(float amount)
    {
        var resultAmount = (int)(HasBuff(E_EffectType.Blessing) ? amount * 1.5f : amount);
        _nowHp += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
        yield return new WaitForSeconds(0.5f);
    }
    
    [ContextMenu("Show Active Effects")]
    private void ShowActiveEffects()
    {
        // ���� Ȱ��ȭ�� ����Ʈ�� ǥ���մϴ�.
        foreach (BuffBase effect in BuffList)
        {
            Debug.Log($"Effect Type: {effect.BuffType}, Duration: {effect.Duration}, Stack: {effect.Stack}, InfoText: {effect.InfoText}");
        }
    }
}
