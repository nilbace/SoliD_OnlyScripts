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
    protected int _currentHp;                   //���� ü��
    [HideInInspector] public int MaxHP;         //�ִ� ü��
    protected int _currentBarrier;              //���� ��ȣ�� ��ġ
    [HideInInspector] public bool IsInjured;    //�λ� ���� ����
    [HideInInspector] public bool IsChained;    //�ൿ �Ұ� ����
    public List<BuffBase> ActiveBuffList =new List<BuffBase>(); //���� ���� ���
    public Action OnBuffUpdate { get; set; }  //���� ���� �׼�

    //����� ȣ��� ��������Ʈ
    public delegate Sequence OnDeadDelegate();
    public OnDeadDelegate OnDeath;

    private UI_UnitBuffBar _unitEffects;  //���� ǥ�� UI ���� ��ũ��Ʈ

    protected virtual void Awake()
    {
        _unitEffects = GetComponentInChildren<UI_UnitBuffBar>();
    }

    public virtual int GetCurrentBarrier()
    {
        return _currentBarrier;
    }

    /// <summary>
    /// amount��ŭ ����� ȹ��
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public virtual IEnumerator AddBarrierCoroutine(float amount)
    {
        int intAmount = (int)amount;
        _currentBarrier += intAmount;

        // Visual effect or delay logic
        if (amount > 0)
        {
            // Instantiate visual effect
            VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);

            yield return new WaitForSeconds(0.5f); // Adjust the time as needed
        }
    }

    /// <summary>
    /// ������ ��ġ�� Ư�� ��ġ�� ����
    /// </summary>
    /// <param name="amount"></param>
    public virtual void UpdateBarrier(float amount)
    {
        int intAmount = (int)amount;
        _currentBarrier = intAmount;
    }

    /// <summary>
    /// ����Ʈ ���� ü���� �ִ�ġ��ŭ ȸ��
    /// </summary>
    public void SetHP_To_Max_WithoutVFX()
    {
        _currentHp = MaxHP;
    }

    public int GetCurrentHP()
    {
        return _currentHp;
    }

    /// <summary>
    /// ȸ�� ó�� �ڷ�ƾ
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public IEnumerator HealCoroutine(float amount)
    {
        var resultAmount = (int)(HasBuff(E_EffectType.Blessing) ? amount * 1.5f : amount);
        _currentHp += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Heal, this);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Ư�� ����Ʈ�� �Բ� �������� ����, ��κ��� ��Ȳ���� ȣ��Ǵ� �Լ�
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="effectType"></param>
    /// <returns></returns>
    public IEnumerator GetDamageCoroutine(float amount, E_EffectType effectType)
    {
        yield return new WaitForEndOfFrame();
        ApplyDamage(amount, effectType);
    }

    /// <summary>
    /// CardEffectManager���� �ַ� ȣ���ϸ�, ���� ������� ī�� ȿ���� ���� ����Ʈ�� �Բ� �������� ����
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public IEnumerator GetDamageCoroutine(float amount)
    {
        yield return new WaitForEndOfFrame();
        ApplyDamage(amount, CardEffectManager.CurrentCardData.DamageEffectType);
    }

    /// <summary>
    /// �������� �޴� ������ ó���ϴ� �Լ�
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="effectType"></param>
    protected virtual void ApplyDamage(float amount, E_EffectType effectType)
    {
        // Calculate damage considering vulnerability
        int resultAmount = (int)(HasBuff(E_EffectType.Vulnerability) ? amount * 1.5f : amount);
        int effectiveBarrier = GetCurrentBarrier();

        //������ üũ
        if (effectiveBarrier > 0)
        {
            effectiveBarrier -= resultAmount;

            if (effectiveBarrier < 0)
            {
                _currentHp += effectiveBarrier; // Reduce HP by the overflow damage
                effectiveBarrier = 0;
            }
        }
        else
        {
            _currentHp -= resultAmount;
        }

        // Update the barrier in the derived classes
        UpdateBarrier(effectiveBarrier);

        // Trigger visual effect for damage
        VisualEffectManager.Inst.InstantiateEffect(effectType, this);
        Debug.Log($"{gameObject.name} took {resultAmount} damage. Barrier: {effectiveBarrier}, HP: {_currentHp}");

        // Check if the character is dead
        if (_currentHp <= 0)
        {
            StartCoroutine(DeathCoroutine());
        }
    }



    private void ApplyBuff(BuffBase effect)
    {
        effect.ApplyEffect(this);
        OnBuffUpdate?.Invoke();
    }

  

    public void RemoveBuff(E_EffectType type)
    {
        for (int i = 0; i < ActiveBuffList.Count; i++)
        {
            if (ActiveBuffList[i].BuffType == type)
            {
                // ȿ�� ����
                ActiveBuffList.RemoveAt(i);
                OnBuffUpdate?.Invoke();
                break; // ������ �ϳ��� ������ �� ����
            }
        }
    }

    /// <summary>
    /// Ư�� ���� ������ ���� ȿ��
    /// </summary>
    /// <param name="type"></param>
    public void TwinkleBuffIcon(E_EffectType type)
    {
        if(HasBuff(type, out BuffBase buffObject))
        {
            var buffindex = ActiveBuffList.IndexOf(buffObject);
            _unitEffects.TwinkleIcion(buffindex);
        }
    }

    /// <summary>
    /// �ش� ������ amount���ø�ŭ ����
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public virtual IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        if (!IsAlive()) yield break;
        Debug.Log($"{gameObject.name} ���� {type}�� {amount} ��ŭ �ο�");
        ApplyBuff(BuffFactory.GetBuffByType(type, amount));
        VisualEffectManager.Inst.InstantiateEffect(type, this);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// �ش� ������ ������ �ִ��� Ȯ��
    /// </summary>
    /// <param name="effectType"></param>
    /// <returns></returns>
    public bool HasBuff(E_EffectType effectType)
    {
        var effect = ActiveBuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }

    /// <summary>
    /// �ش� ������ ������ �ִ��� Ȯ���ϸ� outŰ����� �ش� ���� ��ȯ
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="effect"></param>
    /// <returns></returns>
    public bool HasBuff(E_EffectType effectType, out BuffBase effect)
    {
        effect = ActiveBuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }

    /// <summary>
    /// �ϳ� �̻��� ������� ������ �ִ��� Ȯ��
    /// </summary>
    /// <returns></returns>
    public bool HasDebuff()
    {
        // ActiveEffectList�� IsDebuff�� true�� �׸��� �ִ��� Ȯ���մϴ�.
        return ActiveBuffList.Any(effect => effect.IsDebuff);
    }


    public void ClearAllBuffs()
    {
        ActiveBuffList.Clear();
        OnBuffUpdate?.Invoke();
    }

 
   
    public bool IsAlive()
    {
        return _currentHp > 0;
    }

    /// <summary>
    /// ��� ó�� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator DeathCoroutine()
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

    //SpriteRenderer ���̵� �ƿ�
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

    //Image ���̵� �ƿ�
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

    //TMP_Text ���̵� �ƿ�
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
 }
