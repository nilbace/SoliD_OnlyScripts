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
    /// 적당한 게임을 키거나 전투를 실행시킴
    /// </summary>
    private void OnEnable()
    {
        StartRandomGame();
    }
    public void StartRandomGame()
    {
        int index = GetRandomGameIndex();
        Instantiate(Games[GetRandomGameIndex()], transform);

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

    /// <summary>
    /// ?방 이벤트가 끝나면 호출되는 함수
    /// 이 함수를 호출 이후 자기 자신을 Destroy하면 됨
    /// </summary>
    public void ExitMystery()
    {
        gameObject.SetActive(false);
        Map.MapPlayerTracker.Instance.Locked = false;
        BaseUI.Inst.MapBTN();
    }
}
