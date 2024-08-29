using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// 전투 대상, 턴, 캐릭터 이동 관리
/// </summary>
/// TODO  90번째줄 수정 예정

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

    //캐릭터 이동 관련
    [Tooltip("캐릭터 사이의 거리")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;
    public Transform[] PlayerPozs;

    //플레이어가 조준한 몬스터
    public UnitBase TargetMonster;
    
    //에너지 관련
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
    /// 새로운 전투가 시작될 때 호출
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
    /// 적을 배치하고 플레이어 턴 시작하면 됨
    /// TargetMonster를 비우고, MonsterUnits리스트 초기화 필요
    /// </summary>
    public IEnumerator StartBattleCoroutine()
    {
        TargetMonster = null;
        MonsterUnits = new List<MonsterBase>();
        TurnCount = 0;
        NowEnergy = 0;
        IsOnBattle = true;

        // 데이터 초기화 및 적 설정
        ResetDatas();

        //Todo 각 전투마다 적절한 규칙에 따라 적 설정하도록 변경 
        SpawnEnemy(MonsterContainer.Inst.GetMonsterByType(), new Vector3(2.5f, -0.3f, 0f));

        // 전투 시작 이벤트 트리거
        OnBattleStart?.Invoke();

        // 0.2초 대기 이후 전투 시작시 발동되는 유물들 발동
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnBattleStart));

        // 플레이어 턴 시작
        StartCoroutine(StartPlayerTurnCoroutine());
    }


    /// <summary>
    /// 적을 스폰하는 로직
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
        //전투 중이 아니거나 누를 수 없는 상황이라면 탈출
        if (!_canEndTurn || !IsOnBattle) return;

        _canEndTurn = false;

        HandManager.Inst.DiscardAllCardsFromHand();
        ReduceEffectDuration(isPlayer: true);
        StartMonsterTurn();
    }
    
    /// <summary>
    /// 해당 턴에 몬스터가 어떤 행동을 할지 의도를 보여줌
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
        // 의도를 이제 안보이게 처리
        foreach (MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            mon.HideIntent();
        }

        // 몬스터마다 예정된 패턴 실행
        foreach (MonsterBase mon in MonsterUnits)
        {
            if (!mon.isAlive()) continue;
            yield return StartCoroutine(mon.StartNowPattern());
            yield return new WaitForSeconds(0.5f);
        }

        // 모든 몬스터 패턴이 끝난 후 효과 지속 시간 감소 및 플레이어 턴 시작
        ReduceEffectDuration(isPlayer: false);
        StartCoroutine(StartPlayerTurnCoroutine());
    }

    /// <summary>
    /// 외부 호출용 함수로 전투가 끝났는지 확인
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

    [ContextMenu("클리어")]
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


    #region MonsterBase 호출용 함수 목록
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
    /// 주체와 대상 타입을 넣어서 대상(들) 반환
    /// </summary>
    /// <param name="unit">주체</param>
    /// <param name="targetType">대상</param>
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
                    // 살아있는 유닛을 찾을 때까지 반복
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, MonsterUnits.Count); // 무작위 인덱스 생성
                    } while (!MonsterUnits[randomIndex].isAlive()); // 살아있지 않으면 다시 반복

                    // 살아있는 유닛을 tempUnits에 추가
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

    #region 캐릭터 이동 관련
    public Sequence MoveCharFront(E_CharName cardOwner)
    {
        // cardOwner와 일치하는 캐릭터를 찾습니다.
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
        // cardOwner를 가장 앞자리로, 나머지는 뒤로 땡깁니다
        if (targetChar != null && targetIndex != -1)
        {
            // DOTween 시퀀스를 사용하여 애니메이션 순서를 관리합니다.
            

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

            // 애니메이션이 완료된 후 리스트의 순서를 바꿉니다.
            moveSequence.OnComplete(() =>
            {
                // targetChar를 리스트의 첫 번째로 이동합니다.
                var movedUnit = PlayerUnits[targetIndex];
                PlayerUnits.RemoveAt(targetIndex);
                PlayerUnits.Insert(0, movedUnit);
            });

            
        }

        return moveSequence;
    }

    public IEnumerator ReorganizeCharactersWhenDeadCoroutine()
    {
        // 살아있는 캐릭터를 앞으로, 죽은 캐릭터를 뒤로 이동시키기 위해 리스트를 정렬합니다.
        PlayerUnits.Sort((a, b) => a.isAlive() == b.isAlive() ? 0 : a.isAlive() ? -1 : 1);

        // 정렬된 리스트를 바탕으로 위치와 크기를 애니메이션합니다.
        for (int i = 0; i < PlayerUnits.Count; i++)
        {
            var unit = PlayerUnits[i];

            // 캐릭터의 목표 위치와 크기를 설정합니다.
            Transform targetTransform = PlayerPozs[i];
            Vector3 targetPosition = targetTransform.position;
            Vector3 targetScale = Vector3.one * (unit.isAlive() ? (i == 0 ? FrontCharSize : BackCharSize) : BackCharSize);

            // 위치와 크기를 애니메이션하기 위한 코루틴 시작
            StartCoroutine(AnimatePositionAndScale(unit, targetPosition, targetScale, PlayerMoveDuration));
        }

        // 모든 애니메이션이 완료될 때까지 대기
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
