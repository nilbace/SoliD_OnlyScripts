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
    public float radius = 30.0f; // ���Ƿ� ������ ������ ��

    [Header("ī�� 1,2,3���� �� ��ġ")]
    [SerializeField] private float _tValueForOneCard;
    [SerializeField] private Vector2 _tValueForTwoCards;
    [SerializeField] private Vector3 _tValueForThreeCards;

    public List<GameObject> CardsInMyHandList = new List<GameObject>();
    private Coroutine _arrangeCardsCoroutine;

    public IEnumerator DrawCardsCoroutine(int count)
    {
        // Don't draw cards if not in battle
        if (!BattleManager.Inst.IsOnBattle) yield break;

        for (int i = 0; i < count; i++)
        {
            // Create and set up a new card
            GameObject newCard = Instantiate(cardPrefab);
            newCard.AddComponent<CardMouseDetection>();
            var newcardData = newCard.GetComponent<CardGO>();

            newcardData.thisCardData = Deck_GraveManager.Inst.DrawCard();
            newcardData.SetUpCardSprite();
            CardsInMyHandList.Add(newCard);

            // Check if ArrangeCardsCoroutine is running, and stop it if necessary
            if (_arrangeCardsCoroutine != null)
            {
                StopCoroutine(_arrangeCardsCoroutine);
                _arrangeCardsCoroutine = null;
            }

            // Start the ArrangeCardsCoroutine and store its reference
            _arrangeCardsCoroutine = StartCoroutine(ArrangeCardsCoroutine());

            // Wait for a short interval before drawing the next card
            yield return new WaitForSeconds(0.2f);
        }
    }
   

    public IEnumerator AddCardToHandCoroutine(CardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab);
        newCard.AddComponent<CardMouseDetection>();
        var newcardData = newCard.GetComponent<CardGO>();

        newcardData.thisCardData = cardData;
        newcardData.SetUpCardSprite();
        CardsInMyHandList.Add(newCard);
        yield return StartCoroutine(ArrangeCardsCoroutine());
    }

    public void DiscardCardFromHand(int index)
    {
        if (index >= 0 && index < CardsInMyHandList.Count)
        {
            GameObject discardedCard = CardsInMyHandList[index];
            Deck_GraveManager.Inst.DiscardPile.Add(discardedCard.GetComponent<CardGO>().thisCardData);
            CardsInMyHandList.RemoveAt(index);
            Destroy(discardedCard);
            StartCoroutine(ArrangeCardsCoroutine());
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
            StartCoroutine(ArrangeCardsCoroutine());
        }
    }

    public void ExpireCardFromHand(GameObject cardGO)
    {
        if (CardsInMyHandList.Contains(cardGO))
        {
            CardsInMyHandList.Remove(cardGO);
            Destroy(cardGO);
            StartCoroutine(ArrangeCardsCoroutine());
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
        ArrangeCardsCoroutine();
    }

    public bool CanDiscardBlackCard()
    {
        foreach (GameObject card in CardsInMyHandList)
        {
            CardData cardData = card.GetComponent<CardGO>().thisCardData;

            if (cardData.CardIndex >= 65 && cardData.CardIndex <= 70)
            {
                return true;
            }
        }

        return false; 
    }

    public void DiscardRandomBlackCard()
    {
        List<GameObject> blackCardsInHand = new List<GameObject>();

        foreach (GameObject card in CardsInMyHandList)
        {
            CardData cardData = card.GetComponent<CardGO>().thisCardData;

            if (cardData.CardIndex >= 65 && cardData.CardIndex <= 70)
            {
                blackCardsInHand.Add(card);
            }
        }

        if (blackCardsInHand.Count > 0)
        {
            int randomIndex = Random.Range(0, blackCardsInHand.Count);
            GameObject cardToDiscard = blackCardsInHand[randomIndex];

            DiscardCardFromHand(cardToDiscard);
        }
        else
        {
            Debug.Log("No black cards to discard.");
        }
    }


    /// <summary>
    /// ī�尡 3�� �̸��϶��� ������ ��ġ ó���� �Լ�
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
            // ���� ó��: 1, 2, 3���� �ƴ� ����� �⺻�� ��ȯ
            return 0.5f;
        }
    }


    public IEnumerator ArrangeCardsCoroutine()
    {
        int totalCards = CardsInMyHandList.Count;
        float startAngle = Mathf.Deg2Rad * -AngleOffset;
        float endAngle = Mathf.Deg2Rad * AngleOffset;

        radius = 30.0f; // ���Ƿ� ������ ������ ��

        for (int i = 0; i < totalCards; i++)
        {
            float t = totalCards > 3 ? i / (float)(totalCards - 1) : GetProperT_Value(i);
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            // ���� ��ġ ���� x, y ��ġ ��� (z�� ������� �ʰų� �ٸ� �뵵�� ��� ����)
            Vector3 cardPosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle) - 0.8f, 0.01f * i) * radius;
            Vector3 targetPosition = centerPosition.position + cardPosition;

            CardsInMyHandList[i].transform.DOLocalMove(targetPosition, MoveDuration);

            // ī�尡 ��ġ�� ���� �ùٸ� ������ ����Ű���� z�� ȸ�� ����
            float targetRotation = -(angle * Mathf.Rad2Deg);
            CardsInMyHandList[i].transform.DORotate(new Vector3(0, 0, targetRotation), MoveDuration);

            // ��������Ʈ �������� ĵ���� ���� ���� ����
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

            yield return null;
        }
        yield return new WaitForSeconds(MoveDuration);
    }

    /// <summary>
    /// ��� ī�� ����
    /// </summary>
    public void ArrangeCardsOnce()
    {
        int totalCards = CardsInMyHandList.Count;
        // ��ġ�� �����ϴ� ���� ����
        float startAngle = Mathf.Deg2Rad * -AngleOffset;
        float endAngle = Mathf.Deg2Rad * AngleOffset;

        // ������ ����� �ʿ��ϴٸ� ���⼭ �߰��� �� �ֽ��ϴ�. ���������� ����ȭ�� ���� ����.
        // ���� ���, �ձ۰� ��ġ�� ���� ��ġ �������� ������ �� �ֽ��ϴ�.
        radius = 30.0f; // ���Ƿ� ������ ������ ��

        for (int i = 0; i < totalCards; i++)
        {
            float t = totalCards > 3 ? i / (float)(totalCards - 1) : GetProperT_Value(i);
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            // ���� ��ġ ���� x, y ��ġ ��� (z�� ������� �ʰų� �ٸ� �뵵�� ��� ����)
            Vector3 cardPosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle) - 0.8f, 0.01f * i) * radius;

            // ��ġ�� �߽� ��ġ�� ���� ������ �ʿ��ϴٸ� ���⼭ centerPosition�� ���մϴ�.
            Vector3 targetPosition = centerPosition.position + cardPosition;
            CardsInMyHandList[i].transform.DOLocalMove(targetPosition, 0);

            // ī�尡 ��ġ�� ���� �ùٸ� ������ ����Ű���� z�� ȸ�� ����
            float targetRotation = -(angle * Mathf.Rad2Deg);
            CardsInMyHandList[i].transform.DORotate(new Vector3(0, 0, targetRotation), 0);

            // ��������Ʈ �������� ĵ���� ���� ���� ����
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
