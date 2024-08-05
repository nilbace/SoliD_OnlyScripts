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

        // Ȯ���� ���� ���� ī�� ����
        if (Random.value <= RareCardProbability)
        {
            // ���� ī�� 1��, �Ϲ� ī�� 2�� ����
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
            // �Ϲ� ī�� 3�� ����
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
