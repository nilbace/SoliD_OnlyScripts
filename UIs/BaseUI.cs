using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BaseUI : MonoBehaviour
{
    public TMP_Text TMP_Gold;
    public static BaseUI Inst;
    public Transform TR_RelicParent;
    public GameObject RelicGO;
    public List<RelicGO> RelicGOList;
    public TMP_Text TMP_DeckCount;
    public GameObject CardListPopup;

    private void Awake()
    {
        Inst = this;
    }

    public void UpdateUIs()
    {
        TMP_Gold.text = TrialManager.Inst.MoonStone.ToString();
        TMP_DeckCount.text = TrialManager.Inst.UserDeck.Count.ToString();
    }

    public void MapBTN()
    {
        Map.MapView.Inst.MapBTN();
    }

    public void DeckBTN()
    {
        UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.UserDeck);
    }

    /// <summary>
    /// TrialManager에서만 호출할 것
    /// </summary>
    /// <param name="relic"></param>
    public void AddRelicIcon(RelicBase relic)
    {
        var GO = Instantiate(RelicGO, TR_RelicParent);
        var relicGO = GO.GetComponent<RelicGO>();
        relicGO.ThisRelic = relic;
        RelicGOList.Add(relicGO);
    }

    public void TwinkleRelicIcon(int index)
    {
        TR_RelicParent.GetChild(index).GetComponent<RelicGO>().TwinkleIcon();
    }

    public void TwinkleRelicIcon(E_RelicType type)
    {
        int relicIdnex = 0;
        foreach (RelicBase relic in TrialManager.Inst.RelicList)
        {
            if (relic.RelicType == type) relicIdnex = TrialManager.Inst.RelicList.IndexOf(relic);
        }
        TwinkleRelicIcon(relicIdnex);
    }

    public void UpdateRelicStacks()
    {
        foreach (RelicGO relicgo in RelicGOList)
        {
            if (relicgo.ThisRelic.HasStack)
            {
                relicgo.UpdateStack();
            }
        }
    }
}
