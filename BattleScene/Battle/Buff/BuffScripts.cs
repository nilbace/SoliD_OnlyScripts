using System;
using System.Collections.Generic;
using System.Linq;

public enum E_EffectType
{
    //버프 디버프 종류
    Crystallization, Blessing, Vulnerability, Weakening, Bleeding, Blade,
    BlueDragon, WhiteTiger, RedBird, BlackTortoise, Freeze,
    Lightning, Burn, Posion, HeadShot, SharpShooter, ReaperMark, DemonicWhisper, DarkMagic, SugarRush, CombatStance, Strength,

    //몬스터 전용 특수 버프들
    LightThirst,

    //카드 이펙트 관련
    Talisman_Attack, Blade_Attack, ShotGun_Attack, Talisman_Finisher, Talisman_RB, Talisman_BT, Talisman_BD, Talisman_WT, ShotGun_Finisher, Scythe_Attack, Staff_Attack,
    Black_Attack, Blade_Finisher, None,

    //카드 관련 효과들
    Interval, Damage, Energy, Shield, Heal, DrawCard, MakeCardToHand, CheckStatusEffect, DiscardRandomCard, CheckHasDebuff, SelfHarm, HealLowestHPAlly,
    AddRandomBullet,ShootBullet, AddRandomFourGods, DrainMagic, FourGodsJudgement, CatchingBreath, Stuntman, Overcome, LastShot, UnfairTrade, LastMercy,
    AddRandomBlackCard, AllOutAttack, SoSweet, SacrificeOfBlood, Purify, BloodySword, SilverDance, Shimai, RipWound, EZ,
}

//넘겨받은 타입에 따라 버프를 반환해주는 Static한 Factory
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

            case E_EffectType.Bleeding:
                return new Bleeding(amount);

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

            case E_EffectType.Freeze:
                return new Freeze(amount);

            case E_EffectType.Lightning:
                return new Lightning(amount);

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
    /// -1이라면 지속시간이 없는 타입
    /// </summary>
    public float Duration;
    public float Stack;
    public string InfoText;
    public bool isDealerBuff;
    public bool isDefensiveBuff;
    public bool IsDebuff = false;

    public BuffBase(E_EffectType effectType, float duration, float stack, string infoText)
    {
        BuffType = effectType;
        Duration = duration;
        Stack = stack;
        InfoText = infoText;
    }

    public abstract void ApplyEffect(UnitBase unit);

    //기본적으로 턴이 지나면 지속시간이 줄어듦
    public virtual void NextTurnStarted(UnitBase unit)
    {
        if (Duration > 1) Duration--;
        if (Duration == 1) unit.BuffList.Remove(this);
    }
    protected virtual void ApplyOrUpdateEffectByStack(UnitBase unit)
    {
        var existingEffect = unit.BuffList.FirstOrDefault(e => e.BuffType == this.BuffType);
        SortBuffList(unit);
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
        SortBuffList(unit);
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

    protected virtual void SortBuffList(UnitBase unit)
    {
        // 특정 버프의 우선순위를 정의
        var buffPriority = new List<E_EffectType>
    {
        E_EffectType.DemonicWhisper,
        E_EffectType.DarkMagic,
        E_EffectType.CombatStance,
        E_EffectType.Blade,
        E_EffectType.SharpShooter
    };

        unit.BuffList = unit.BuffList
            .OrderByDescending(buff => buffPriority.Contains(buff.BuffType)) // 특정 버프가 포함되면 우선
            .ThenBy(buff => buffPriority.IndexOf(buff.BuffType)) // 특정 버프의 우선순위에 따라 정렬
            .ThenByDescending(buff => buff.Duration < 0) // 영구 지속 버프를 우선
            .ThenByDescending(buff => buff.Duration > 0) // 그 다음 일시적 지속 버프
            .ToList();
    }


}

