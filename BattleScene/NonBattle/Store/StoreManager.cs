using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public GameObject CardGO;
    public GameObject RelicGO;
    public Vector3 FirstCardPoz;
    public float CardsSpacing;
    public Vector3 FirstRelicPoz;
    public float RelicsSpacing;
    public Transform ItemsParentTR;

    //ī�� ����, ���� ���
    [HideInInspector] public int DeckManageCost;
    [HideInInspector] public bool CanDeckManage;
    public TMPro.TMP_Text TMP_DeckManageCost;
    public Button DuplicateBTN;
    public Button RemoveBTN;

    private void Awake()
    {
        DeckManageCost = 100;
    }

    private void OnEnable()
    {
        CanDeckManage = true;
        DuplicateBTN.interactable = true;
        RemoveBTN.interactable = true;
        TMP_DeckManageCost.text = DeckManageCost.ToString();
        RemoveItems();
        ShowRelic();
        ShowCards();
    }

    bool CanBuyDeckManage()
    {
        return TrialManager.Inst.MoonStone >= DeckManageCost;
    }
    public void DuplicateCardBTN()
    {
        if (CanBuyDeckManage())
        {
            CanDeckManage = false;
            TrialManager.Inst.UseMoonStoneOverTime(DeckManageCost);
            DeckManageCost = (int)(DeckManageCost * 1.2f);
            DuplicateBTN.interactable = false;
            RemoveBTN.interactable = false;
            UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.DuplicateCard);
        }
        else
        {
            Debug.Log("����");
        }
    }

    public void RemoveCardBTN()
    {
        if (CanBuyDeckManage())
        {
            CanDeckManage = false;
            TrialManager.Inst.UseMoonStoneOverTime(DeckManageCost);
            DeckManageCost = (int)(DeckManageCost * 1.2f);
            DuplicateBTN.interactable = false;
            RemoveBTN.interactable = false;
            UI_CardOverView.Inst.ShowOverview(E_CardOverviewType.RemoveCard);
        }
        else
        {
            Debug.Log("����");
        }
    }

    private void RemoveItems()
    {
        // CardParentTR�� ��� �ڽ� ���� ������Ʈ�� �����մϴ�.
        foreach (Transform child in ItemsParentTR)
        {
            Destroy(child.gameObject);
        }
    }
    private void ShowRelic()
    {
        var relics = GameManager.Card_RelicContainer.GetRandomRelics(2, E_RelicTier.Normal);

        for (int i = 0; i < relics.Count; i++)
        {
            // ù ��° ���� ��ġ�� ��������, ������ �ΰ� ���� ������ ��ġ�� ����մϴ�.
            Vector3 relicPosition = FirstRelicPoz + new Vector3(i * RelicsSpacing, 0, 2);
            GameObject newRelic = Instantiate(RelicGO, relicPosition, Quaternion.identity, ItemsParentTR);
            var storeRelic = newRelic.GetComponent<StoreRelic>();

            var iconSprite = IconContainer.Inst.GetRelicSprite(relics[i].RelicType);

            if (iconSprite != null) storeRelic.IMG_Relic.sprite = iconSprite;
            storeRelic.RelicType = relics[i].RelicType;

            int randomPrice = Random.Range(150, 171);
            storeRelic.RelicCost = randomPrice;
            storeRelic.TMP_RelicCost.text = randomPrice.ToString();
        }
    }



    private void ShowCards()
    {
        //�Ϲ� ��� ī�� 5�� ��ȯ
        for (int i = 0; i < 6; i++)
        {
            GameObject newCard = Instantiate(CardGO, FirstCardPoz + new Vector3(i * CardsSpacing, 0, 2), Quaternion.identity, ItemsParentTR);
            var storemouse = newCard.GetComponent<StoreCard>();


            
            var newcardData = newCard.GetComponent<CardGO>();
            newcardData.thisCardData = GameManager.Card_RelicContainer.GetRandomCardDatas(1, E_CardTier.Normal, weapon: (E_WeaponType)(i + 1))[0];

            newcardData.SetUpCardSprite();

            int randomPrice = Random.Range(70, 91);
            storemouse.CardCost = randomPrice;
            storemouse.TMP_CardCost.text = randomPrice.ToString();
            newCard.name = "Card" + (i + 1) + " - Price: " + randomPrice;
        }
    }

    public void ExitStore()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        gameObject.SetActive(false);
        BaseUI.Inst.MapBTN();
    }
}
