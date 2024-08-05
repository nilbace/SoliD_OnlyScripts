using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum E_RelicType { Elixir, Tongue, Topaz, BlackRose, MaxCount }
public enum E_RelicKorName { 분노의_영약, 배신자의_혀, 황수정의_가호, 검은_장미 }
public class RelicManager : MonoBehaviour
{
    private List<RelicBase> RelicList;
    public static RelicManager Inst;
    public Action OnBattleStart;
    public Sprite[] RelicIcons;

    private void Awake()
    {
        Inst = this;
        RelicList = new List<RelicBase>();
    }
    public void AddRelic(E_RelicType relic)
    {
        var relicClass = RelicFactory.GetRelic(relic);
        BaseUI.Inst.RelicIMGs[RelicList.Count].sprite = GetRelicSprite(relic);
        RelicList.Add(relicClass);
        OnBattleStart += relicClass.BattleStart;
    }

    public Sprite GetRelicSprite(E_RelicType relic)
    {
        return RelicIcons[(int)relic];
    }

    [ContextMenu("혀 추가")]
    public void AddTongue()
    {
        AddRelic(E_RelicType.Tongue);
    }
}

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