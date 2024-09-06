using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
/// <summary>
/// 몬스터의 공통 뼈대, 상속 받은 몬스터는 각자의 패턴과 공격 모션만 작성하면 됨
/// </summary>

public abstract class MonsterBase : UnitBase
{
    public int StartHP;
    public int NowIntentNumber;
    public SpriteRenderer SR_Intent;
    public List<SpriteRenderer> SR_PlayerInks;
    private E_CardColor[] _playerInks;
    private int _inkCount;

    public virtual void Start()
    {
        _playerInks = new E_CardColor[2];
        MaxHP = StartHP; SetHP_To_Max_WithoutVFX();
    }

    public virtual void SetIntent()
    {
        SR_Intent.DOFade(1, 0.4f);
    }

    public void HideIntent()
    {
        SR_Intent.DOFade(0, 0.4f);
    }

    protected override void ApplyDamage(float amount, E_EffectType effectType)
    {
        float damageMagnitude = TrialManager.Inst.HasRelic(E_RelicType.RippedDoll) ? 1.75f : 1.5f;
        int resultAmount = (int)(HasBuff(E_EffectType.Vulnerability) ? amount * damageMagnitude : amount);

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

    public IEnumerator AddInk(E_CardColor color)
    {
        if (!IsAlive()) yield break;
        Color nowColor = GetColor(color);

        // 색을 처음 칠한다면
        if (_inkCount == 0)
        {
            SR_PlayerInks[0].color = nowColor;
            _playerInks[0] = color;
            _inkCount++;
        }
        // 이미 색이 1종류 묻어 있다면
        else
        {
            // 같은 색이라면 무시
            if (color == _playerInks[0]) yield break;

            // 두 번째 색상을 추가
            SR_PlayerInks[1].color = nowColor;
            _playerInks[1] = color;
            _inkCount++;

            // 0.2초 대기
            yield return new WaitForSeconds(0.2f);

            // InkBurst 코루틴 실행
            yield return StartCoroutine(InkBurst());
        }
    }

    public IEnumerator InkBurst()
    {
        Debug.Log($"색{_playerInks[0]}과 {_playerInks[1]} 효과 발동");
        if(_playerInks[0]==E_CardColor.Black || _playerInks[1] == E_CardColor.Black)
        {
            Debug.Log("검은색 증발");
        }
        else
        {
            yield return StartCoroutine(InkSkillManager.Inst.UseInkSkill(_playerInks[0]));
            yield return StartCoroutine(InkSkillManager.Inst.UseInkSkill(_playerInks[1]));
        }

        ClearInk();
    }

    public void ClearInk()
    {
        _inkCount = 0;
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

    /// <summary>
    /// 몬스터 공격시 공격 관련 각종 버프 적용
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public IEnumerator AttackCoroutine(float amount)
    {
        float resultDamage = amount;
        foreach (BuffBase buff in ActiveBuffList)
        {
            if (buff.isDealerBuff)
            {
                switch (buff.BuffType)
                {
                    //약화 디버프
                    case E_EffectType.Weakening:
                        resultDamage = resultDamage * 3 / 4;
                        TwinkleBuffIcon(E_EffectType.Weakening);
                        yield return new WaitForSeconds(0.2f);
                        break;
                  
                    case E_EffectType.Strength:
                        resultDamage = resultDamage * 1.3f;
                        TwinkleBuffIcon(E_EffectType.Strength);
                        yield return new WaitForSeconds(0.2f);
                        break;
                }
            }
        }
        StartCoroutine(BattleManager.Inst.MonsterAttackPlayer(amount));
    }

    public IEnumerator ApplyEffectToPlayer(E_EffectType type, float amount)
    {
        return BattleManager.Inst.MonsterApplyEffect_To_Player(type, amount);
    }

    public virtual IEnumerator StartNowPattern()
    {
        return null;
    }

    public IEnumerator Speech(float time, string message)
    {
        GameObject sp = Instantiate(Resources.Load("Prefabs/SpeechBalloon") as GameObject, transform);
        sp.transform.localPosition = Vector3.zero;
        yield return StartCoroutine(sp.GetComponent<SpeechBallon>().SetUPSpeechBalloon(time, message));
    }
  }

