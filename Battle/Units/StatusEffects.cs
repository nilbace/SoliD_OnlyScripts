using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectFactory
{
    public static EffectBase GetEffectByType(E_EffectType effectType, float amount)
    {
        switch (effectType)
        {
            case E_EffectType.Strength:
                return new Strength(amount);

            case E_EffectType.Crystallization:
                return new Crystallization(amount);

            case E_EffectType.Blessing:
                return new Blessing(amount);

            case E_EffectType.Vulnerability:
                return new Vulnerability(amount);

            case E_EffectType.Weakening:
                return new Weakening(amount);

            case E_EffectType.Thorn:
                return new Thorn(amount);

            case E_EffectType.Bloodstain:
                return new Bloodstain(amount);

            case E_EffectType.Chain:
                return new Chain(amount);

            case E_EffectType.Encroachment:
                return new Encroachment(amount);

            case E_EffectType.Blade:
                return new Blade(amount);

            case E_EffectType.BulletMark:
                return new BulletMark(amount);

            case E_EffectType.Injury:
                return new Injury((int)amount); // Assuming Injury uses an int for stack

            case E_EffectType.Concussion:
                return new Concussion(amount);

            case E_EffectType.Despair:
                return new Despair(amount);

            case E_EffectType.MuscleLoss:
                return new MuscleLoss(amount);

            case E_EffectType.Scabbard:
                return new Scabbard((int)amount); // Assuming Scabbard uses an int for stack

            default:
                throw new ArgumentException($"Unknown effect type: {effectType}");
        }
    }
}

[System.Serializable]
public abstract class EffectBase
{
    public E_EffectType ThisEffectType;
    /// <summary>
    /// -1이라면 지속시간이 없는 타입
    /// </summary>
    public float Duration;
    public float Stack;
    public string InfoText;

    public EffectBase(E_EffectType effectType, float duration, float stack, string infoText)
    {
        ThisEffectType = effectType;
        Duration = duration;
        Stack = stack;
        InfoText = infoText;
    }

    public abstract void ApplyEffect(UnitBase unit);

    public virtual void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 1) Duration--;
        if (Duration == 1) unit.ActiveEffectList.Remove(this);
    }
    protected virtual void ApplyOrUpdateEffectByStack(UnitBase unit)
    {
        var existingEffect = unit.ActiveEffectList.FirstOrDefault(e => e.ThisEffectType == this.ThisEffectType);

        if (existingEffect != null)
        {
            existingEffect.Stack += this.Stack;
            if (existingEffect.Stack == 0)
            {
                unit.ActiveEffectList.Remove(existingEffect);
            }
        }
        else
        {
            unit.ActiveEffectList.Add(this);
        }
    }

    protected virtual void ApplyOrUpdateEffectByDuration(UnitBase unit)
    {
        var existingEffect = unit.ActiveEffectList.FirstOrDefault(e => e.ThisEffectType == this.ThisEffectType);

        if (existingEffect != null)
        {
            existingEffect.Duration += this.Duration;
            if (existingEffect.Duration == 0)
            {
                unit.ActiveEffectList.Remove(existingEffect);
            }
        }
        else
        {
            unit.ActiveEffectList.Add(this);
        }
    }
}

