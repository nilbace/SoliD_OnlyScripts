using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyDifficultyType { Normal, Elite, Boss}

/// <summary>
/// 전투 대상, 턴, 캐릭터 이동 관리
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
    public EnemyDifficultyType EnemyDifficultyType;

    //캐릭터 이동 관련
    [Tooltip("캐릭터 사이의 거리")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;

    //플레이어가 조준한 몬스터
    public UnitBase TargetMonster;
    
    //에너지 관련
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

    /// <summary>
    /// 적을 배치하고 플레이어 턴 시작하면 됨
    /// TargetMonster를 비우고, MonsterUnits리스트 초기화 필요
    /// </summary>
    public void StartBattle()
    {
        StartCoroutine(StartBattleCoroutine());
    }

    public IEnumerator StartBattleCoroutine()
    {
        TargetMonster = null;
        MonsterUnits = new List<MonsterBase>();
        TurnCount = 0;
        IsOnBattle = true;

        // Reset data and set up enemies
        ResetDatas();
        SetUpEnemy(MonsterContainer.Inst.GetMonsterByType(E_MinorEnemyType.Yare), new Vector3(3.5f, 0f, 0f));
        // Uncomment to set up another enemy
        // SetUpEnemy(MonsterContainer.Inst.GetMonsterByType(E_MinorEnemyType.SeekeroftheRainbow), new Vector3(6.5f, 0f, 0f));

        // Trigger OnBattleStart event
        OnBattleStart?.Invoke();

        // Wait for another 0.2 seconds
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnBattleStart));

        // Start the player's turn
        StartCoroutine(StartPlayerTurnCoroutine());
    }


    private void SetUpEnemy(GameObject monsterGO, Vector3 poz)
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
        OnPlayerTurnStart?.Invoke();
    }

    private IEnumerator DrawCards()
    {
        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(5));
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
        // 몬스터 턴 코루틴 시작
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

    //모든 몬스터 삭제
    [ContextMenu("클리어")]
    private void ClearBattle()
    {
        StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnBattleEnd));
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

    public IEnumerator MonsterApplyEffect_To_Player(E_EffectType buff, float amount)
    {
        return PlayerUnits[0].ApplyBuffCoroutine(buff, amount);
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

    #region 캐릭터 이동 
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
        Debug.Log("재정렬 완료");
        Debug.Log($"현재 가장 앞 {PlayerUnits[0].gameObject.tag}");
        foreach (UnitBase player in PlayerUnits)
        {
            Debug.Log(player.gameObject.tag + player.GetHP().ToString());
        }

        // 정렬된 리스트를 바탕으로 위치와 크기를 애니메이션합니다.
        for (int i = 0; i < PlayerUnits.Count; i++)
        {
            var unit = PlayerUnits[i];
            float targetX = -PlayerCharOffset * i;
            Vector3 targetScale = Vector3.one * (unit.isAlive() ? (i == 0 ? FrontCharSize : BackCharSize) : BackCharSize);

            // Start a coroutine to animate position and scale
            StartCoroutine(AnimatePositionAndScale(unit, targetX, targetScale, PlayerMoveDuration));
        }

        // 모든 애니메이션이 완료될 때까지 대기
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
