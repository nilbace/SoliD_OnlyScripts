using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum E_MysteryType
{
    ScienceClassroom, HomeClassroom, ArtClassroom, MusicClassroom,
    Gym, Library, Hallway, TowerSearch, Merchant, MaxCount
}
public class MysteryManager : MonoBehaviour
{
    public GameObject[] Games;
    private int _nowGameIndex;
    public static MysteryManager Inst;

    private void Awake()
    {
        Inst = this;
    }
    /// <summary>
    /// Mystery에 진입하면 호출되는 함수
    /// </summary>
    private void OnEnable()
    {
        StartRandomGame();
    }
    public void StartRandomGame()
    {
        int index = GetRandomGameIndex();
        Instantiate(Games[GetRandomGameIndex()], transform);

        GameManager.Novel.StartStory(E_MysteryType.ScienceClassroom);


        if (TrialManager.Inst.HasRelic(E_RelicType.QuestionCollector))
        {
            TrialManager.Inst.AddMoonStone(50);
            BaseUI.Inst.TwinkleRelicIcon(E_RelicType.QuestionCollector);
        }
    }

    /// <summary>
    /// Todo
    /// TeachersKey유물 관련 로직 추가 예정
    /// </summary>
    /// <returns></returns>
    public int GetRandomGameIndex()
    {
        //return Random.Range(0, (int)E_MysteryType.MaxCount);
        return 0;
    }

    public void ExitMystery()
    {
        gameObject.SetActive(false);
        Games[_nowGameIndex].SetActive(false);
        Map.MapPlayerTracker.Instance.Locked = false;
        BaseUI.Inst.MapBTN();
    }
}
