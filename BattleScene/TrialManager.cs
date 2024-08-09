using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이번 전투에서 들고 있는 돈, 유물, 카드 등을 저장할 곳
/// </summary>
public class TrialManager : MonoBehaviour
{

    public static TrialManager Inst;

    #region Relic
    private List<RelicBase> RelicList;

    private void Awake()
    {
        Inst = this;
        RelicList = new List<RelicBase>();
    }
    public void AddRelic(E_RelicType relic)
    {
        var relicClass = RelicFactory.GetRelic(relic);
        BaseUI.Inst.RelicIMGs[RelicList.Count].sprite = IconContainer.Inst.GetRelicSprite(relic);
        RelicList.Add(relicClass);
    }

    
    #endregion

    #region Reward
    public int MoonStone;

    #endregion
    #region Deck
    public List<CardData> UserDeck = new List<CardData>();

    #endregion
}
