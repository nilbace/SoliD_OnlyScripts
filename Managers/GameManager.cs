using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    public static GameManager Inst { get { Init(); return s_instance; } }

    UserData _userData = new UserData();
    Card_RelicContainer _cardData = new Card_RelicContainer();
    RewardManager _reward = new RewardManager();
    NovelManager _novel = new NovelManager();
    public static BattleManager Battle { get { return BattleManager.Inst; } }
    public static UserData UserData { get { return Inst._userData; } }
    public static Card_RelicContainer Card_RelicContainer { get { return Inst._cardData; } }
    public static RewardManager Reward { get { return Inst._reward; } }
    public static NovelManager Novel { get { return Inst._novel; } }
    

    private void Awake()
    {
        Init();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            s_instance = go.GetComponent<GameManager>();

            s_instance._userData.Init();
            s_instance._cardData.Init();
            s_instance._reward.Init();
        }

    }

    public static Camera GetMainCam()
    {
        return Camera.main;
    }
}
