using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicReward : MonoBehaviour
{
    public static E_RelicType S_RelicType;
    public UnityEngine.UI.Image Image;

    private void OnEnable()
    {
        Setup();
    }

    private void Setup()
    {
        Image.sprite = RelicManager.Inst.GetRelicSprite(S_RelicType);
    }

    public void OnClick()
    {
        RelicManager.Inst.AddRelic(S_RelicType);
    }

}
