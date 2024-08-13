using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicReward : MonoBehaviour
{
    public static E_RelicType S_RelicType;
    public UnityEngine.UI.Image Image;
    public bool IsEnergyAddRelic;


    private void OnEnable()
    {
        Setup();
    }

    private void Setup()
    {
        Image.sprite = IconContainer.Inst.GetRelicSprite(S_RelicType);
    }

    public void OnClick()
    {
        //TrialManager.Inst.AddRelic(S_RelicType);
    }

}
