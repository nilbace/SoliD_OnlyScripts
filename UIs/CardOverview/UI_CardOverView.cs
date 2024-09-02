using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public enum E_CardOverviewType { UserDeck, DrawPile, DiscardPile, DuplicateCard, RemoveCard}

public class UI_CardOverView : MonoBehaviour
{
    public static UI_CardOverView Inst;
    public int columns = 5; // 그리드의 열 개수
    public float spacing = 10f; // 그리드 요소 간의 간격
    public Vector2 cellSize = new Vector2(100f, 100f); // 각 셀의 크기
    public GameObject CardGO;
    private bool _isOpened;
    public E_CardOverviewType OverviewType;
    public int NowCardCount;
    public float CardSize;
    public Button BackBTN;



    private void Awake()
    {
        Inst = this;
        transform.parent.position = Vector3.zero;
        ClosePannel();
    }

    public void ShowOverview(E_CardOverviewType type)
    {
        //이미 그 패널이 열려있는거라면
        if (_isOpened && OverviewType == type)
        {
            ClosePannel();
        }
        //열린 패널이 없거나 다른 패널이 열려있다면
        //패널을 닫고 올바른 패널을 열음
        RemoveAllChildren();
        ClosePannel();
        DragParentNonUI.Inst.CallWhenEnabled();
        OverviewType = type;

        switch (type)
        {
            case E_CardOverviewType.UserDeck:
                ShowUserDeck();
                break;
            case E_CardOverviewType.DrawPile:
                ShowDrawPile();
                break;
            case E_CardOverviewType.DiscardPile:
                ShowDiscardPile();
                break;
            case E_CardOverviewType.DuplicateCard:
                ShowUserDeck();
                break;
            case E_CardOverviewType.RemoveCard:
                ShowUserDeck();
                break;
        }
        BackBTN.gameObject.SetActive(true);
    }

    private void ShowUserDeck()
    {
        PopulateCards(TrialManager.Inst.UserDeck);
    }

    private void ShowDrawPile()
    {
        // DrawPile의 깊은 복사본을 생성하고, 이를 무작위로 섞음
        var drawPileCopy = new List<CardData>(Deck_GraveManager.Inst.DrawPile);
        var shuffledDrawPile = drawPileCopy.OrderBy(card => Random.value).ToList();
        PopulateCards(shuffledDrawPile);
    }

    private void ShowDiscardPile()
    {
        PopulateCards(Deck_GraveManager.Inst.DiscardPile);
    }

    private void PopulateCards(List<CardData> cardList)
    {
        _isOpened = true;
        NowCardCount = cardList.Count;
        transform.parent.gameObject.SetActive(true);
        for (int i = 0; i < cardList.Count; i++)
        {
            GameObject go = Instantiate(CardGO, transform);
            go.transform.localScale = go.transform.localScale * CardSize;
            go.AddComponent<Overview_CardTouch>();
            var cardgo = go.GetComponent<CardGO>();
            cardgo.thisCardData = cardList[i];
            cardgo.SetUpCardSprite();
            cardgo.SetSortingLayerForChildren("CardList");
        }

        StartCoroutine(ArrangeCardsCoroutine());
    }

    private IEnumerator ArrangeCardsCoroutine()
    {
        yield return new WaitForEndOfFrame();
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            int row = i / columns;
            int column = i % columns;

            float xPosition = column * (cellSize.x + spacing);
            float yPosition = -row * (cellSize.y + spacing); // Y축은 음수로 내려가도록 배치

            child.localPosition = new Vector3(xPosition, yPosition, 0f);
        }
    }

    public void ClosePannel()
    {
        _isOpened = false;
        transform.parent.gameObject.SetActive(false);
        BackBTN.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        RemoveAllChildren();
    }

    private void RemoveAllChildren()
    {
        int childCount = transform.childCount;

        // 자식 오브젝트를 역순으로 삭제
        for (int i = childCount - 1; i >= 0; i--)
        {
            // 자식 오브젝트를 찾고 삭제
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
