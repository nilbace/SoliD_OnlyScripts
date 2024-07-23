using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserData
{
    public List<CardData> AllCardsList = new List<CardData>();
    public List<CardData> UserDeck = new List<CardData>();
    private int _nowGold;
    public int NowGold { get { return _nowGold; } }

    public List<CardData> GetRandomCards(int number, E_CardTier? tier = null, E_CardOwner? cardOwner = null)
    {
        var temp = new List<CardData>();
        var randomIndices = new HashSet<int>();

        IEnumerable<CardData> filteredCards = AllCardsList;

        // Apply filters based on the provided tier and cardOwner
        if (tier.HasValue)
        {
            filteredCards = filteredCards.Where(card => card.CardTier == tier.Value);
        }
        if (cardOwner.HasValue)
        {
            filteredCards = filteredCards.Where(card => card.CardOwner == cardOwner.Value);
        }

        var filteredList = filteredCards.ToList();

        // Select random cards from the filtered list
        while (randomIndices.Count < number)
        {
            int randomIndex = Random.Range(0, filteredList.Count);
            if (randomIndices.Add(randomIndex))
            {
                temp.Add(filteredList[randomIndex]);
            }
        }

        return temp;
    }




    public void AddGold(int amount)
    {
        _nowGold += amount;
        BaseUI.Inst.UpdateUIs();
    }

    public void UseGold(int amount)
    {
        _nowGold -= amount;
        BaseUI.Inst.UpdateUIs();
    }

    public void Init()
    {
        if (PlayerPrefs.HasKey("UserData"))
        {
            LoadData();
        }
        else
        {
            // 货肺款 蜡历 单捞磐 积己
            AllCardsList = new List<CardData>();
            SaveData();
        }
        BaseUI.Inst.UpdateUIs();
    }

    public void LoadData()
    {
        string jsonData = PlayerPrefs.GetString("UserData");
        UserData loadedData = JsonUtility.FromJson<UserData>(jsonData);
        AllCardsList = loadedData.AllCardsList;
    }

    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("UserData", jsonData);
        PlayerPrefs.Save();
    }
}
