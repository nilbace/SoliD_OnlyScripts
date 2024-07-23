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
    /// -1�̶�� ���ӽð��� ���� Ÿ��
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
        "������ �ִ� ���ط��� �� ��ġ��ŭ �����Ѵ�. ���� ���� ����") { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Crystallization : EffectBase
{
    public Crystallization(float stack) : base(E_EffectType.Crystallization, -1, stack, 
        "�� ȹ�� ��, �ش� ��ġ��ŭ �߰� ���� ����. ���� ���� ����") { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Blessing : EffectBase
{
    public Blessing(float stack) : base(E_EffectType.Blessing, -1, stack,
        "ĳ���� ȸ�� ��, �ش� ��ġ��ŭ �߰� ȸ������ ����. ���� ���� ����") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Vulnerability : EffectBase
{
    public Vulnerability(float duration) : base(E_EffectType.Vulnerability, duration, -1,
        "������ ���ظ� ���� �� 50%(�Ҽ��� ����)�� ���ظ� �߰��� �Դ´�. ���� �� ���۽� 1 ����") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Weakening : EffectBase
{
    public Weakening(float duration) : base(E_EffectType.Weakening, duration, -1,
        "������ �ִ� ���ط��� 25%(�Ҽ��� ����)��ŭ �پ���. ���� �� ���۽� 1 ����") { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Thorn : EffectBase
{
    public Thorn(float stack) : base(E_EffectType.Thorn, -1, stack,
        "������ ���� ���ظ� ������ ���� ��󿡰� ���� ��ġ��ŭ ���ظ� �ش�. ���� ���� ����"){ }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Bloodstain : EffectBase
{
    public Bloodstain(float stack) : base(E_EffectType.Bloodstain, -1, stack,
        "������ ���� ���ظ� ������ ���� ��󿡰� ���� ��ġ��ŭ ���ظ� �ش�. ���� ���� ����")
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
        "�ش� ��ġ��ŭ�� �� ���� ��� Ȥ�� ȸ�� ī�带 ����� �� ����. ���� �� ���� �� 1 ����")
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
        "�� ���� ��, ��ġ��ŭ ���ظ� �޴´�. ���� �� ���� �� 1 �����Ѵ�. ")
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
        "0 �ڽ�Ʈ ī�� ��� �� �ش� ��ġ��ŭ �߰� �������� ���Ѵ�. ���� ���� ����")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BulletMark : EffectBase
{
    public BulletMark(float duration) : base(E_EffectType.BulletMark, duration, -1,
        "ȸ������ 50%(�Ҽ��� ����)��ŭ �پ���. ���� �� ���� �� 1 ����")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Injury : EffectBase
{
    public Injury(int stack) : base(E_EffectType.Injury, -1, stack,
        "�ൿ�Ұ�. ���� �� ���� �� 1 ����")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Concussion : EffectBase
{
    public Concussion(float stack) : base(E_EffectType.Concussion, -1, stack,
        "3ȸ ��ø�� ��, '�λ�'���� ��ȯ�ȴ�. ��ȯ ���Ŀ��� ��ø�� ���� �������.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Despair : EffectBase
{
    public Despair(float stack) : base(E_EffectType.Despair, -1, stack,
        "�� ���� ��, ��ġ��ŭ ��� ������ ����� ��ġ��ŭ ���Ѵ�. ���� ���� ����")
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
        "�� ���� ��, ��ġ��ŭ ���� �����Ѵ�. ")
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
        "�� ���� ��, ��ġ��ŭ ���ظ� �޴´�. ���� �� ���� �� 1 �����Ѵ�. ")
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
