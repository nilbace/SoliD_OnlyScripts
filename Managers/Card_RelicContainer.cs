using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 카드와 유물 데이터를 가지고 있으며 상점이나 보상에서 원하는 것이 있다면 반환
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

        //CardTier==E_CardTier.InBattle 라면 제거
        //InBattle은 전투 보상으로 얻는 카드가 아닌 전투 도중에서 생성되는 카드이기 때문
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

        // E_RelicTier를 기반으로 필터 적용
        if (tier.HasValue)
        {
            filteredRelics = filteredRelics.Where(relic => relic.RelicTier == tier.Value);
        }

        // 보유한 유물 제외 필터 적용
        filteredRelics = filteredRelics.Where(relic => !TrialManager.Inst.HasRelic(relic.RelicType));

        var filteredList = filteredRelics.ToList();

        // 필터링된 목록에서 무작위로 유물 선택
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
