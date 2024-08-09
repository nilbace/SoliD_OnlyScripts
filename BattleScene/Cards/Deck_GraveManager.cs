using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck_GraveManager : MonoSingleton<Deck_GraveManager>
{
    public List<CardData> DrawPile = new List<CardData>();
    public List<CardData> DiscardPile = new List<CardData>();

    private void Start()
    {
        DataParser.Inst.OnCardParseEnd += MakeBaseDeck;
        BattleManager.Inst.OnBattleStart += StartBattle;
    }
    public void MakeBaseDeck()
    {
        Debug.Log("덱 생성 완료");
        TrialManager.Inst.UserDeck.AddRange(GameManager.CardData.AllCardsList.GetRange(0, 20));
    }

    public void StartBattle()
    {
        DrawPile = new List<CardData>(TrialManager.Inst.UserDeck);
        //Shuffle(DrawPile); // 드로우 파일의 모든 카드 섞기
        DiscardPile = new List<CardData>();
    }

    public CardData DrawCard()
    {
        if (DrawPile.Count > 0)
        {
            CardData topCard = DrawPile[0]; // DrawPile의 맨 위 카드 가져오기
            DrawPile.RemoveAt(0); // DrawPile에서 카드 제거
            return topCard; // 카드 반환
        }
        else
        {
            // DrawPile이 비어있다면 DiscardPile의 모든 카드를 DrawPile로 옮기고 섞음
            if (DiscardPile.Count > 0)
            {
                DrawPile.AddRange(DiscardPile); // DiscardPile의 모든 카드를 DrawPile로 이동
                DiscardPile.Clear(); // DiscardPile 비우기
                Shuffle(DrawPile); // DrawPile 섞기
            }
            else
            {
                Debug.LogWarning("DrawPile과 DiscardPile 모두 비어있습니다."); // 카드가 없을 경우 경고 메시지 출력
                return null; // 카드가 없을 때 null 반환
            }
        }

        // 다시 DrawPile에서 카드 하나 반환
        CardData newTopCard = DrawPile[0];
        DrawPile.RemoveAt(0);
        return newTopCard;
    }

    private void Shuffle(List<CardData> cards)
    {
        System.Random rng = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            CardData value = cards[n];
            cards[n] = cards[k];
            cards[k] = value;
        }
    }

    public void ShowDrawPile()
    {
        foreach(CardData data in DrawPile)
        {
            Debug.Log(data.CardName);
        }
    }

    public void ShowDiscardPile()
    {
        foreach (CardData data in DiscardPile)
        {
            Debug.Log(data.CardName);
        }
    }
}
