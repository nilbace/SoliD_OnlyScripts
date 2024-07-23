using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 전투 대상, 턴, 캐릭터 이동 관리
/// </summary>

public class BattleManager : MonoBehaviour
{
    public static BattleManager Inst;
    public List<UnitBase> PlayerUnits;
    public List<MonsterBase> MonsterUnits;
    public int MonsterCount;

    //캐릭터 이동 관련
    [Tooltip("캐릭터 사이의 거리")]
    public float PlayerCharOffset;
    public float FrontCharSize;
    public float BackCharSize;
    public float PlayerMoveDuration;

    //플레이어가 조준한 몬스터
    [HideInInspector]
    public UnitBase TargetMonster;
    
    //에너지 관련
    [HideInInspector]public int EnergyAmount;
    [HideInInspector] public int NowEnergy;
    public TMPro.TMP_Text TMP_Energy;

    

    //TODO
    //몬스터 쪽은 ResetDatas에 넣어 수정 예정
    private void Awake()
    {
        Inst = this;
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
    }

    

    /// <summary>
    /// 적을 배치하고 플레이어 턴 시작하면 됨
    /// </summary>
    [ContextMenu("적 시작")]
    public void StartMinorBattle()
    {
        var seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            ResetDatas();
            SetUpEnemy(MonsterContainer.Inst.GetAnyMinorMonster(), new Vector3(5, 0f, 0f));
            
        })
            .AppendInterval(0.2f)
            .AppendCallback(RelicON)
            .AppendInterval(0.2f)
            .AppendCallback(StartPlayerTurn);
    }


    private void SetUpEnemy(GameObject monsterGO, Vector3 poz)
    {
        var Monster = Instantiate(monsterGO);
        Monster.transform.position = poz;
        var MonsterBase = Monster.GetComponent<MonsterBase>();
        MonsterUnits.Add(MonsterBase);
        MonsterBase.OnDead += () => MonsterUnits.Remove(MonsterBase);
        MonsterBase.OnDead += () => Destroy(Monster);
        MonsterBase.OnDead += () => ClearCheck();
        MonsterCount++;
    }

    public void RelicON()
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
            mon.SetIntent();
        }
    }

    public void StartMonsterTurn()
    {
        var sequence = DOTween.Sequence();
        foreach (MonsterBase mon in MonsterUnits)
        {
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
        Debug.Log("체크");
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

    public void MonsterApplyEffect_To_Player(E_EffectType effect, float amount)
    {
        PlayerUnits[0].ApplyStatusEffect(effect, amount);
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
            for (int j = 0; j < unit.ActiveEffectList.Count; j++)
            {
                EffectBase effect = unit.ActiveEffectList[j];
                effect.NextTurnStarted(unit);
            }
            unit.EffectUpdateAction?.Invoke();
        }
    }

    public UnitBase GetPlayer(E_CharName name)
    {
        return PlayerUnits.FirstOrDefault(unit => unit.tag == name.ToString());
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
                    if (unit.tag != _unit.tag) tempUnits.Add(_unit);
                }
                break;
            case E_TargetType.Self:
                tempUnits.Add(unit);
                break;

            case E_TargetType.AnyEnemy:
                if (MonsterUnits.Count > 0)
                {
                    int randomIndex = Random.Range(0,MonsterUnits.Count); // 무작위 인덱스 생성
                    tempUnits.Add(MonsterUnits[randomIndex]); // 해당 인덱스의 유닛 추가
                }
                break;
        }
        return tempUnits;
    }

    public List<UnitBase> GetProperUnits(E_CardOwner ownerName, E_TargetType targetType)
    {
        UnitBase ownerUnit = PlayerUnits.Find(unit => unit.tag == ownerName.ToString());

        if (ownerUnit != null)
        {
            return GetProperUnits(ownerUnit, targetType);
        }

        return new List<UnitBase>();
    }

    #region 캐릭터 이동 
    public Sequence MoveCharFront(E_CardOwner cardOwner)
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

    #endregion

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
    public void UpdateBattleUI()
    {
        TMP_Energy.text = $"{NowEnergy}";
    }
}
