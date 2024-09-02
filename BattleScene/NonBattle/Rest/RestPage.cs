using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestPage : MonoBehaviour
{
    public Slider[] HP_Sliders;

    private void OnEnable()
    {
        for (int i = 0; i < 3; i++)
        {
            HP_Sliders[i].UpdateHP(BattleManager.Inst.GetPlayer((E_CharName)(i + 1)));
        }
    }
}
