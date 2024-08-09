using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum E_RewardType { Normal, Elite, Boss}
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
    /// Resoruces�������� �ʿ��� �����͵� �ε�
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
    /// ���� ���̵��� ���� ���� ����
    /// </summary>
    /// <param name="rewardType"></param>
    public void GenerateReward(E_RewardType rewardType)
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
        //Todo ���� ���� ���� ����
        //Todo ī�� ���� ���� ����

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
 
    public void AddMoonStone()
    {
        AddMoonStoneOverTime(1f);
    }

    private void AddMoonStoneOverTime(float duration)
    {
        int initialGold = GameManager.UserData.MoonStoneAmount;
        int targetGold = initialGold + GoldReward.GoldRewardAmount;

        // Variable to store the last frame's gold value
        int lastGold = initialGold;

        // DOTween to animate gold increase over time
        DOTween.To(() => initialGold,
                   x => {
                       int deltaGold = x - lastGold;
                       GameManager.UserData.AddMoonStone(deltaGold); // Update gold incrementally
                   lastGold = x;
                   },
                   targetGold,
                   duration)
            .OnComplete(() =>
            {
            // Ensure the final value is set correctly if necessary
            int finalGoldAmount = targetGold - GameManager.UserData.MoonStoneAmount;
                if (finalGoldAmount > 0)
                {
                    GameManager.UserData.AddMoonStone(finalGoldAmount);
                }
            });
    }
}
