using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class HandManager : MonoSingleton<HandManager>
{
    public GameObject cardPrefab;
    public Transform bottomPosition;
    public Transform centerPosition;
    public Transform topPosition;
    public float curvature = 1.0f;
    public float cardSpacing = 1.0f;
    public int startingSortingOrder = 0;
    public int sortingOrderIncrement = 10;
    public int AngleOffset;
    public float MoveDuration;
    public float radius = 30.0f; // 임의로 설정한 반지름 값

    [Header("카드 1,2,3장일 떄 위치")]
    [SerializeField] private float _tValueForOneCard;
    [SerializeField] private Vector2 _tValueForTwoCards;
    [SerializeField] private Vector3 _tValueForThreeCards;

    public List<GameObject> CardsInMyHandList = new List<GameObject>();

    public void DrawCards(int count)
    {
        // DOTween 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < count; i++)
        {
            sequence.AppendCallback(() => {
                GameObject newCard = Instantiate(cardPrefab);
                newCard.AddComponent<CardMouseDetection>();
                var newcardData = newCard.GetComponent<CardGO>();

                newcardData.thisCardData = Deck_GraveManager.Inst.DrawCard();
                newcardData.SetUpCardSprite();
                CardsInMyHandList.Add(newCard);
                ArrangeCards();
            });

            // 각 카드 생성 후 대기 시간 추가
            sequence.AppendInterval(0.2f);
        }
        // 시퀀스 실행
        //sequence.Play();
    }

    public void AddCardToHand(CardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab);
        newCard.AddComponent<CardMouseDetection>();
        var newcardData = newCard.GetComponent<CardGO>();

        newcardData.thisCardData = cardData;
        newcardData.SetUpCardSprite();
        CardsInMyHandList.Add(newCard);
        ArrangeCards();
    }

    public void AddCardToHand(int cardIndex)
    {
        AddCardToHand(GameManager.UserData.AllCardsList.FirstOrDefault(e => e.CardIndex == cardIndex));
    }

    public void AddRandomBlackCard()
    {
        int index = Random.Range(65, 71);
        AddCardToHand(GameManager.UserData.AllCardsList.FirstOrDefault(e => e.CardIndex == index));
    }


    public void DiscardCardFromHand(int index)
    {
        if (index >= 0 && index < CardsInMyHandList.Count)
        {
            GameObject discardedCard = CardsInMyHandList[index];
            Deck_GraveManager.Inst.DiscardPile.Add(discardedCard.GetComponent<CardGO>().thisCardData);
            CardsInMyHandList.RemoveAt(index);
            Destroy(discardedCard);
            ArrangeCards();
        }
    }

    public void DiscardRandomCardFromHand()
    {
        int index = Random.Range(0, CardsInMyHandList.Count);
        DiscardCardFromHand(index);
    }

    public void DiscardCardFromHand(GameObject cardGO)
    {
        if(CardsInMyHandList.Contains(cardGO))
        {
            CardsInMyHandList.Remove(cardGO);
            Deck_GraveManager.Inst.DiscardPile.Add(cardGO.GetComponent<CardGO>().thisCardData);
            Destroy(cardGO);
            ArrangeCards();
        }
    }

    public void DiscardAllCardsFromHand()
    {
        foreach (GameObject card in CardsInMyHandList)
        {
            Deck_GraveManager.Inst.DiscardPile.Add(card.GetComponent<CardGO>().thisCardData);
            Destroy(card);
        }
        CardsInMyHandList.Clear();
        ArrangeCards();
    }

    /// <summary>
    /// 카드가 3장 미만일때의 적절한 위치 처리용 함수
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private float GetProperT_Value(int i)
    {
        if (CardsInMyHandList.Count == 1)
        {
            return _tValueForOneCard;
        }
        else if (CardsInMyHandList.Count == 2)
        {
            if (i == 0)
            {
                return _tValueForTwoCards.x;
            }
            else
            {
                return _tValueForTwoCards.y;
            }
        }
        else if (CardsInMyHandList.Count == 3)
        {
            if (i == 0)
            {
                return _tValueForThreeCards.x;
            }
            else if (i == 1)
            {
                return _tValueForThreeCards.y;
            }
            else
            {
                return _tValueForThreeCards.z;
            }
        }
        else
        {
            // 예외 처리: 1, 2, 3장이 아닌 경우의 기본값 반환
            return 0.5f;
        }
    }


    public void ArrangeCards()
    {
        int totalCards = CardsInMyHandList.Count;
        // 아치를 형성하는 각도 범위
        float startAngle = Mathf.Deg2Rad * -AngleOffset;
        float endAngle = Mathf.Deg2Rad * AngleOffset; 

        // 반지름 계산이 필요하다면 여기서 추가할 수 있습니다. 예제에서는 간단화를 위해 생략.
        // 예를 들어, 둥글게 배치할 때의 아치 반지름을 지정할 수 있습니다.
        radius = 30.0f; // 임의로 설정한 반지름 값

        for (int i = 0; i < totalCards; i++)
        {
            float t = totalCards > 3 ? i / (float)(totalCards - 1) : GetProperT_Value(i);
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            // 원형 아치 위의 x, y 위치 계산 (z는 사용하지 않거나 다른 용도로 사용 가능)
            Vector3 cardPosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle) -0.8f, 0.01f * i) * radius;

            // 아치의 중심 위치에 대한 조정이 필요하다면 여기서 centerPosition을 더합니다.
            Vector3 targetPosition = centerPosition.position + cardPosition;
            CardsInMyHandList[i].transform.DOLocalMove(targetPosition, MoveDuration);

            // 카드가 아치를 따라 올바른 방향을 가리키도록 z축 회전 조정
            float targetRotation = -(angle * Mathf.Rad2Deg);
            CardsInMyHandList[i].transform.DORotate(new Vector3(0, 0, targetRotation), MoveDuration);

            // 스프라이트 렌더러와 캔버스 정렬 순서 조정
            SpriteRenderer[] spriteRenderers = CardsInMyHandList[i].GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].sortingOrder = startingSortingOrder + i * sortingOrderIncrement - j;
            }

            Canvas[] canvases = CardsInMyHandList[i].GetComponentsInChildren<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.sortingOrder = startingSortingOrder + i * sortingOrderIncrement + 1;
            }
        }
    }

    /// <summary>
    /// 즉시 카드 정렬
    /// </summary>
    public void ArrangeCardsOnce()
    {
        int totalCards = CardsInMyHandList.Count;
        // 아치를 형성하는 각도 범위
        float startAngle = Mathf.Deg2Rad * -AngleOffset;
        float endAngle = Mathf.Deg2Rad * AngleOffset;

        // 반지름 계산이 필요하다면 여기서 추가할 수 있습니다. 예제에서는 간단화를 위해 생략.
        // 예를 들어, 둥글게 배치할 때의 아치 반지름을 지정할 수 있습니다.
        radius = 30.0f; // 임의로 설정한 반지름 값

        for (int i = 0; i < totalCards; i++)
        {
            float t = totalCards > 3 ? i / (float)(totalCards - 1) : GetProperT_Value(i);
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            // 원형 아치 위의 x, y 위치 계산 (z는 사용하지 않거나 다른 용도로 사용 가능)
            Vector3 cardPosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle) - 0.8f, 0.01f * i) * radius;

            // 아치의 중심 위치에 대한 조정이 필요하다면 여기서 centerPosition을 더합니다.
            Vector3 targetPosition = centerPosition.position + cardPosition;
            CardsInMyHandList[i].transform.DOLocalMove(targetPosition, 0);

            // 카드가 아치를 따라 올바른 방향을 가리키도록 z축 회전 조정
            float targetRotation = -(angle * Mathf.Rad2Deg);
            CardsInMyHandList[i].transform.DORotate(new Vector3(0, 0, targetRotation), 0);

            // 스프라이트 렌더러와 캔버스 정렬 순서 조정
            SpriteRenderer[] spriteRenderers = CardsInMyHandList[i].GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].sortingOrder = startingSortingOrder + i * sortingOrderIncrement - j;
            }

            Canvas[] canvases = CardsInMyHandList[i].GetComponentsInChildren<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.sortingOrder = startingSortingOrder + i * sortingOrderIncrement + 1;
            }
        }
    }
}
