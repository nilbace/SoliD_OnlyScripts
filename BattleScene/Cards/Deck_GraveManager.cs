using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ī�� ��ġ�� ���� ī�� ��ġ�� �����ϴ� Ŭ����
/// </summary>
public class Deck_GraveManager : MonoSingleton<Deck_GraveManager>
{
    public List<CardData> DrawPile = new List<CardData>();
    public List<CardData> DiscardPile = new List<CardData>();
    public Button DrawPileBTN;
    public Button DiscardPileBTN;
    public TMPro.TMP_Text TMP_DrawPileCount;
    public TMPro.TMP_Text TMP_DiscardPileCount;

    private void Start()
    {
        DataParser.Inst.OnCardParseEnd += MakeBaseDeck;
        BattleManager.Inst.OnBattleStart += SetUpPiles;
    }

    private void Update()
    {
        TMP_DrawPileCount.text = DrawPile.Count.ToString();
        TMP_DiscardPileCount.text = DiscardPile.Count.ToString();
    }

    /// <summary>
    /// ���� ī�� ��ġ�� ������� �������� �ʰ� ���� ������ ������
    /// </summary>
    public void ShowDrawPile()
    {
        UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.DrawPile);
    }

    /// <summary>
    /// ���� ī�� ��ġ�� ������� ������
    /// </summary>
    public void ShowDiscardPile()
    {
        UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.DiscardPile);
    }

    /// <summary>
    /// ���� ���۽� �⺻ ���� ������
    /// </summary>
    public void MakeBaseDeck()
    {
        Debug.Log("�� ���� �Ϸ�");
        TrialManager.Inst.AddCard(GameManager.Card_RelicContainer.GetBaseDeck());
    }

    /// <summary>
    /// ������ �����ϸ� ���� ���� ���� ���縦 ���� �����Ͽ� �����
    /// </summary>
    public void SetUpPiles()
    {
        DrawPile = new List<CardData>(TrialManager.Inst.UserDeck);
        Shuffle(DrawPile); // ��ο� ������ ��� ī�� ����
        DiscardPile = new List<CardData>();
    }

    /// <summary>
    /// ī�带 ���� �̾Ƽ� �� �����͸� ��ȯ
    /// </summary>
    /// <returns>���� ī�� ��ġ���� ���� ���� �ִ� ������</returns>
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

    /// <summary>
    /// ī�� List�� ���� �Լ�
    /// </summary>
    /// <param name="cards"></param>
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
 }
