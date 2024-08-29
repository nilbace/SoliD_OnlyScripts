using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CardReward : MonoBehaviour
{
    public GameObject RewardCard;
    public GameObject RewardPannel;
    private void OnMouseUpAsButton()
    {
        TrialManager.Inst.AddCard(GetComponent<CardGO>().thisCardData);
        EndCardReward();
    }

    public void EndCardReward()
    {
        RewardCard.SetActive(false);
        RewardPannel.SetActive(true);
    }
}
