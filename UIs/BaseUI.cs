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
    public Image[] RelicIMGs;
    private void Awake()
    {
        Inst = this;
    }

    public void UpdateUIs()
    {
        TMP_Gold.text = GameManager.UserData.MoonStoneAmount.ToString();
    }

    public void MapBTN()
    {
        Map.MapView.Inst.MapBTN();
    }
}
