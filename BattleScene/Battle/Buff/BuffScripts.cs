using System;
using System.Linq;

public enum E_EffectType
{
    //���� ����� ���� ���� �Ȱ��� �����ϸ� ��
    Crystallization, Blessing, Vulnerability, Weakening, Bloodstain, Blade,
    BlueDragon, WhiteTiger, RedBird, BlackTortoise, Frost,
    Electrocution, Burn, Posion, HeadShot, SharpShooter, ReaperMark, DemonicWhisper, DarkMagic, SugarRush, CombatStance, Strength,

    //���� ���� Ư�� ������
    LightThirst,


    //ī�� ���� Ư�� ȿ����
    Interval, Damage, Energy, Shield, Heal, Black, DrawCard, MakeCardToHand, CheckStatusEffect, DiscardRandomCard, CheckHasDebuff, SelfHarm, HealLowestHPAlly,
    AddRandomBullet,ShootBullet, AddRandomFourGods, DrainMagic, FourGodsJudgement, CatchingBreath, Stuntman, Overcome, LastShot, UnfairTrade, LastMercy,
    AddRandomBlackCard, AllOutAttack, SoSweet, SacrificeOfBlood, Purify, BloodySword, SilverDance, Shimai, RipWound, EZ,
}
public class BuffFactory
{
    public static BuffBase GetBuffByType(E_EffectType buffType, float amount)
    {
        switch (buffType)
        {
            case E_EffectType.Crystallization:
                return new Crystallization(amount);

            case E_EffectType.Blessing:
                return new Blessing(amount);

            case E_EffectType.Vulnerability:
                return new Vulnerability(amount);

            case E_EffectType.Weakening:
                return new Weakening(amount);

            case E_EffectType.Bloodstain:
                return new Bloodstain(amount);

            case E_EffectType.Blade:
                return new Blade(amount);

            case E_EffectType.BlueDragon:
                return new BlueDragon(amount);

            case E_EffectType.WhiteTiger:
                return new WhiteTiger(amount);

            case E_EffectType.RedBird:
                return new RedBird(amount);

            case E_EffectType.BlackTortoise:
                return new BlackTortoise(amount);

            case E_EffectType.Frost:
                return new Frost(amount);

            case E_EffectType.Electrocution:
                return new Electrocution(amount);

            case E_EffectType.Burn:
                return new Burn(amount);

            case E_EffectType.Posion:
                return new Posion(amount);

            case E_EffectType.HeadShot:
                return new HeadShot(amount);

            case E_EffectType.SharpShooter:
                return new SharpShooter(amount);

            case E_EffectType.ReaperMark:
                return new ReaperMark(amount);

            case E_EffectType.DemonicWhisper:
                return new DemonicWhisper(amount);

            case E_EffectType.DarkMagic:
                return new DarkMagic(amount);

            case E_EffectType.SugarRush:
                return new SugarRush(amount);

            case E_EffectType.CombatStance:
                return new CombatStance(amount);

            case E_EffectType.Strength:
                return new Strength(amount);

            case E_EffectType.LightThirst:
                return new LightThirst(amount);

            default:
                throw new ArgumentException($"Unknown buff type: {buffType}");
        }
    }
}

[System.Serializable]
public abstract class BuffBase
{
    public E_EffectType BuffType;
    /// <summary>
    /// -1�̶�� ���ӽð��� ���� Ÿ��
    /// </summary>
    public float Duration;
    public float Stack;
    public string InfoText;
    public bool IsDebuff = false;

    public BuffBase(E_EffectType effectType, float duration, float stack, string infoText)
    {
        BuffType = effectType;
        Duration = duration;
        Stack = stack;
        InfoText = infoText;
    }

    public abstract void ApplyEffect(UnitBase unit);

    public virtual void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 1) Duration--;
        if (Duration == 1) unit.BuffList.Remove(this);
    }
    protected virtual void ApplyOrUpdateEffectByStack(UnitBase unit)
    {
        var existingEffect = unit.BuffList.FirstOrDefault(e => e.BuffType == this.BuffType);

        if (existingEffect != null)
        {
            existingEffect.Stack += this.Stack;
            if (existingEffect.Stack == 0)
            {
                unit.BuffList.Remove(existingEffect);
            }
        }
        else
        {
            unit.BuffList.Add(this);
        }
    }

    protected virtual void ApplyOrUpdateEffectByDuration(UnitBase unit)
    {
        var existingEffect = unit.BuffList.FirstOrDefault(e => e.BuffType == this.BuffType);

        if (existingEffect != null)
        {
            existingEffect.Duration += this.Duration;
            if (existingEffect.Duration == 0)
            {
                unit.BuffList.Remove(existingEffect);
            }
        }
        else
        {
            unit.BuffList.Add(this);
        }
    }
}

