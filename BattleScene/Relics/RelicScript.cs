using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_RelicType { Elixir, Tongue, Topaz, BlackRose, MaxCount }
public enum E_RelicKorName { 분노의_영약, 배신자의_혀, 황수정의_가호, 검은_장미 }
public class RelicFactory
{
    public static RelicBase GetRelic(E_RelicType relicType)
    {
        switch (relicType)
        {
            case E_RelicType.Elixir:
                return new Elixir();

            case E_RelicType.Tongue:
                return new Tongue();

            case E_RelicType.Topaz:
                return new Topaz();

            case E_RelicType.BlackRose:
                return new Rose();

        }

        Debug.Log("유물 없음");
        return null;
    }
}
public abstract class RelicBase
{
    public E_RelicType ThisRelicType;
    public abstract void ActiveEffect();
}

public class Elixir : RelicBase
{
    public Elixir()
    {
        ThisRelicType = E_RelicType.Elixir;
    }
    public override void ActiveEffect()
    {
        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
        {
            //player.ApplyStatusEffect(E_EffectType.Strength, 1);
        }
    }
}
public class Tongue : RelicBase
{
    public Tongue()
    {
        ThisRelicType = E_RelicType.Tongue;
    }

    public override void ActiveEffect()
    {
        foreach (UnitBase player in BattleManager.Inst.MonsterUnits)
        {
            player.ApplyBuff(E_EffectType.Vulnerability, 1);
        }
    }
}

public class Topaz : RelicBase
{
    public Topaz()
    {
        ThisRelicType = E_RelicType.Topaz;
    }

    public override void ActiveEffect()
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

    public override void ActiveEffect()
    {
        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
        {
            //player.ApplyStatusEffect(E_EffectType.Thorn, 3);
        }
    }
}
