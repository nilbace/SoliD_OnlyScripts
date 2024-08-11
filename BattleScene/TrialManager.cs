using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �̹� �������� ��� �ִ� ��, ����, ī�� ���� ������ ��
/// </summary>
public class TrialManager : MonoBehaviour
{

    public static TrialManager Inst;

    #region Relic
    private List<RelicBase> _relicList;

    private void Awake()
    {
        Inst = this;
        _relicList = new List<RelicBase>();
    }
    public void AddRelic(E_RelicType relic)
    {
        //var relicClass = RelicFactory.GetRelic(relic);
        //BaseUI.Inst.RelicIMGs[RelicList.Count].sprite = IconContainer.Inst.GetRelicSprite(relic);
        //RelicList.Add(relicClass);
    }

    public bool HasRelic(E_RelicType relicType)
    {
        foreach (var relic in _relicList)
        {
            if (relic.RelicType == relicType)
            {
                return true; // Relic found
            }
        }
        return false; // Relic not found
    }

    
    #endregion

    #region Reward
    public int MoonStone;

    #endregion
    #region Deck
    public List<CardData> UserDeck = new List<CardData>();

    #endregion
}
