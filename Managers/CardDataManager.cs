using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ��� ī���� �����͸� ������ ������ �����̳� ���󿡼� ���ϴ� ī�尡 �ִٸ� ��ȯ
/// </summary>
public class CardDataManager
{
    public List<CardData> AllCardsList = new List<CardData>();

    public void Init()
    {
    }

    public List<CardData> GetRandomCards(int number, E_CardTier? tier = null, E_CharName? cardOwner = null, E_WeaponType? weapon = null)
    {
        var temp = new List<CardData>();
        var randomIndices = new HashSet<int>();

        IEnumerable<CardData> filteredCards = AllCardsList;

        // Apply filters based on the provided tier, cardOwner, and weapon
        if (tier.HasValue)
        {
            filteredCards = filteredCards.Where(card => card.CardTier == tier.Value);
        }
        if (cardOwner.HasValue)
        {
            filteredCards = filteredCards.Where(card => card.CardOwner == cardOwner.Value);
        }
        if (weapon.HasValue)
        {
            filteredCards = filteredCards.Where(card => card.WeaponType == weapon.Value);
        }

        var filteredList = filteredCards.ToList();

        // Select random cards from the filtered list
        while (randomIndices.Count < number)
        {
            int randomIndex = Random.Range(0, filteredList.Count);
            if (randomIndices.Add(randomIndex))
            {
                temp.Add(filteredList[randomIndex]);
            }
        }

        return temp;
    }

}
