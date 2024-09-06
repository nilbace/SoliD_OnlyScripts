using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 뽑을 카드 뭉치와 버린 카드 뭉치를 관리하는 클래스
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
    /// 뽑을 카드 뭉치를 순서대로 보여주지 않고 랜덤 순서로 보여줌
    /// </summary>
    public void ShowDrawPile()
    {
        UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.DrawPile);
    }

    /// <summary>
    /// 버린 카드 뭉치를 순서대로 보여줌
    /// </summary>
    public void ShowDiscardPile()
    {
        UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.DiscardPile);
    }

    /// <summary>
    /// 게임 시작시 기본 덱을 생성함
    /// </summary>
    public void MakeBaseDeck()
    {
        Debug.Log("덱 생성 완료");
        TrialManager.Inst.AddCard(GameManager.Card_RelicContainer.GetBaseDeck());
    }

    /// <summary>
    /// 전투가 시작하면 유저 덱을 깊은 복사를 통해 복제하여 사용함
    /// </summary>
    public void SetUpPiles()
    {
        DrawPile = new List<CardData>(TrialManager.Inst.UserDeck);
        Shuffle(DrawPile); // 드로우 파일의 모든 카드 섞기
        DiscardPile = new List<CardData>();
    }

    /// <summary>
    /// 카드를 한장 뽑아서 그 데이터를 반환
    /// </summary>
    /// <returns>뽑을 카드 뭉치에서 가장 위에 있는 데이터</returns>
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

    /// <summary>
    /// 카드 List를 섞는 함수
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
