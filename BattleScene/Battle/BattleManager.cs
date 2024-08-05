using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ���� ���, ��, ĳ���� �̵� ����
/// </summary>
public class BattleManager : MonoSingleton<BattleManager>
{
    public List<UnitBase> PlayerUnits;
    public List<MonsterBase> MonsterUnits;
    public int MonsterCount;
    public Action OnBattleStart;

    //ĳ���� �̵� ����
    [Tooltip("ĳ���� ������ �Ÿ�")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;

    //�÷��̾ ������ ����
    [HideInInspector]
    public UnitBase TargetMonster;
    
    //������ ����
    [HideInInspector]public int EnergyAmount;
    [HideInInspector] public int NowEnergy;
    public TMPro.TMP_Text TMP_Energy;

    
    protected override void Awake()
    {
        base.Awake();
        PlayerUnits = new List<UnitBase>();
        MonsterUnits = new List<MonsterBase>();
    }

    private void Start()
    {
        EnergyAmount = 3;
        NowEnergy = EnergyAmount;
        UpdateBattleUI();
    }

    /// <summary>
    /// ���ο� ������ ���۵� �� ȣ��
    /// </summary>
    public void ResetDatas()
    {
        MonsterCount = 0;
        MonsterUnits = new List<MonsterBase>();
    }

    

    /// <summary>
    /// ���� ��ġ�ϰ� �÷��̾� �� �����ϸ� ��
    /// </summary>
    [ContextMenu("�� ����")]
    public void StartMinorBattle()
    {
        OnBattleStart?.Invoke();
        var seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            ResetDatas();
            SetUpEnemy(MonsterContainer.Inst.GetAnyMinorMonster(), new Vector3(5, 0f, 0f));

        })
            .AppendInterval(0.2f)
            .AppendCallback(ActiveRelics)
            .AppendInterval(0.2f)
            .AppendCallback(StartPlayerTurn);
    }


    private void SetUpEnemy(GameObject monsterGO, Vector3 poz)
    {
        var Monster = Instantiate(monsterGO);
        Monster.transform.position = poz;
        var MonsterBase = Monster.GetComponent<MonsterBase>();
        MonsterUnits.Add(MonsterBase);
        MonsterBase.OnDead += () => ClearCheck();
        MonsterCount++;
    }

    public void ActiveRelics()
    {
        RelicManager.Inst.OnBattleStart?.Invoke();
    }

    public void StartPlayerTurn()
    {
        FillEnergy();
        HandManager.Inst.DrawCards(6);
        ShowMonsterIntents();
    }

    public void EndPlayerTurn()
    {
        HandManager.Inst.DiscardAllCardsFromHand();
        ReduceEffectDuration(isPlayer: true);
        StartMonsterTurn();
    }

    

    void ShowMonsterIntents()
    {
        foreach(MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            mon.SetIntent();
        }
    }

    public void StartMonsterTurn()
    {
        var sequence = DOTween.Sequence();
        foreach (MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            sequence.Append(mon.GetSequenceByIntent()).AppendInterval(0.5f);
        }
        sequence.OnComplete(() => {
            ReduceEffectDuration(isPlayer: false);
            StartPlayerTurn();
        });
    }

    void ClearCheck()
    {
        MonsterCount--;
        Debug.Log("üũ");
        if (MonsterCount <= 0) Clear();
    }

    public void Clear()
    {
        ClearPlayerStatusEffects();
        HandManager.Inst.DiscardAllCardsFromHand();
        RewardManager.Inst.GenerateNormalMonsterReward();
    }

    private void ClearPlayerStatusEffects()
    {
        foreach(UnitBase player in PlayerUnits)
        {
            player.ClearStatusEffect();
        }
    }

    public void MonsterAttackPlayer(float amount)
    {
        PlayerUnits[0].GetDamage(amount);
    }

    public void MonsterApplyEffect_To_Player(E_EffectType buff, float amount)
    {
        PlayerUnits[0].ApplyBuff(buff, amount);
    }

    public void ReduceEffectDuration(bool isPlayer)
    {
        if (isPlayer)
        {
            ProcessUnits(PlayerUnits);
        }
        else
        {
            ProcessUnits(MonsterUnits.Cast<UnitBase>().ToList());
        }
    }

    private void ProcessUnits(List<UnitBase> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitBase unit = units[i];
            if (!unit.isAlive()) continue;
            for (int j = 0; j < unit.BuffList.Count; j++)
            {
                BuffBase effect = unit.BuffList[j];
                effect.NextTurnStarted(unit);
            }
            unit.EffectUpdateAction?.Invoke();
        }
    }

    public UnitBase GetPlayer(E_CharName name)
    {
        return PlayerUnits.FirstOrDefault(unit => unit.tag == name.ToString());
    }
    public UnitBase GetLowestHealthPlayer()
    {
        // Ensure there is at least one player unit
        if (PlayerUnits == null || PlayerUnits.Count == 0)
        {
            return null; // Return null if no players are available
        }

        // Initialize the variable to keep track of the player with the lowest health
        UnitBase lowestHealthPlayer = PlayerUnits[0];

        // Iterate over each player unit to find the one with the lowest health
        foreach (var player in PlayerUnits)
        {
            if (player.NowHp < lowestHealthPlayer.NowHp)
            {
                lowestHealthPlayer = player; // Update to the current player if their health is lower
            }
        }

        return lowestHealthPlayer;
    }

    /// <summary>
    /// ��ü�� ��� Ÿ���� �־ ���(��) ��ȯ
    /// </summary>
    /// <param name="unit">��ü</param>
    /// <param name="targetType">���</param>
    /// <returns></returns>
    public List<UnitBase> GetProperUnits(UnitBase unit, E_TargetType targetType)
    {
        List<UnitBase> tempUnits = new List<UnitBase>();

        switch (targetType)
        {
            case E_TargetType.TargetEnemy:
                tempUnits.Add(TargetMonster);
                break;
            case E_TargetType.AllEnemies:
                foreach(UnitBase _unit in MonsterUnits)
                {
                    if (unit.tag != _unit.tag && _unit.isAlive()) tempUnits.Add(_unit);
                }
                break;
            case E_TargetType.Self:
                tempUnits.Add(unit);
                break;

            case E_TargetType.AnyEnemy:
                if (MonsterUnits.Count > 0)
                {
                    int randomIndex;
                    // ����ִ� ������ ã�� ������ �ݺ�
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, MonsterUnits.Count); // ������ �ε��� ����
                    } while (!MonsterUnits[randomIndex].isAlive()); // ������� ������ �ٽ� �ݺ�

                    // ����ִ� ������ tempUnits�� �߰�
                    tempUnits.Add(MonsterUnits[randomIndex]);
                }
                break;

            case E_TargetType.AllAllies:
                tempUnits.AddRange(PlayerUnits);
                break;
        }
        return tempUnits;
    }

    public List<UnitBase> GetProperUnits(E_CharName ownerName, E_TargetType targetType)
    {
        UnitBase ownerUnit = PlayerUnits.Find(unit => unit.tag == ownerName.ToString());

        if (ownerUnit != null)
        {
            return GetProperUnits(ownerUnit, targetType);
        }

        return new List<UnitBase>();
    }

    #region ĳ���� �̵� 
    public Sequence MoveCharFront(E_CharName cardOwner)
    {
        // cardOwner�� ��ġ�ϴ� ĳ���͸� ã���ϴ�.
        GameObject targetChar = null;
        int targetIndex = -1;
        for (int i = 0; i < PlayerUnits.Count; i++)
        {
            if (PlayerUnits[i].tag == cardOwner.ToString())
            {
                targetChar = PlayerUnits[i].gameObject;
                targetIndex = i;
                break;
            }
        }
        Sequence moveSequence = DOTween.Sequence();
        // cardOwner�� ���� ���ڸ���, �������� �ڷ� ����ϴ�
        if (targetChar != null && targetIndex != -1)
        {
            // DOTween �������� ����Ͽ� �ִϸ��̼� ������ �����մϴ�.
            

            moveSequence.Append(targetChar.transform.DOLocalMoveX(0, PlayerMoveDuration));
            moveSequence.Join(targetChar.transform.DOScale(FrontCharSize, PlayerMoveDuration));

            int pozIndex = 1;
            for (int i = 0; i < PlayerUnits.Count; i++)
            {
                if (i != targetIndex)
                {
                    moveSequence.Join(PlayerUnits[i].transform.DOLocalMoveX(-PlayerCharOffset*pozIndex, PlayerMoveDuration));
                    moveSequence.Join(PlayerUnits[i].transform.DOScale(BackCharSize, PlayerMoveDuration));
                    pozIndex++;
                }
            }

            // �ִϸ��̼��� �Ϸ�� �� ����Ʈ�� ������ �ٲߴϴ�.
            moveSequence.OnComplete(() =>
            {
                // targetChar�� ����Ʈ�� ù ��°�� �̵��մϴ�.
                var movedUnit = PlayerUnits[targetIndex];
                PlayerUnits.RemoveAt(targetIndex);
                PlayerUnits.Insert(0, movedUnit);
            });

            
        }

        return moveSequence;
    }

    #endregion

    #region Energy
    public void FillEnergy()
    {
        NowEnergy = EnergyAmount;
        UpdateBattleUI();
    }

    public void UseEnergy(int cost)
    {
        NowEnergy -= cost;
        UpdateBattleUI();
    }

    public void AddEnergy(float amount)
    {
        NowEnergy += (int)amount;
        UpdateBattleUI();
    }

    #endregion
    public void UpdateBattleUI()
    {
        TMP_Energy.text = $"{NowEnergy}";
    }
}