public class Crystallization : BuffBase
{
    public Crystallization(float stack) : base(E_EffectType.Crystallization, -1, stack,
        "�� ȹ�� ��, �ش� ��ġ��ŭ �߰� ���� ����. ���� ���� ����")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Blessing : BuffBase
{
    public Blessing(float stack) : base(E_EffectType.Blessing, -1, stack,
        "ĳ���� ȸ�� ��, �ش� ��ġ��ŭ �߰� ȸ������ ����. ���� ���� ����")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Vulnerability : BuffBase
{
    public Vulnerability(float duration) : base(E_EffectType.Vulnerability, duration, -1,
        "������ ���ظ� ���� �� 50%(�Ҽ��� ����)�� ���ظ� �߰��� �Դ´�. ���� �� ���۽� 1 ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Weakening : BuffBase
{
    public Weakening(float duration) : base(E_EffectType.Weakening, duration, -1,
        "������ �ִ� ���ط��� 25%(�Ҽ��� ����)��ŭ �پ���. ���� �� ���۽� 1 ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Bloodstain : BuffBase
{
    public Bloodstain(float duration) : base(E_EffectType.Bloodstain, duration, -1,
        "���� �� ���� �� 1 �����Ѵ�.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
 }


public class Blade : BuffBase
{
    public Blade(float stack) : base(E_EffectType.Blade, -1, stack,
        "0 �ڽ�Ʈ ī�� ��� �� �ش� ��ġ��ŭ �߰� �������� ���Ѵ�. ���� ���� ����")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlueDragon : BuffBase
{
    public BlueDragon(float stack) : base(E_EffectType.BlueDragon, -1, stack,
        "û���� ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class WhiteTiger : BuffBase
{
    public WhiteTiger(float stack) : base(E_EffectType.WhiteTiger, -1, stack,
        "��ȣ�� ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class RedBird : BuffBase
{
    public RedBird(float stack) : base(E_EffectType.RedBird, -1, stack,
        "������ ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlackTortoise : BuffBase
{
    public BlackTortoise(float stack) : base(E_EffectType.BlackTortoise, -1, stack,
        "������ ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Frost : BuffBase
{
    public Frost(float duration) : base(E_EffectType.Frost, duration, -1,
        "��� ���差 50%����, �ϸ��� 1����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Electrocution : BuffBase
{
    public Electrocution(float duration) : base(E_EffectType.Electrocution, duration, -1,
        "���� ������ �ʴ� ���ݿ� ���ȸ�ŭ�� �߰���, �ϸ��� 1 ����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Burn : BuffBase
{
    public Burn(float duration) : base(E_EffectType.Burn, duration, -1,
        "ġ���� 50% ����, �ϸ��� 1����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class Posion : BuffBase
{
    public Posion(float stack) : base(E_EffectType.Posion, -1, stack,
        "�� ���� ��, ��ġ��ŭ ���ظ� �޴´�. ���� �� ���� �� 1 �����Ѵ�. ")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 0) Duration--;
        if (Duration == 0)
        {
            unit.BuffList.Remove(this);
            unit.IsChained = false;
        }
    }
}


public class HeadShot : BuffBase
{
    public HeadShot(float duration) : base(E_EffectType.HeadShot, duration, -1,
        "������ �޴� ���� 50%���� �� ȿ�� ���ӽð� 50%����, �ϸ��� 1����")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class SharpShooter : BuffBase
{
    public SharpShooter(float stack) : base(E_EffectType.SharpShooter, -1, stack,
        "������ ���ϴ� ���� 50%����, ���� ����")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class ReaperMark : BuffBase
{
    public ReaperMark(float duration) : base(E_EffectType.ReaperMark, duration, -1,
        "����� ǥ���� �ִ� ���� óġ�ϸ� ī�带 ���� ��ο��ϰ� �������� 1 �޽��ϴ�.")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DemonicWhisper : BuffBase
{
    public DemonicWhisper(float stack) : base(E_EffectType.DemonicWhisper, -1, stack,
        "������ ī�� ���� ���� ��� ī�� �߰�")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DarkMagic : BuffBase
{
    public DarkMagic(float stack) : base(E_EffectType.DarkMagic, -1, stack,
        "����ī�� ��� �� 1 ����, �渶�� ���ȸ�ŭ �߰��� ���� ���̴� HP ����")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class SugarRush : BuffBase
{
    public SugarRush(float stack) : base(E_EffectType.SugarRush, -1, stack,
        "������� �ɸ� ���� �����Ҷ����� �������� 1 �����޴´�.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class CombatStance : BuffBase
{
    public CombatStance(float stack) : base(E_EffectType.CombatStance, -1, stack,
        "���� ���� ���� '�ܰ�'�� �п� �߰��մϴ�.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetCardDataByIndex(87));
    }
}

public class LightThirst : BuffBase
{
    public LightThirst(float stack) : base(E_EffectType.LightThirst, -1, stack,
        "����,����,ȭ��,�ߵ��� �ο����ϸ� �ش� ȿ���� �����ϰ� HP5ȸ��, ��5���")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Strength : BuffBase
{
    public Strength(float stack) : base(E_EffectType.Strength, -1, stack,
        "���ô� ���ϴ� ���� 1 ����")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

//������ �ߵ� �ο� �Ŀ� ����(�� ���)
//public class Despair : EffectBase
//{
//    public Despair(float stack) : base(E_EffectType.Despair, -1, stack,
//        "�� ���� ��, ��ġ��ŭ ��� ������ ����� ��ġ��ŭ ���Ѵ�. ���� ���� ����")
//    { }
//    public override void ApplyEffect(UnitBase unit)
//    {
//        ApplyOrUpdateEffectByStack(unit);
//    }

//    public override void NextTurnStarted(UnitBase unit)
//    {
//        if(unit is MonsterBase)
//        {
//            foreach (MonsterBase mon in BattleManager.Inst.PlayerUnits)
//            {
//                //mon.ApplyStatusEffect(E_EffectType.Encroachment, Stack);
//            }
//        }
//        else
//        {
//            foreach(MonsterBase mon in BattleManager.Inst.MonsterUnits)
//            {
//                //mon.ApplyStatusEffect(E_EffectType.Encroachment, Stack);
//            }
//        }
//    }
//}