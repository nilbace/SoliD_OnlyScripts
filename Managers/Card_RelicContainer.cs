using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ��� ī��� ���� �����͸� ������ ������ �����̳� ���󿡼� ���ϴ� ���� �ִٸ� ��ȯ
/// </summary>
public class Card_RelicContainer
{
    private List<CardData> _allCardsList = new List<CardData>();
    public List<RelicBase> AllRelicList = new List<RelicBase>();

    public void Init()
    {
    }

    public List<CardData> GetBaseDeck()
    {
        var baseDeck = _allCardsList.GetRange(0, 17);
        return baseDeck;
    }

    public void AddCardToAllCardList(CardData cardData)
    {
        _allCardsList.Add(cardData);
    }

    public RelicBase GetRelicByType(E_RelicType type)
    {
        return AllRelicList.FirstOrDefault(x => x.RelicType == type);
    }

    public List<CardData> GetRandomCardDatas(int number, E_CardTier? tier = null, E_CharName? cardOwner = null, E_WeaponType? weapon = null)
    {
        var temp = new List<CardData>();
        var randomIndices = new HashSet<int>();

        //CardTier==E_CardTier.InBattle ��� ����
        //InBattle�� ���� �������� ��� ī�尡 �ƴ� ���� ���߿��� �����Ǵ� ī���̱� ����
        IEnumerable<CardData> filteredCards = _allCardsList.Where(card => card.CardTier != E_CardTier.InBattle);

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

    public CardData GetCardDataByIndex(int index)
    {
        return _allCardsList.FirstOrDefault(card => card.CardIndex == index);
    }


    public CardData GetRandomFourGodCardData()
    {
        // Define the range for random card selection
        int minIndex = 18;
        int maxIndex = 22;

        // Select a random index within the specified range
        int randomCardIndex = UnityEngine.Random.Range(minIndex, maxIndex);

        // Find the card with the matching index in the AllCardsList
        CardData selectedCard = GameManager.Card_RelicContainer._allCardsList
            .FirstOrDefault(card => card.CardIndex == randomCardIndex);

        // Return the selected card, or null if not found
        return selectedCard;
    }

    public CardData GetRandomBlackCardData()
    {
        // Define the range for random card selection
        int minIndex = 65;
        int maxIndex = 71;

        // Select a random index within the specified range
        int randomCardIndex = UnityEngine.Random.Range(minIndex, maxIndex);

        // Find the card with the matching index in the AllCardsList
        CardData selectedCard = GameManager.Card_RelicContainer._allCardsList
            .FirstOrDefault(card => card.CardIndex == randomCardIndex);

        // Return the selected card, or null if not found
        return selectedCard;
    }




    public List<RelicBase> GetRandomRelics(int number, E_RelicTier? tier = null)
    {
        var temp = new List<RelicBase>();
        var randomIndices = new HashSet<int>();

        IEnumerable<RelicBase> filteredRelics = AllRelicList;

        // E_RelicTier�� ������� ���� ����
        if (tier.HasValue)
        {
            filteredRelics = filteredRelics.Where(relic => relic.RelicTier == tier.Value);
        }

        // ������ ���� ���� ���� ����
        filteredRelics = filteredRelics.Where(relic => !TrialManager.Inst.HasRelic(relic.RelicType));

        var filteredList = filteredRelics.ToList();

        // ���͸��� ��Ͽ��� �������� ���� ����
        while (randomIndices.Count < number && randomIndices.Count < filteredList.Count)
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
