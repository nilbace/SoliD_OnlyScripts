using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_ContentType { StartHelper, Mystery, Rest, Store, MaxCount }
public class BattleScene : MonoBehaviour
{
    public static BattleScene Inst;
    public GameObject[] Contents;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        GameManager.UserData.AddGold(300);
    }

    public void StartContent(E_ContentType contentName)
    {
        Contents[(int)contentName].SetActive(true);
    }
}
