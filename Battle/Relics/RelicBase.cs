using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicBase
{
    public E_RelicType ThisRelicType;
    public virtual void BattleStart()
    {

    }
}

public class Elixir : RelicBase
{
    public Elixir()
    {
        ThisRelicType = E_RelicType.Elixir;
    }
    public override void BattleStart()
    {
        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
        {
            player.ApplyStatusEffect(E_EffectType.Strength, 1);
        }
    }
}
public class Tongue : RelicBase
{
    public Tongue()
    {
        ThisRelicType = E_RelicType.Tongue;
    }

    public override void BattleStart()
    {
        foreach (UnitBase player in BattleManager.Inst.MonsterUnits)
        {
            player.ApplyStatusEffect(E_EffectType.Vulnerability, 1);
        }
    }
}

public class Topaz : RelicBase
{
    public Topaz()
    {
        ThisRelicType = E_RelicType.Topaz;
    }

    public override void BattleStart()
    {
        BattleManager.Inst.GetPlayer(E_CharName.Minju).AddBarrier(10);
    }
}

public class Rose : RelicBase
{
    public Rose()
    {
        ThisRelicType = E_RelicType.BlackRose;
    }

    public override void BattleStart()
    {
        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
        {
            player.ApplyStatusEffect(E_EffectType.Thorn, 3);
        }
    }
}
