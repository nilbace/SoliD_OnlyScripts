using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public GameObject CardGO;
    public GameObject RelicGO;
    public float spacing = 10f; // АЃАн
    public Vector3 FirstCardPoz;

    private void OnEnable()
    {
        ShowRelic();
        //ShowCards();
    }

    private void ShowRelic()
    {
        GameObject newRelic = Instantiate(RelicGO, new Vector3(-2.1f,-2,0), Quaternion.identity, transform);
        var storeRelic = newRelic.GetComponent<StoreRelic>();

        var randIndex = Random.Range(0, (int)E_RelicType.MaxCount);
        newRelic.GetComponent<SpriteRenderer>().sprite = RelicManager.Inst.GetRelicSprite((E_RelicType)randIndex);
        storeRelic.RelicType = (E_RelicType)randIndex;

        int randomPrice = Random.Range(100, 121);
        storeRelic.RelicCost = randomPrice;



        GameObject newRelic2 = Instantiate(RelicGO, new Vector3(1.5f, -2, 0), Quaternion.identity, transform);
        var storeRelic2 = newRelic2.GetComponent<StoreRelic>();

        var randIndex2 = Random.Range(0, (int)E_RelicType.MaxCount);
        newRelic2.GetComponent<SpriteRenderer>().sprite = RelicManager.Inst.GetRelicSprite((E_RelicType)randIndex2);
        storeRelic2.RelicType = (E_RelicType)randIndex2;

        int randomPrice2 = Random.Range(100, 121);
        storeRelic2.RelicCost = randomPrice2;
    }
    private void ShowCards()
    {
        var Cards = GameManager.UserData.AllCardsList;

        for (int i = 0; i < 5; i++)
        {
            GameObject newCard = Instantiate(CardGO, FirstCardPoz + new Vector3(i * spacing, 0, 0), Quaternion.identity, transform);
            var storemouse = newCard.AddComponent<StoreCard>();


            var randIndex = Random.Range(0, Cards.Count);
            var newcardData = newCard.GetComponent<CardGO>();
            newcardData.thisCardData = Cards[randIndex];

            newcardData.SetCardSprite();

            int randomPrice = Random.Range(100, 121);
            storemouse.CardCost = randomPrice;
            newCard.name = "Card" + (i + 1) + " - Price: " + randomPrice;
        }
    }

    public void ExitStore()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        gameObject.SetActive(false);
    }
}
