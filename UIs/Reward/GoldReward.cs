using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldReward : MonoBehaviour
{
    public static int GoldRewardAmount;

    public void OnClick()
    {
        TrialManager.Inst.AddMoonStone(GoldRewardAmount);
    }
}
