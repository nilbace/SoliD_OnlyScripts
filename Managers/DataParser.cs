using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using System.Reflection;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DataParser : MonoBehaviour
{
    public static DataParser Inst;
    public List<CardEffectData> CardEffectList;
    private const string URL_CardData = "https://docs.google.com/spreadsheets/d/1-taJJ7Z8a61PP_4emH93k5ooAO3j0-tKZxo4WkM7wz8/export?format=tsv&gid=0&range=A2:K32";
    private const string URL_CardEffectData = "https://docs.google.com/spreadsheets/d/1-taJJ7Z8a61PP_4emH93k5ooAO3j0-tKZxo4WkM7wz8/export?format=tsv&gid=1198669234&range=A2:D26";

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
        yield return StartCoroutine(RequestAndSetDayDatas(URL_CardEffectData, ProcessCardEffectData_To_List));
        yield return StartCoroutine(RequestAndSetDayDatas(URL_CardData, ProcessCard_To_Deck));
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

    void ProcessCardEffectData_To_List(string data)
    {
        string[] lines = data.Substring(0, data.Length).Split('\t');
        CardEffectData cardEffect = new CardEffectData();
        if (!int.TryParse(lines[0], out cardEffect.EffectID))
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

    void ProcessCard_To_Deck(string data)
    {
        string[] lines = data.Substring(0, data.Length).Split('\t');
        CardData cardData = new CardData();
        if (!Enum.TryParse(lines[0], out cardData.CardType))
        {
            Debug.LogError($"{lines[0]} : Failed to parse the string to CardType.");
        }

        if (!Enum.TryParse(lines[1], out cardData.CardOwner))
        {
            Debug.LogError($"{lines[1]} : Failed to parse the string to CardOwner.");
        }

        if (!Enum.TryParse(lines[2], out cardData.CardColor))
        {
            Debug.LogError($"{lines[2]} : Failed to parse the string to CardColor.");
        }

        if (!Enum.TryParse(lines[3], out cardData.CardTier))
        {
            Debug.LogError($"{lines[3]} : Failed to parse the string to CardTier.");
        }

        if (!int.TryParse(lines[4], out cardData.CardCost))
        {
            Debug.LogError($"{lines[4]} : Failed to parse the string to CardCost.");
        }

        cardData.CardName = lines[5];
        cardData.CardInfoText = lines[6];

        if (!bool.TryParse(lines[7], out cardData.NeedTarget))
        {
            Debug.LogError($"{lines[7]} : Failed to parse the string to NeedTarget.");
        }
    

        string[] effectIDs = lines[8].Split('/');
        string[] effectParameters = lines[9].Split('/');
        for (int i = 0; i < effectIDs.Length; i++)
        {
            string index = effectIDs[i];
            if (int.TryParse(index, out int effectIndex))
            {
                // ���� ���縦 ���� ���� �����ڸ� ���
                CardEffectData newEffect = new CardEffectData(GetCardEffectFromListByIndex(effectIndex));

                // effectParameters[i] ���� Amount �Ӽ��� ����
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
                Debug.LogError($"{index} : Failed to parse the string to effect index.");
            }
        }
        cardData.CardSpriteNameString = lines[10].Trim();
        // ó���� ī�带 ���� �߰�
        GameManager.UserData.AllCardsList.Add(cardData);
    }

    public CardEffectData GetCardEffectFromListByIndex(int index)
    {
        foreach (var effectData in CardEffectList)
        {
            if (effectData.EffectID == index)
            {
                return effectData;
            }
        }
        // �ش��ϴ� EffectID�� ���� ��� null ��ȯ
        return null;
    }


 
}
