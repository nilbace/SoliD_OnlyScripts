using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// ���� ���, ��, ĳ���� �̵� ����
/// </summary>
/// TODO  90��°�� ���� ����

public class BattleManager : MonoSingleton<BattleManager>
{
    public List<UnitBase> PlayerUnits;
    public List<MonsterBase> MonsterUnits;
    public int MonsterCount;
    public Action OnBattleStart { get; set; }
    public Action OnBattleEnd { get; set; }
    public Action OnPlayerTurnStart { get; set; }
    public bool IsOnBattle;
    public int TurnCount;
    public bool _canEndTurn;
    public E_EnemyDifficultyType EnemyDifficultyType;

    //ĳ���� �̵� ����
    [Tooltip("ĳ���� ������ �Ÿ�")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;
    public Transform[] PlayerPozs;

    //�÷��̾ ������ ����
    public UnitBase TargetMonster;
    
    //������ ����
    [HideInInspector]public int EnergyAmount { get; set; }
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

 
    public void StartBattle()
    {
        StartCoroutine(StartBattleCoroutine());
    }

    /// <summary>
    /// ���� ��ġ�ϰ� �÷��̾� �� �����ϸ� ��
    /// TargetMonster�� ����, MonsterUnits����Ʈ �ʱ�ȭ �ʿ�
    /// </summary>
    public IEnumerator StartBattleCoroutine()
    {
        TargetMonster = null;
        MonsterUnits = new List<MonsterBase>();
        TurnCount = 0;
        NowEnergy = 0;
        IsOnBattle = true;

        // ������ �ʱ�ȭ �� �� ����
        ResetDatas();

        //Todo �� �������� ������ ��Ģ�� ���� �� �����ϵ��� ���� 
        SpawnEnemy(MonsterContainer.Inst.GetMonsterByType(), new Vector3(2.5f, -0.3f, 0f));

        // ���� ���� �̺�Ʈ Ʈ����
        OnBattleStart?.Invoke();

        // 0.2�� ��� ���� ���� ���۽� �ߵ��Ǵ� ������ �ߵ�
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnBattleStart));

