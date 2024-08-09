using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_ContentType { StartHelper, Mystery, Rest, Store, MaxCount }
public class BattleScene : MonoBehaviour
{
    public static BattleScene Inst;
    public GameObject[] Contents;
    public bool ActiveStartHelper;
    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        if (ActiveStartHelper)
            StartContent(E_ContentType.StartHelper);
    }

    public void StartContent(E_ContentType contentName)
    {
        Contents[(int)contentName].SetActive(true);
    }
}
