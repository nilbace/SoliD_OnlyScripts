using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CardReward : MonoBehaviour
{
    public GameObject RewardCard;
    public GameObject RewardPannel;
    private void OnMouseUpAsButton()
    {
        Debug.Log($"���� ���� ī�� {GetComponent<CardGO>().thisCardData.CardName} �߰�");
        EndCardReward();
    }

    public void EndCardReward()
    {
        RewardCard.SetActive(false);
        RewardPannel.SetActive(true);
    }
}
