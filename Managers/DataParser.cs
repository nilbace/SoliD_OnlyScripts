using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DataParser : MonoBehaviour
{
    public static DataParser Inst;
    private List<CardEffectData> CardEffectList = new List<CardEffectData>();
    private const string URL_CardData = "https://docs.google.com/spreadsheets/d/1-taJJ7Z8a61PP_4emH93k5ooAO3j0-tKZxo4WkM7wz8/export?format=tsv&gid=0&range=A2:O86";
    private const string URL_CardEffectData = "https://docs.google.com/spreadsheets/d/1-taJJ7Z8a61PP_4emH93k5ooAO3j0-tKZxo4WkM7wz8/export?format=tsv&gid=1198669234&range=A2:D55";
    private const string URL_RelicData = "https://docs.google.com/spreadsheets/d/1-taJJ7Z8a61PP_4emH93k5ooAO3j0-tKZxo4WkM7wz8/export?format=tsv&gid=1371132894&range=A2:G43";
    public Action OnCardParseEnd { get; set; }

    private void Awake()
    {
        Inst = this;
    }
    private void Start()
    {
        StartCoroutine(RequestDatas());
    }

    IEnumerator RequestDatas()
    {
        StartCoroutine(RequestAndSetDayDatas(URL_RelicData, ProcessRelicData_To_List));
        yield return StartCoroutine(RequestAndSetDayDatas(URL_CardEffectData, ProcessCardEffectData_To_List));
        yield return StartCoroutine(RequestAndSetDayDatas(URL_CardData, ProcessCard_To_AllCardList));
        OnCardParseEnd?.Invoke();
    }

 

    public IEnumerator RequestAndSetDayDatas(string url, Action<string> processData)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string data = www.downloadHandler.text;
            string[] lines = data.Split('\n');

            foreach (string line in lines)
            {
                processData(line);
            }
        }
    }

    private void ProcessRelicData_To_List(string data)
    {
        string[] lines = data.Substring(0, data.Length).Split('\t');
        RelicBase relic = new RelicBase();
        if (!int.TryParse(lines[0], out relic.RelicID))
        {
            Debug.LogError($"{lines[0]} : Failed to parse the string to RelicID.");
        }
        if (!Enum.TryParse(lines[1], out relic.RelicType))
        {
            Debug.LogError($"{lines[1]} : Failed to parse the string to RelicType enum.");
        }
        if (!Enum.TryParse(lines[2], out relic.RelicTier))
        {
            Debug.LogError($"{lines[2]} : Failed to parse the string to RelicTier enum.");
        }
        relic.RelicNameKor = lines[3];
        relic.RelicInfoString = lines[4];
        if (!Enum.TryParse(lines[5], out relic.TriggerType))
        {
            Debug.LogError($"{lines[5]} : Failed to parse the string to TriggerType enum.");
        }
        if (!bool.TryParse(lines[6], out relic.HasStack))
        {
            Debug.LogError($"{lines[6]} : Failed to parse the string to HasStack .");
        }
        GameManager.Card_RelicContainer.AllRelicList.Add(relic);
    }

    private void ProcessCardEffectData_To_List(string data)
    {
        string[] lines = data.Substring(0, data.Length).Split('\t');
        CardEffectData cardEffect = new CardEffectData();
        if (!int.TryParse(lines[0], out cardEffect.EffectIndex))
        {
            Debug.LogError($"{lines[0]} : Failed to parse the string to EffectID.");
        }
        if (!Enum.TryParse(lines[1], out cardEffect.TargetType))
        {
            Debug.LogError($"{lines[1]} : Failed to parse the string to TargetType enum.");
        }
        if (!Enum.TryParse(lines[2], out cardEffect.CardEffectType))
        {
            Debug.LogError($"{lines[2]} : Failed to parse the string to CardEffectType enum.");
        }
        cardEffect.InfoString = lines[3];
        CardEffectList.Add(cardEffect);
    }

    private void ProcessCard_To_AllCardList(string data)
    {
        string[] lines = data.Substring(0, data.Length).Split('\t');
        CardData cardData = new CardData();
        if (!int.TryParse(lines[0], out cardData.CardIndex))
        {
            Debug.LogError($"{lines[0]} : Failed to parse the string to CardIndex.");
        }

        if (!Enum.TryParse(lines[1], out cardData.CardType))
        {
            Debug.LogError($"{lines[1]} : Failed to parse the string to CardType.");
        }

        if (!Enum.TryParse(lines[2], out cardData.CardOwner))
        {
            Debug.LogError($"{lines[2]} : Failed to parse the string to CardOwner.");
        }

        if (!Enum.TryParse(lines[3], out cardData.CardColor))
        {
            Debug.LogError($"{lines[3]} : Failed to parse the string to CardColor.");
        }

        if (!Enum.TryParse(lines[4], out cardData.WeaponType))
        {
            Debug.LogError($"{lines[4]} : Failed to parse the string to WeaponType.");
        }

        if (!Enum.TryParse(lines[5], out cardData.CardTier))
        {
            Debug.LogError($"{lines[5]} : Failed to parse the string to CardTier.");
        }

        if (!int.TryParse(lines[6], out cardData.CardCost))
        {
            Debug.LogError($"{lines[6]} : Failed to parse the string to CardCost.");
        }

        cardData.CardName = lines[7].Trim();
        cardData.CardInfoText = lines[8].Trim();

        if (!bool.TryParse(lines[9], out cardData.NeedTarget))
        {
            Debug.LogError($"{lines[9]} : Failed to parse the string to NeedTarget.");
        }

        if (!bool.TryParse(lines[10], out cardData.WillExpire))
        {
            Debug.LogError($"{lines[10]} : Failed to parse the string to Expire.");
        }

        //Debug.Log(cardData.CardName);

        string[] effectIDs = lines[11].Split('/');
        string[] effectParameters = lines[12].Split('/');
        for (int i = 0; i < effectIDs.Length; i++)
        {
            string index = effectIDs[i];
            if (int.TryParse(index, out int effectIndex))
            {
                // 깊은 복사를 위해 복사 생성자를 사용
                CardEffectData newEffect = new CardEffectData(GetCardEffectFromListByIndex(effectIndex));

                // effectParameters[i] 값을 Amount 속성에 설정
                if (float.TryParse(effectParameters[i], out float amount))
                {
                    newEffect.Amount = amount;
                }
                else
                {
                    Debug.LogError($"{effectParameters[i]} : Failed to parse the string to amount.");
                }

                cardData.CardEffectList.Add(newEffect);
            }
            else
            {
                //Debug.LogError($"{index} : Failed to parse the string to effect index.");
            }
        }
        cardData.CardSpriteNameString = lines[13].Trim();
        cardData.DamageEffectType = lines[14].Trim();
        // 처리된 카드를 덱에 추가
        GameManager.Card_RelicContainer.AddCardToAllCardList(cardData);
    }

    public CardEffectData GetCardEffectFromListByIndex(int index)
    {
        foreach (var effectData in CardEffectList)
        {
            if (effectData.EffectIndex == index)
            {
                return effectData;
            }
        }
        // 해당하는 EffectID가 없는 경우 null 반환
        return null;
    }


 
}
