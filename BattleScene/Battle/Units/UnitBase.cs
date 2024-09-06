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
    protected int _currentHp;                   //현재 체력
    [HideInInspector] public int MaxHP;         //최대 체력
    protected int _currentBarrier;              //현재 보호막 수치
    [HideInInspector] public bool IsInjured;    //부상 상태 여부
    [HideInInspector] public bool IsChained;    //행동 불가 여부
    public List<BuffBase> ActiveBuffList =new List<BuffBase>(); //적용 버프 목록
    public Action OnBuffUpdate { get; set; }  //버프 갱신 액션

    //사망시 호출될 델리게이트
    public delegate Sequence OnDeadDelegate();
    public OnDeadDelegate OnDeath;

    private UI_UnitBuffBar _unitEffects;  //버프 표시 UI 관련 스크립트

    protected virtual void Awake()
    {
        _unitEffects = GetComponentInChildren<UI_UnitBuffBar>();
    }

    public virtual int GetCurrentBarrier()
    {
        return _currentBarrier;
    }

    /// <summary>
    /// amount만큼 베리어를 획득
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
    /// 베리어 수치를 특정 수치로 갱신
    /// </summary>
    /// <param name="amount"></param>
    public virtual void UpdateBarrier(float amount)
    {
        int intAmount = (int)amount;
        _currentBarrier = intAmount;
    }

    /// <summary>
    /// 이펙트 없이 체력을 최대치만큼 회복
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
    /// 회복 처리 코루틴
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
    /// 특정 이펙트와 함께 데미지를 받음, 대부분의 상황에서 호출되는 함수
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
    /// CardEffectManager에서 주로 호출하며, 현재 사용중인 카드 효과에 따른 이펙트와 함께 데미지를 받음
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public IEnumerator GetDamageCoroutine(float amount)
    {
        yield return new WaitForEndOfFrame();
        ApplyDamage(amount, CardEffectManager.CurrentCardData.DamageEffectType);
    }

    /// <summary>
    /// 데미지를 받는 과정을 처리하는 함수
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="effectType"></param>
    protected virtual void ApplyDamage(float amount, E_EffectType effectType)
    {
        // Calculate damage considering vulnerability
        int resultAmount = (int)(HasBuff(E_EffectType.Vulnerability) ? amount * 1.5f : amount);
        int effectiveBarrier = GetCurrentBarrier();

        //베리어 체크
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
                // 효과 제거
                ActiveBuffList.RemoveAt(i);
                OnBuffUpdate?.Invoke();
                break; // 버프를 하나만 제거한 후 종료
            }
        }
    }

    /// <summary>
    /// 특정 버프 아이콘 강조 효과
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
    /// 해당 버프를 amount스택만큼 받음
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public virtual IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        if (!IsAlive()) yield break;
        Debug.Log($"{gameObject.name} 에게 {type}을 {amount} 만큼 부여");
        ApplyBuff(BuffFactory.GetBuffByType(type, amount));
        VisualEffectManager.Inst.InstantiateEffect(type, this);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 해당 버프를 가지고 있는지 확인
    /// </summary>
    /// <param name="effectType"></param>
    /// <returns></returns>
    public bool HasBuff(E_EffectType effectType)
    {
        var effect = ActiveBuffList.FirstOrDefault(e => e.BuffType == effectType);
        return effect != null;
    }

    /// <summary>
    /// 해당 버프를 가지고 있는지 확인하며 out키워드로 해당 버프 반환
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
    /// 하나 이상의 디버프를 가지고 있는지 확인
    /// </summary>
    /// <returns></returns>
    public bool HasDebuff()
    {
        // ActiveEffectList에 IsDebuff가 true인 항목이 있는지 확인합니다.
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
    /// 사망 처리 코루틴
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator DeathCoroutine()
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
            if (spriteRenderer.GetComponent<DestroyAfterAnimation>() != null) continue;
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
    }

    //SpriteRenderer 페이드 아웃
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

    //Image 페이드 아웃
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

    //TMP_Text 페이드 아웃
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
