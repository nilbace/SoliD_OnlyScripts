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
        Debug.Log("�� ���� �Ϸ�");
        TrialManager.Inst.UserDeck.AddRange(GameManager.CardData.AllCardsList.GetRange(0, 20));
    }

    public void StartBattle()
    {
        DrawPile = new List<CardData>(TrialManager.Inst.UserDeck);
        //Shuffle(DrawPile); // ��ο� ������ ��� ī�� ����
        DiscardPile = new List<CardData>();
    }

    public CardData DrawCard()
    {
        if (DrawPile.Count > 0)
        {
            CardData topCard = DrawPile[0]; // DrawPile�� �� �� ī�� ��������
            DrawPile.RemoveAt(0); // DrawPile���� ī�� ����
            return topCard; // ī�� ��ȯ
        }
        else
        {
            // DrawPile�� ����ִٸ� DiscardPile�� ��� ī�带 DrawPile�� �ű�� ����
            if (DiscardPile.Count > 0)
            {
                DrawPile.AddRange(DiscardPile); // DiscardPile�� ��� ī�带 DrawPile�� �̵�
                DiscardPile.Clear(); // DiscardPile ����
                Shuffle(DrawPile); // DrawPile ����
            }
            else
            {
                Debug.LogWarning("DrawPile�� DiscardPile ��� ����ֽ��ϴ�."); // ī�尡 ���� ��� ��� �޽��� ���
                return null; // ī�尡 ���� �� null ��ȯ
            }
        }

        // �ٽ� DrawPile���� ī�� �ϳ� ��ȯ
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
