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
    public Action OnPlayerTurnStart;
    public bool IsOnBattle;
    public int TurnCount;

    //ĳ���� �̵� ����
    [Tooltip("ĳ���� ������ �Ÿ�")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;

    //�÷��̾ ������ ����
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

    public static bool TargetIsAlive()
    {
        if (Inst.TargetMonster == null) return false;
        return Inst.TargetMonster.isAlive();
    }

    /// <summary>
    /// ���� ��ġ�ϰ� �÷��̾� �� �����ϸ� ��
    /// TargetMonster�� ����, MonsterUnits����Ʈ �ʱ�ȭ �ʿ�
    /// </summary>
    [ContextMenu("�� ����")]
    public void StartMinorBattle()
    {
        TargetMonster = null;
        MonsterUnits = new List<MonsterBase>();
        TurnCount = 0;
        IsOnBattle = true;
        OnBattleStart?.Invoke();
        var seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            ResetDatas();
            SetUpEnemy(MonsterContainer.Inst.GetMonsterByType(E_MinorEnemyType.Yare), new Vector3(3.5f, 0f, 0f));
            //SetUpEnemy(MonsterContainer.Inst.GetMonsterByType(E_MinorEnemyType.SeekeroftheRainbow), new Vector3(6.5f, 0f, 0f));

        })
            .AppendInterval(0.2f)
            .AppendCallback(StartPlayerTurn);
        
    }


    private void SetUpEnemy(GameObject monsterGO, Vector3 poz)
    {
        var Monster = Instantiate(monsterGO);
        Monster.transform.position = poz;
        var MonsterBase = Monster.GetComponent<MonsterBase>();
        MonsterUnits.Add(MonsterBase);
        MonsterCount++;
    }
  
    public void StartPlayerTurn()
    {
        TurnCount++;
        FillEnergy();
        DrawCards();
        ShowMonsterIntents();
        OnPlayerTurnStart?.Invoke();
    }

    private void DrawCards()
    {
        var seq = DOTween.Sequence();
        seq.Append(HandManager.Inst.DrawCards(5)).
            Append(HandManager.Inst.DrawCards(2));
    }

    public void EndPlayerTurn()
    {
        if (!IsOnBattle) return;
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
        // ���� �� �ڷ�ƾ ����
        StartCoroutine(MonsterTurnCoroutine());
    }

    private IEnumerator MonsterTurnCoroutine()
    {
        // �ǵ��� ���� �Ⱥ��̰� ó��
        foreach (MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            mon.HideIntent();
        }

        // ���͸��� ������ ���� ����
        foreach (MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            yield return StartCoroutine(mon.StartNowPattern());
            yield return new WaitForSeconds(0.5f);
        }

        // ��� ���� ������ ���� �� ȿ�� ���� �ð� ���� �� �÷��̾� �� ����
        ReduceEffectDuration(isPlayer: false);
        StartPlayerTurn();
    }

    public void ClearCheck()
    {
        bool allMonstersDead = true;
        for (int i = 0; i < MonsterUnits.Count; i++)
        {
            if (MonsterUnits[i].isAlive())
            {
                allMonstersDead = false;
                break; 
            }
        }

        if (allMonstersDead)
        {
            ClearBattle();
        }
    }

    //��� ���� ����
    [ContextMenu("Ŭ����")]
    private void ClearBattle()
    {
        // Execute the following actions after a 3-second delay using DOTween
        DOVirtual.DelayedCall(1f, () =>
        {
            for (int i = 0; i < MonsterUnits.Count; i++)    
            {
                Destroy(MonsterUnits[i].gameObject);
            }
            IsOnBattle = false;
            ClearPlayerStatusEffects();
            HandManager.Inst.DiscardAllCardsFromHand();
            GameManager.Reward.GenerateReward(E_RewardType.Normal);
        });
    }

    private void ClearPlayerStatusEffects()
    {
        foreach(UnitBase player in PlayerUnits)
        {
            player.ClearStatusEffect();
        }
    }

    public IEnumerator MonsterAttackPlayer(float amount)
    {
        return PlayerUnits[0].GetDamageCoroutine(amount);
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
            if (player.GetHP() < lowestHealthPlayer.GetHP())
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

    public IEnumerator ReorganizeCharactersWhenDeadCoroutine()
    {
        // ����ִ� ĳ���͸� ������, ���� ĳ���͸� �ڷ� �̵���Ű�� ���� ����Ʈ�� �����մϴ�.
        PlayerUnits.Sort((a, b) => a.isAlive() == b.isAlive() ? 0 : a.isAlive() ? -1 : 1);
        Debug.Log("������ �Ϸ�");
        Debug.Log($"���� ���� �� {PlayerUnits[0].gameObject.tag}");
        foreach (UnitBase player in PlayerUnits)
        {
            Debug.Log(player.gameObject.tag + player.GetHP().ToString());
        }

        // ���ĵ� ����Ʈ�� �������� ��ġ�� ũ�⸦ �ִϸ��̼��մϴ�.
        for (int i = 0; i < PlayerUnits.Count; i++)
        {
            var unit = PlayerUnits[i];
            float targetX = -PlayerCharOffset * i;
            Vector3 targetScale = Vector3.one * (unit.isAlive() ? (i == 0 ? FrontCharSize : BackCharSize) : BackCharSize);

            // Start a coroutine to animate position and scale
            StartCoroutine(AnimatePositionAndScale(unit, targetX, targetScale, PlayerMoveDuration));
        }

        // ��� �ִϸ��̼��� �Ϸ�� ������ ���
        yield return new WaitForSeconds(PlayerMoveDuration);
    }

    private IEnumerator AnimatePositionAndScale(UnitBase unit, float targetX, Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = unit.transform.localPosition;
        Vector3 startingScale = unit.transform.localScale;
        Vector3 targetPosition = new Vector3(targetX, startingPosition.y, startingPosition.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Lerp position and scale
            unit.transform.localPosition = Vector3.Lerp(startingPosition, targetPosition, t);
            unit.transform.localScale = Vector3.Lerp(startingScale, targetScale, t);

            yield return null;
        }

        // Ensure final position and scale
        unit.transform.localPosition = targetPosition;
        unit.transform.localScale = targetScale;
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
