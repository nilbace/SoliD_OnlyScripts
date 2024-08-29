using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 각종 재화, 스토리 진행 상태 등을 저장
/// </summary>
public class UserData
{
    public void Init()
    {
        if (PlayerPrefs.HasKey("UserData"))
        {
            LoadData();
        }
        else
        {
            SaveData();
        }
        BaseUI.Inst.UpdateUIs();
    }
    public void LoadData()
    {
        string jsonData = PlayerPrefs.GetString("UserData");
        UserData loadedData = JsonUtility.FromJson<UserData>(jsonData);
    }

    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("UserData", jsonData);
        PlayerPrefs.Save();
    }

    #region Currency

    private int _memoryFragmentAmount;
    public int MemoryFragmentAmount { get { return _memoryFragmentAmount; } }
    private int _coreFragmentAmount;
    public int CoreFragmentAmount { get { return _coreFragmentAmount; } }


    public void AddMemoryFragment(int amount)
    {
        _memoryFragmentAmount += amount;
        BaseUI.Inst.UpdateUIs();
    }

    public void UseMemoryFragment(int amount)
    {
        _memoryFragmentAmount -= amount;
        BaseUI.Inst.UpdateUIs();
    }
    
    public void AddCoreFragment(int amount)
    {
        _coreFragmentAmount += amount;
        BaseUI.Inst.UpdateUIs();
    }

    public void UseCoreFragment(int amount)
    {
        _coreFragmentAmount -= amount;
        BaseUI.Inst.UpdateUIs();
    }

    #endregion
}
