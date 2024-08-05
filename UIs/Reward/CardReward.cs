using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardReward : MonoBehaviour
{
    public static float RareCardProbability;
    public GameObject[] CardGOs;
    public void ExitCardReward()
    {

    }
    private void OnEnable()
    {
        RareCardProbability = 0.5f;
        SetUpCardRewards();
    }

    void SetUpCardRewards()
    {
        var _rewardCards = new List<CardData>();

        // 확률에 따라 레어 카드 선택
        if (Random.value <= RareCardProbability)
        {
            // 레어 카드 1장, 일반 카드 2장 선택
            var rareCard = GameManager.UserData.GetRandomCards(1, E_CardTier.Rare);
            var normalCards = GameManager.UserData.GetRandomCards(2, E_CardTier.Normal);

            if (rareCard != null)
            {
                _rewardCards.Add(rareCard[0]);
            }

            _rewardCards.AddRange(normalCards);
        }
        else
        {
            // 일반 카드 3장 선택
            var normalCards = GameManager.UserData.GetRandomCards(3, E_CardTier.Normal);
            _rewardCards.AddRange(normalCards);
        }

        for (int i = 0; i < 3; i++)
        {
            CardGOs[i].transform.position = new Vector3(0, -0.51f, 0);
            var cardgo = CardGOs[i].GetComponent<CardGO>();
            cardgo.thisCardData = _rewardCards[i];
            cardgo.SetUpCardSprite();
            CardGOs[i].transform.DOMoveX((i - 1) * 5, 0.2f);
        }
    }
}
