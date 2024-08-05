using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_MysteryType { Roulette, /*CardFlip,*/ MaxCount }
public class MysteryManager : MonoBehaviour
{
    public GameObject[] Games;
    private int _nowGameIndex;

    private void OnEnable()
    {
        StartRandomGame();
    }
    public void StartRandomGame()
    {
        _nowGameIndex = Random.Range(0, (int)E_MysteryType.MaxCount);
        gameObject.SetActive(true);
        Games[_nowGameIndex].SetActive(true);
    }

    public void ExitMystery()
    {
        gameObject.SetActive(false);
        Games[_nowGameIndex].SetActive(false);
        Map.MapPlayerTracker.Instance.Locked = false;
    }
}
