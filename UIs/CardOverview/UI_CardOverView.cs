using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public enum E_CardOverviewType { UserDeck, DrawPile, DiscardPile, DuplicateCard, RemoveCard}

public class UI_CardOverView : MonoBehaviour
{
    public static UI_CardOverView Inst;
    public int columns = 5; // �׸����� �� ����
    public float spacing = 10f; // �׸��� ��� ���� ����
    public Vector2 cellSize = new Vector2(100f, 100f); // �� ���� ũ��
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
        //�̹� �� �г��� �����ִ°Ŷ��
        if (_isOpened && OverviewType == type)
        {
            ClosePannel();
        }
        //���� �г��� ���ų� �ٸ� �г��� �����ִٸ�
        //�г��� �ݰ� �ùٸ� �г��� ����
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
        // DrawPile�� ���� ���纻�� �����ϰ�, �̸� �������� ����
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
            float yPosition = -row * (cellSize.y + spacing); // Y���� ������ ���������� ��ġ

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

        // �ڽ� ������Ʈ�� �������� ����
        for (int i = childCount - 1; i >= 0; i--)
        {
            // �ڽ� ������Ʈ�� ã�� ����
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