        // �÷��̾� �� ����
        StartCoroutine(StartPlayerTurnCoroutine());
    }


    /// <summary>
    /// ���� �����ϴ� ����
    /// </summary>
    /// <param name="monsterGO"></param>
    /// <param name="poz"></param>
    private void SpawnEnemy(GameObject monsterGO, Vector3 poz)
    {
        var Monster = Instantiate(monsterGO);
        Monster.transform.position = poz;
        var MonsterBase = Monster.GetComponent<MonsterBase>();
        MonsterUnits.Add(MonsterBase);
        MonsterCount++;
    }
    
    public IEnumerator StartPlayerTurnCoroutine()
    {
        TurnCount++;
        FillEnergy();
        yield return DrawCards();

        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnPlayerTurnStart));

        ShowMonsterIntents();
        _canEndTurn = true;
        OnPlayerTurnStart?.Invoke();
    }

    private IEnumerator DrawCards()
    {
        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(5));

        if(GetPlayer(E_CharName.Seolha).HasBuff(E_EffectType.CombatStance, out BuffBase buff))
        {
            for(int i = 0;i<buff.Stack; i++)
                yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetCardDataByIndex(87)));
        }
        
        if (TurnCount == 1 && TrialManager.Inst.HasRelic(E_RelicType.BlueScabbard))
        {
            BaseUI.Inst.TwinkleRelicIcon(E_RelicType.BlueScabbard);
            yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetCardDataByIndex(87)));
            yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetCardDataByIndex(87)));
        }
    }

    public void EndPlayerTurn()
    {
        //���� ���� �ƴϰų� ���� �� ���� ��Ȳ�̶�� Ż��
        if (!_canEndTurn || !IsOnBattle) return;

        _canEndTurn = false;

        HandManager.Inst.DiscardAllCardsFromHand();
        ReduceEffectDuration(isPlayer: true);
        StartMonsterTurn();
    }
    
    /// <summary>
    /// �ش� �Ͽ� ���Ͱ� � �ൿ�� ���� �ǵ��� ������
    /// </summary>
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
        StartCoroutine(StartPlayerTurnCoroutine());
    }

    /// <summary>
    /// �ܺ� ȣ��� �Լ��� ������ �������� Ȯ��
    /// </summary>
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

    [ContextMenu("Ŭ����")]
    private void ClearBattle()
    {
        OnBattleEnd?.Invoke();
        StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnBattleEnd));
        DOVirtual.DelayedCall(1f, () =>
        {
            for (int i = 0; i < MonsterUnits.Count; i++)    
            {
                Destroy(MonsterUnits[i].gameObject);
            }
            IsOnBattle = false;
            ClearPlayerStatusEffects();
            HandManager.Inst.DiscardAllCardsFromHand();
            GameManager.Reward.GenerateReward(E_EnemyDifficultyType.Normal);
        });
    }

    private void ClearPlayerStatusEffects()
    {
        foreach(UnitBase player in PlayerUnits)
        {
            player.ClearStatusEffect();
        }
    }


    #region MonsterBase ȣ��� �Լ� ���
    public IEnumerator MonsterAttackPlayer(float amount)
    {
        return PlayerUnits[0].GetDamageCoroutine(amount, E_EffectType.Bleeding);
    }

    public IEnumerator MonsterApplyEffect_To_Player(E_EffectType buff, float amount)
    {
        return PlayerUnits[0].ApplyBuffCoroutine(buff, amount);
    }
    #endregion

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

    public UnitBase GetPlayer(E_CardColor color)
    {
        UnitBase player = null;
        switch (color)
        {
            case E_CardColor.Magenta:
                player = GetPlayer(E_CharName.Minju);
                break;
            case E_CardColor.Cyan:
                player = GetPlayer(E_CharName.Seolha);
                break;
            case E_CardColor.Yellow:
                player = GetPlayer(E_CharName.Yerin);
                break;
            case E_CardColor.Black:
                player = GetPlayer(E_CharName.Minju);
                break;
        }
        return player;
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

        if (tempUnits.Count == 0) return null;
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

    #region ĳ���� �̵� ����
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
            

            moveSequence.Append(targetChar.transform.DOMove(PlayerPozs[0].position, PlayerMoveDuration));
            moveSequence.Join(targetChar.transform.DOScale(FrontCharSize, PlayerMoveDuration));

            int pozIndex = 1;
            for (int i = 0; i < PlayerUnits.Count; i++)
            {
                if (i != targetIndex)
                {
                    moveSequence.Join(PlayerUnits[i].transform.DOMove(PlayerPozs[pozIndex].position, PlayerMoveDuration));
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

        // ���ĵ� ����Ʈ�� �������� ��ġ�� ũ�⸦ �ִϸ��̼��մϴ�.
        for (int i = 0; i < PlayerUnits.Count; i++)
        {
            var unit = PlayerUnits[i];

            // ĳ������ ��ǥ ��ġ�� ũ�⸦ �����մϴ�.
            Transform targetTransform = PlayerPozs[i];
            Vector3 targetPosition = targetTransform.position;
            Vector3 targetScale = Vector3.one * (unit.isAlive() ? (i == 0 ? FrontCharSize : BackCharSize) : BackCharSize);

            // ��ġ�� ũ�⸦ �ִϸ��̼��ϱ� ���� �ڷ�ƾ ����
            StartCoroutine(AnimatePositionAndScale(unit, targetPosition, targetScale, PlayerMoveDuration));
        }

        // ��� �ִϸ��̼��� �Ϸ�� ������ ���
        yield return new WaitForSeconds(PlayerMoveDuration);
    }

    private IEnumerator AnimatePositionAndScale(UnitBase unit, Vector3 targetPosition, Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = unit.transform.position;
        Vector3 startingScale = unit.transform.localScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Lerp position and scale
            unit.transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            unit.transform.localScale = Vector3.Lerp(startingScale, targetScale, t);

            yield return null;
        }

        // Ensure final position and scale
        unit.transform.position = targetPosition;
        unit.transform.localScale = targetScale;
    }



    #endregion

    #region Energy
    public void FillEnergy()
    {
        if (TrialManager.Inst.HasRelic(E_RelicType.BlessingofTopaz))
        {
            BaseUI.Inst.TwinkleRelicIcon(E_RelicType.BlessingofTopaz);
        }
        else
        {
            NowEnergy = 0;
        }

        int additionEnergy = 0;
        if (TrialManager.Inst.HasRelic(E_RelicType.HeartofEternity)) additionEnergy++;
        if (TrialManager.Inst.HasRelic(E_RelicType.BlackFeather)) additionEnergy++;
        if (TrialManager.Inst.HasRelic(E_RelicType.BeautysTear)) additionEnergy++;
        if (TrialManager.Inst.HasRelic(E_RelicType.EnergyDrink) &&
            (EnemyDifficultyType == E_EnemyDifficultyType.Elite || EnemyDifficultyType == E_EnemyDifficultyType.Boss))
            additionEnergy++;

        NowEnergy += EnergyAmount + additionEnergy;
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
