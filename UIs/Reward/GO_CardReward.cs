using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CardReward : MonoBehaviour
{
    public GameObject RewardCard;
    public GameObject RewardPannel;
    private void OnMouseUpAsButton()
    {
        Debug.Log($"유저 덱에 카드 {GetComponent<CardGO>().thisCardData.CardName} 추가");
        EndCardReward();
    }

    public void EndCardReward()
    {
        RewardCard.SetActive(false);
        RewardPannel.SetActive(true);
    }
}
