using System;
using System.Linq;

public enum E_BuffType
{
    Crystallization, Blessing, Vulnerability, Weakening, Bloodstain, Blade,
    BlueDragon, WhiteTiger, RedBird, BlackTortoise, Frost,
    Electrocution, Burn, Posion, HeadShot, SharpShooter, ReaperMark, DemonicWhisper, DarkMagic
}

public enum E_EffectType
{
    //버프 디버프 종류 위랑 똑같이 복붙하면 됨
    Crystallization, Blessing, Vulnerability, Weakening, Bloodstain, Blade,
    BlueDragon, WhiteTiger, RedBird, BlackTortoise, Frost,
    Electrocution, Burn, Posion, HeadShot, SharpShooter, ReaperMark, DemonicWhisper, DarkMagic,


    //카드 관련 특수 효과들
    Interval, Damage, Energy, Shield, Heal, Black, DrawCard, MakeCardToHand, CheckStatusEffect, DiscardRandomCard, CheckHasDebuff, SelfHarm, HealLowestHPAlly,
    AddRandomBullet,ShootBullet, AddRandomFourGods, DrainMagic, FourGodsJudgement, CatchingBreath, Stuntman, Overcome, LastShot, UnfairTrade, LastMercy,
    AddRandomBlackCard,
}
public class BuffFactory
{
    public static BuffBase GetBuffByType(E_BuffType buffType, float amount)
    {
        switch (buffType)
        {
            case E_BuffType.Crystallization:
                return new Crystallization(amount);

            case E_BuffType.Blessing:
                return new Blessing(amount);

            case E_BuffType.Vulnerability:
                return new Vulnerability(amount);

            case E_BuffType.Weakening:
                return new Weakening(amount);


            case E_BuffType.Bloodstain:
                return new Bloodstain(amount);



            case E_BuffType.Blade:
                return new Blade(amount);



            case E_BuffType.BlueDragon:
                return new BlueDragon(amount);

            case E_BuffType.WhiteTiger:
                return new WhiteTiger(amount);

            case E_BuffType.RedBird:
                return new RedBird(amount);

            case E_BuffType.BlackTortoise:
                return new BlackTortoise(amount);

            case E_BuffType.Frost:
                return new Frost(amount);

            case E_BuffType.Electrocution:
                return new Electrocution(amount);

            case E_BuffType.Burn:
                return new Burn(amount);

            case E_BuffType.Posion:
                return new Posion(amount);

            case E_BuffType.HeadShot:
                return new HeadShot(amount);

            case E_BuffType.SharpShooter:
                return new SharpShooter(amount);

            case E_BuffType.ReaperMark:
                return new ReaperMark(amount);


            case E_BuffType.DemonicWhisper:
                return new DemonicWhisper(amount);

            case E_BuffType.DarkMagic:
                return new DarkMagic(amount);

            default:
                throw new ArgumentException($"Unknown buff type: {buffType}");
        }
    }
}

[System.Serializable]
public abstract class BuffBase
{
    public E_BuffType BuffType;
    /// <summary>
    /// -1이라면 지속시간이 없는 타입
    /// </summary>
    public float Duration;
    public float Stack;
    public string InfoText;
    public bool IsDebuff = false;

    public BuffBase(E_BuffType effectType, float duration, float stack, string infoText)
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
    public Crystallization(float stack) : base(E_BuffType.Crystallization, -1, stack,
        "방어도 획득 시, 해당 수치만큼 추가 방어도를 더함. 전투 내내 지속")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Blessing : BuffBase
{
    public Blessing(float stack) : base(E_BuffType.Blessing, -1, stack,
        "캐릭터 회복 시, 해당 수치만큼 추가 회복량을 더함. 전투 내내 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Vulnerability : BuffBase
{
    public Vulnerability(float duration) : base(E_BuffType.Vulnerability, duration, -1,
        "적에게 피해를 받을 때 50%(소수점 버림)의 피해를 추가로 입는다. 다음 턴 시작시 1 감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Weakening : BuffBase
{
    public Weakening(float duration) : base(E_BuffType.Weakening, duration, -1,
        "적에게 주는 피해량이 25%(소수점 버림)만큼 줄어든다. 다음 턴 시작시 1 감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Bloodstain : BuffBase
{
    public Bloodstain(float stack) : base(E_BuffType.Bloodstain, -1, stack,
        "적에게 공격 피해를 받으면 공격 대상에게 가시 수치만큼 피해를 준다. 전투 내내 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

    public override void NextTurnStarted(UnitBase unit)
    {
        unit.NowHp -= Stack;
        unit.BuffList.Remove(this);
    }
}


public class Blade : BuffBase
{
    public Blade(float stack) : base(E_BuffType.Blade, -1, stack,
        "0 코스트 카드 사용 시 해당 수치만큼 추가 데미지를 가한다. 전투 내내 지속")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlueDragon : BuffBase
{
    public BlueDragon(float stack) : base(E_BuffType.BlueDragon, -1, stack,
        "청룡의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class WhiteTiger : BuffBase
{
    public WhiteTiger(float stack) : base(E_BuffType.WhiteTiger, -1, stack,
        "백호의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class RedBird : BuffBase
{
    public RedBird(float stack) : base(E_BuffType.RedBird, -1, stack,
        "주작의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlackTortoise : BuffBase
{
    public BlackTortoise(float stack) : base(E_BuffType.BlackTortoise, -1, stack,
        "현무의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Frost : BuffBase
{
    public Frost(float duration) : base(E_BuffType.Frost, duration, -1,
        "얻는 쉴드량 50%감소, 턴마다 1감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Electrocution : BuffBase
{
    public Electrocution(float duration) : base(E_BuffType.Electrocution, duration, -1,
        "방어막에 막히지 않는 공격에 스탯만큼의 추가딜, 턴마다 1 감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Burn : BuffBase
{
    public Burn(float duration) : base(E_BuffType.Burn, duration, -1,
        "치유량 50% 감소, 턴마다 1감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class Posion : BuffBase
{
    public Posion(float stack) : base(E_BuffType.Posion, -1, stack,
        "턴 시작 시, 수치만큼 피해를 받는다. 다음 턴 시작 시 1 감소한다. ")
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
    public HeadShot(float duration) : base(E_BuffType.HeadShot, duration, -1,
        "총으로 받는 피해 50%증가 및 효과 지속시간 50%증가, 턴마다 1감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class SharpShooter : BuffBase
{
    public SharpShooter(float stack) : base(E_BuffType.SharpShooter, -1, stack,
        "총으로 가하는 피해 50%증가, 영구 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class ReaperMark : BuffBase
{
    public ReaperMark(float duration) : base(E_BuffType.ReaperMark, duration, -1,
        "사신의 표식이 있는 적을 처치하면 카드를 한장 드로우하고 에너지를 1 받습니다.")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DemonicWhisper : BuffBase
{
    public DemonicWhisper(float stack) : base(E_BuffType.DemonicWhisper, -1, stack,
        "스태프 카드 사용시 덱에 어둠 카드 추가")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DarkMagic : BuffBase
{
    public DarkMagic(float stack) : base(E_BuffType.DarkMagic, -1, stack,
        "검정카드 사용 시 1 증가, 흑마력 스탯만큼 추가딜 들어가고 깎이는 HP 증가")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

//적에게 중독 부여 파워 로직(구 잠식)
//public class Despair : EffectBase
//{
//    public Despair(float stack) : base(E_EffectType.Despair, -1, stack,
//        "턴 종료 시, 수치만큼 모든 적에게 잠식을 수치만큼 가한다. 전투 내내 지속")
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