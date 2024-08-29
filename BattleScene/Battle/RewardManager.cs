using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 전투 이후 보상 팝업창을 관리하는 스크립트
/// </summary>
public class RewardManager
{
    public GameObject RewardPannel;
    public Transform RewardsParent;
    public MonsterReward[] Rewards;

    public void Init()
    {
        RewardPannel = GameObject.Find("Canvases").transform.GetChild(1).gameObject;
        RewardsParent = RewardPannel.transform.GetChild(1).GetChild(0);
        Button ExitBTN = RewardPannel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
        ExitBTN.onClick.AddListener(()=>ExitReward());
        LoadRewards();
    }

    /// <summary>
    /// Resoruces폴더에서 필요한 데이터들 로드
    /// </summary>
    private void LoadRewards()
    {
        Rewards = Resources.LoadAll<MonsterReward>("BattleReward");

        if (Rewards == null || Rewards.Length == 0)
        {
            Debug.LogError("No MonsterRewards found in the Resources/BattleReward folder.");
        }
        else
        {
        }
    }

    /// <summary>
    /// 적의 난이도에 따라 보상 생성
    /// </summary>
    /// <param name="rewardType"></param>
    public void GenerateReward(E_EnemyDifficultyType rewardType)
    {
        string rewardName = rewardType.ToString();

        MonsterReward selectedReward = System.Array.Find(Rewards, reward => reward.name == rewardName);

        if (selectedReward == null)
        {
            Debug.LogWarning("No matching reward found for the type: " + rewardType);
            return;
        }

        int moonStoneReward = Random.Range(selectedReward.MinMoonStoneReward, selectedReward.MaxMoonStoneReward + 1);
        int memoryFragmentReward = Random.Range(selectedReward.MinMemoryFramentDiv100, selectedReward.MaxMemoryFragmentDiv100 + 1) * 100;
        int coreFragmentReward = Random.Range(selectedReward.MinCoreFragment, selectedReward.MaxCoreFragment + 1);

        GoldReward.GoldRewardAmount = moonStoneReward;
        //Todo 유물 보상 생성 로직
        //Todo 카드 보상 생성 로직

        RewardPannel.SetActive(true);
        foreach (Transform child in RewardsParent)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void ExitReward()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        Map.MapView.Inst.ShowMap();
    }
 
    
}