public class Strength : EffectBase
{
    public Strength(float stack) : base(E_EffectType.Strength, -1, stack , 
        "적에게 주는 피해량이 힘 수치만큼 증가한다. 전투 내내 지속") { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Crystallization : EffectBase
{
    public Crystallization(float stack) : base(E_EffectType.Crystallization, -1, stack, 
        "방어도 획득 시, 해당 수치만큼 추가 방어도를 더함. 전투 내내 지속") { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Blessing : EffectBase
{
    public Blessing(float stack) : base(E_EffectType.Blessing, -1, stack,
        "캐릭터 회복 시, 해당 수치만큼 추가 회복량을 더함. 전투 내내 지속") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Vulnerability : EffectBase
{
    public Vulnerability(float duration) : base(E_EffectType.Vulnerability, duration, -1,
        "적에게 피해를 받을 때 50%(소수점 버림)의 피해를 추가로 입는다. 다음 턴 시작시 1 감소") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Weakening : EffectBase
{
    public Weakening(float duration) : base(E_EffectType.Weakening, duration, -1,
        "적에게 주는 피해량이 25%(소수점 버림)만큼 줄어든다. 다음 턴 시작시 1 감소") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Thorn : EffectBase
{
    public Thorn(float stack) : base(E_EffectType.Thorn, -1, stack,
        "적에게 공격 피해를 받으면 공격 대상에게 가시 수치만큼 피해를 준다. 전투 내내 지속"){ }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Bloodstain : EffectBase
{
    public Bloodstain(float stack) : base(E_EffectType.Bloodstain, -1, stack,
        "적에게 공격 피해를 받으면 공격 대상에게 가시 수치만큼 피해를 준다. 전투 내내 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        unit.NowHp -= Stack;
        unit.ActiveEffectList.Remove(this);
    }
}

public class Chain : EffectBase
{
    public Chain(float duration) : base(E_EffectType.Chain, duration, -1,
        "해당 수치만큼의 턴 동안 방어 혹은 회복 카드를 사용할 수 없다. 다음 턴 시작 시 1 감소")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
        unit.IsChained = true;
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 0) Duration--;
        if (Duration == 0)
        {
            unit.ActiveEffectList.Remove(this);
            unit.IsChained = false;
        }
    }
}

public class Encroachment : EffectBase
{
    public Encroachment(float duration) : base(E_EffectType.Encroachment, duration, -1,
        "턴 시작 시, 수치만큼 피해를 받는다. 다음 턴 시작 시 1 감소한다. ")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 0)
        {
            unit.GetDamage(Duration);
            Duration--;
            if (Duration == 0)
            {
                unit.ActiveEffectList.Remove(this);
                unit.IsChained = false;
            }
        }
        
    }
}

public class Blade : EffectBase
{
    public Blade(float stack) : base(E_EffectType.Blade, -1, stack,
        "0 코스트 카드 사용 시 해당 수치만큼 추가 데미지를 가한다. 전투 내내 지속")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BulletMark : EffectBase
{
    public BulletMark(float duration) : base(E_EffectType.BulletMark, duration, -1,
        "회복력이 50%(소수점 버림)만큼 줄어든다. 다음 턴 시작 시 1 감소")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Injury : EffectBase
{
    public Injury(int stack) : base(E_EffectType.Injury, -1, stack,
        "행동불가. 다음 턴 시작 시 1 감소")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Concussion : EffectBase
{
    public Concussion(float stack) : base(E_EffectType.Concussion, -1, stack,
        "3회 중첩될 시, '부상'으로 변환된다. 변환 이후에는 중첩된 값이 사라진다.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Despair : EffectBase
{
    public Despair(float stack) : base(E_EffectType.Despair, -1, stack,
        "턴 종료 시, 수치만큼 모든 적에게 잠식을 수치만큼 가한다. 전투 내내 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        if(unit is MonsterBase)
        {
            foreach (MonsterBase mon in BattleManager.Inst.PlayerUnits)
            {
                mon.ApplyStatusEffect(E_EffectType.Encroachment, Stack);
            }
        }
        else
        {
            foreach(MonsterBase mon in BattleManager.Inst.MonsterUnits)
            {
                mon.ApplyStatusEffect(E_EffectType.Encroachment, Stack);
            }
        }
    }
}

public class MuscleLoss : EffectBase
{
    public MuscleLoss(float stack) : base(E_EffectType.MuscleLoss, -1, stack,
        "턴 종료 시, 수치만큼 힘이 감소한다. ")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        unit.ApplyStatusEffect(E_EffectType.Strength, -1);
    }
}

public class Scabbard : EffectBase
{
    public Scabbard(int stack) : base(E_EffectType.Scabbard, -1, stack,
        "턴 시작 시, 수치만큼 피해를 받는다. 다음 턴 시작 시 1 감소한다. ")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        unit.NowHp--;
    }
}