public class Crystallization : BuffBase
{
    public Crystallization(float stack) : base(E_EffectType.Crystallization, -1, stack,
        "방어도 획득 시, 해당 수치만큼 추가 방어도를 더함. 전투 내내 지속")
    { }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Blessing : BuffBase
{
    public Blessing(float stack) : base(E_EffectType.Blessing, -1, stack,
        "캐릭터 회복 시, 해당 수치만큼 추가 회복량을 더함. 전투 내내 지속")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Vulnerability : BuffBase
{
    public Vulnerability(float duration) : base(E_EffectType.Vulnerability, duration, -1,
        "적에게 피해를 받을 때 50%(소수점 버림)의 피해를 추가로 입는다. 다음 턴 시작시 1 감소")
    { IsDebuff = true;
        isDefensiveBuff = true;
    }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Weakening : BuffBase
{
    public Weakening(float duration) : base(E_EffectType.Weakening, duration, -1,
        "적에게 주는 피해량이 25%(소수점 버림)만큼 줄어든다. 다음 턴 시작시 1 감소")
    { IsDebuff = true; isDealerBuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByDuration(unit);
    }
}

public class Bleeding : BuffBase
{
    public Bleeding(float duration) : base(E_EffectType.Bleeding, duration, -1,
        "다음 턴 시작 시 1 감소한다.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
 }


public class Blade : BuffBase
{
    public Blade(float stack) : base(E_EffectType.Blade, -1, stack,
        "0 코스트 카드 사용 시 해당 수치만큼 추가 데미지를 가한다. 전투 내내 지속")
    { isDealerBuff = true; }

    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlueDragon : BuffBase
{
    public BlueDragon(float stack) : base(E_EffectType.BlueDragon, -1, stack,
        "청룡의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class WhiteTiger : BuffBase
{
    public WhiteTiger(float stack) : base(E_EffectType.WhiteTiger, -1, stack,
        "백호의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class RedBird : BuffBase
{
    public RedBird(float stack) : base(E_EffectType.RedBird, -1, stack,
        "주작의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class BlackTortoise : BuffBase
{
    public BlackTortoise(float stack) : base(E_EffectType.BlackTortoise, -1, stack,
        "현무의 각인")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Freeze : BuffBase
{
    public Freeze(float duration) : base(E_EffectType.Freeze, duration, -1,
        "얻는 쉴드량 50%감소, 턴마다 1감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Lightning : BuffBase
{
    public Lightning(float duration) : base(E_EffectType.Lightning, duration, -1,
        "방어막에 막히지 않는 공격에 스탯만큼의 추가딜, 턴마다 1 감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Burn : BuffBase
{
    public Burn(float duration) : base(E_EffectType.Burn, duration, -1,
        "치유량 50% 감소, 턴마다 1감소")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class Posion : BuffBase
{
    public Posion(float stack) : base(E_EffectType.Posion, -1, stack,
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
    public HeadShot(float duration) : base(E_EffectType.HeadShot, duration, -1,
        "총으로 받는 피해 50%증가 및 효과 지속시간 50%증가, 턴마다 1감소")
    { IsDebuff = true; isDefensiveBuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class SharpShooter : BuffBase
{
    public SharpShooter(float stack) : base(E_EffectType.SharpShooter, -1, stack,
        "총으로 가하는 피해 50%증가, 영구 지속")
    { isDealerBuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class ReaperMark : BuffBase
{
    public ReaperMark(float duration) : base(E_EffectType.ReaperMark, duration, -1,
        "사신의 표식이 있는 적을 처치하면 카드를 한장 드로우하고 에너지를 1 받습니다.")
    { IsDebuff = true; }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DemonicWhisper : BuffBase
{
    public DemonicWhisper(float stack) : base(E_EffectType.DemonicWhisper, -1, stack,
        "스태프 카드 사용시 덱에 어둠 카드 추가")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}


public class DarkMagic : BuffBase
{
    public DarkMagic(float stack) : base(E_EffectType.DarkMagic, -1, stack,
        "검정카드 사용 시 1 증가, 흑마력 스탯만큼 추가딜 들어가고 깎이는 HP 증가")
    { isDealerBuff = true;   }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class SugarRush : BuffBase
{
    public SugarRush(float stack) : base(E_EffectType.SugarRush, -1, stack,
        "디버프가 걸린 적을 공격할때마다 에너지를 1 돌려받는다.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class CombatStance : BuffBase
{
    public CombatStance(float stack) : base(E_EffectType.CombatStance, -1, stack,
        "매턴 시작 마다 '단검'을 패에 추가합니다.")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }

}

public class LightThirst : BuffBase
{
    public LightThirst(float stack) : base(E_EffectType.LightThirst, -1, stack,
        "빙결,감전,화상,중독을 부여당하면 해당 효과를 무시하고 HP5회복, 힘5상승")
    { }
    public override void ApplyEffect(UnitBase unit)
    {
        ApplyOrUpdateEffectByStack(unit);
    }
}

public class Strength : BuffBase
{
    public Strength(float stack) : base(E_EffectType.Strength, -1, stack,
        "스택당 가하는 피해 1 증가")
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