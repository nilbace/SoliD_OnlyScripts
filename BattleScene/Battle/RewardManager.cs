using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum E_RewardList { Gold,Relic, Card}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Inst;
    public GameObject RewardBoard;
    public Transform RewardsParent;

    private void Awake()
    {
        Inst = this;
    }

    public void GenerateNormalMonsterReward()
    {
        GoldReward.GoldRewardAmount = Random.Range(100, 121);
        RelicReward.S_RelicType = (E_RelicType)Random.Range(0, (int)E_RelicType.MaxCount);
        CardReward.RareCardProbability = 0.1f;
        // RewardBoard의 모든 자식들을 활성화
        RewardBoard.SetActive(true);
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

    public void AddGold()
    {
        StartCoroutine(AddGoldOverTime(1f));
    }

    private IEnumerator AddGoldOverTime(float duration)
    {
        int initialGold = GameManager.UserData.NowGold;
        int targetGold = initialGold + GoldReward.GoldRewardAmount;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            int currentGold = Mathf.RoundToInt(Mathf.Lerp(initialGold, targetGold, elapsedTime / duration));
            GameManager.UserData.AddGold(currentGold - GameManager.UserData.NowGold);

            yield return null;
        }

        // Ensure the final value is set correctly
        GameManager.UserData.AddGold(targetGold - GameManager.UserData.NowGold);
    }
}
