using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 이번 전투에서 들고 있는 돈, 유물, 카드 등을 저장할 곳
/// </summary>
public class TrialManager : MonoBehaviour
{
    public static TrialManager Inst;

    #region Relic
    public List<RelicBase> RelicList;

    private void Awake()
    {
        Inst = this;
        RelicList = new List<RelicBase>();
    }

    private void Start()
    {
        DataParser.Inst.OnCardParseEnd += () => AddMoonStone(500);

    }
    public void AddRelics(List<RelicBase> relics)
    {
        foreach (var relic in relics)
        {
            // Add the relic to the list
            RelicList.Add(relic);
            BaseUI.Inst.AddRelicIcon(relic);
            OnGetRelic(relic.RelicType);
        }
    }

    public void AddRelic(RelicBase relic)
    {
        RelicList.Add(relic);
        BaseUI.Inst.AddRelicIcon(relic);
        OnGetRelic(relic.RelicType);
    }

    public void AddRelic(E_RelicType relic)
    {
        var reli = GameManager.Card_RelicContainer.GetRelicByType(relic);
        AddRelic(reli);
    }


    public void AddStack(E_RelicType type)
    {
        var relic = RelicList.FirstOrDefault(relic => relic.RelicType == type);
        relic.Stack++;
        BaseUI.Inst.UpdateRelicStacks();
    }

    public int GetStack(E_RelicType type)
    {
        var relic = RelicList.FirstOrDefault(relic => relic.RelicType == type);
        return relic.Stack;
    }

    public void ResetStack(E_RelicType type)
    {
        var relic = RelicList.FirstOrDefault(relic => relic.RelicType == type);
        relic.Stack = 0;
        BaseUI.Inst.UpdateRelicStacks();
    }

    public bool HasRelic(E_RelicType relicType)
    {
        foreach (var relic in RelicList)
        {
            if (relic.RelicType == relicType)
            {
                return true; // Relic found
            }
        }
        return false; // Relic not found
    }

    /// <summary>
    /// 전투 시작시, 카드 사용시, 유저 턴 시작, 전투 종료 시 관련 유물 호출
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public IEnumerator ActiveRelic(E_RelicEffectTriggerType trigger)
    {
        for (int i = 0; i < RelicList.Count; i++)
        {
            RelicBase relic = RelicList[i];
            if (relic.TriggerType == trigger)
            {
                yield return StartCoroutine(RelicEffectCoroutine(relic.RelicType, i));
            }
        }
    }

    /// <summary>
    /// 전투 시작시, 카드 사용시, 유저 턴 시작, 전투 종료   전부 여기에 구현
    /// 획득시는 제외 밑에 함수 따로 있음
    /// </summary>
    /// <param name="relicType"></param>
    /// <param name="relicIndex"></param>
    /// <returns></returns>
    private IEnumerator RelicEffectCoroutine(E_RelicType relicType, int relicIndex)
    {
        var battleManager = BattleManager.Inst;

        //몇몇 유물만 작동되지 않음
        //이후 아이콘 활성화 체크용 변수
        bool activated = true;
        switch (relicType)
        {
            case E_RelicType.RubyPendant:
                yield return StartCoroutine(battleManager.GetPlayer(E_CharName.Minju).HealCoroutine(2));
                break;

            case E_RelicType.BlackRose:
                {
                    var minju = battleManager.GetPlayer(E_CharName.Minju);
                    if (minju.GetCurrentHP() <= minju.MaxHP * 0.5f)  // 민주의 체력이 50% 이하인지 확인
                    {
                        yield return StartCoroutine(minju.ApplyBuffCoroutine(E_EffectType.DarkMagic, 3));  // 흑마력 3 적용
                    }
                    else activated = false;
                }
                break;

            case E_RelicType.DarkHeartbeat:
                {
                    var nowcardColor = CardEffectManager.CurrentCardData.CardColor;
                    if (nowcardColor == E_CardColor.Black)
                    {
                        yield return StartCoroutine(battleManager.GetPlayer(E_CharName.Minju).HealCoroutine(2));
                    }
                    else activated = false;
                }
                break;

            case E_RelicType.FourGodsBox:
                yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetRandomFourGodCardData()));
                break;

            case E_RelicType.InfiniteRevolver:
                yield return StartCoroutine(Seolha.Inst.AddRandomBulletCoroutine());
                break;

            case E_RelicType.WrathElixir:
                {
                    if(CardEffectManager.CurrentCardData.CardType==E_CardType.Attack)
                    {
                        AddStack(E_RelicType.WrathElixir);
                        activated = false;
                    }

                    if (GetStack(E_RelicType.WrathElixir) == 3)
                    {
                        ResetStack(E_RelicType.WrathElixir);
                        yield return StartCoroutine(Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, 1));
                    }
                }
                break;

            case E_RelicType.BladeofExecutor:
                {
                    if (CardEffectManager.CurrentCardData.CardType == E_CardType.Attack)
                    {
                        AddStack(E_RelicType.BladeofExecutor);
                        activated = false;
                    }

                    if (GetStack(E_RelicType.BladeofExecutor) == 10)
                    {
                        ResetStack(E_RelicType.BladeofExecutor);
                        yield return StartCoroutine(Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, 1));
                    }
                }
                break;

            case E_RelicType.ScytheofGod:
                foreach (var enemy in battleManager.MonsterUnits)
                {
                    yield return StartCoroutine(enemy.ApplyBuffCoroutine(E_EffectType.ReaperMark, -1)); // Assuming -1 indicates infinite duration
                }
                break;

            case E_RelicType.BlackFeather:
                foreach (var enemy in battleManager.MonsterUnits)
                {
                    yield return StartCoroutine(enemy.ApplyBuffCoroutine(E_EffectType.ReaperMark, -1)); // Assuming -1 indicates infinite duration
                }
                break;

            case E_RelicType.ViperVenom:
                {
                    if (battleManager.EnemyDifficultyType == E_EnemyDifficultyType.Elite)
                    {
                        foreach (var mon in battleManager.MonsterUnits)
                        {
                            yield return StartCoroutine(mon.GetDamageCoroutine(mon.MaxHP / 4));
                        }
                    }
                    else activated = false;
                }
                break;


            case E_RelicType.NicePerfume:
                if (battleManager.TurnCount == 1)
                {
                    battleManager.AddEnergy(1);
                }
                else activated = false;
                break;

            case E_RelicType.FountainofLife:
                // Start of boss fights: Heal all allies for 10 HP
                if (battleManager.EnemyDifficultyType == E_EnemyDifficultyType.Boss)
                {
                    foreach (var player in battleManager.PlayerUnits)
                    {
                        StartCoroutine(player.HealCoroutine(player.MaxHP / 10));
                    }
                }
                else activated = false;
                break;

            case E_RelicType.ChocoProtein:
                foreach (var player in battleManager.PlayerUnits)
                {
                    player.MaxHP++;
                }
                break;

            case E_RelicType.TurtleHouse:
                foreach (var player in battleManager.PlayerUnits)
                {
                    StartCoroutine(player.AddBarrierCoroutine(10));
                }
                break;

            case E_RelicType.MiniMirror:
                foreach (var player in battleManager.PlayerUnits)
                {
                    StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Blessing, 5));
                }
                break;

            case E_RelicType.MiniComb:
                foreach (var player in battleManager.PlayerUnits)
                {
                    StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Crystallization, 3));
                }
                break;

            case E_RelicType.FrozenCat:
                foreach (var mon in battleManager.MonsterUnits)
                {
                    yield return StartCoroutine(mon.ApplyBuffCoroutine(E_EffectType.Freeze, 10));
                }
                break;

            default:
                activated = false;
                break;
        }

        if(activated) BaseUI.Inst.TwinkleRelicIcon(relicType);

    }

    public void OnGetRelic(E_RelicType relictype)
    {
        switch (relictype)
        {
            case E_RelicType.TreasureChest:
                AddMoonStone(500);
                break;

            case E_RelicType.PinkHairRoll:
                GameManager.UserData.AddCoreFragment(10);
                break;

            case E_RelicType.Macaron:
                foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
                {
                    player.SetHP_To_Max_WithoutVFX();
                }
                break;


        }
    }




    #endregion

    #region Reward
    public int MoonStone;
    private int targetMoonStoneAmount;
    private Coroutine currentCoroutine;

    public void AddMoonStone(int amount, float duration = 0.6f)
    {
        if (HasRelic(E_RelicType.BeautysTear)) return;
        StartCoroutine(AddMoonStoneOverTimeCoroutine(amount, duration));
    }

    private IEnumerator AddMoonStoneOverTimeCoroutine(int amount, float duration = 0.6f)
    {
        int initialGold = MoonStone;
        int targetGold = initialGold + amount;

        BaseUI.Inst.TMP_Gold.color = Color.green;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            int currentGold = Mathf.RoundToInt(Mathf.Lerp(initialGold, targetGold, t));
            int deltaGold = currentGold - MoonStone;

            MoonStone += deltaGold;
            BaseUI.Inst.UpdateUIs();

            yield return null;
        }

        MoonStone = targetGold;
        BaseUI.Inst.UpdateUIs();
        BaseUI.Inst.TMP_Gold.color = Color.white;
    }

    public void UseMoonStoneOverTime(int amount, float duration = 0.6f)
    {
        int initialGold = MoonStone;

        if (currentCoroutine != null)
        {
            // 기존 코루틴 중지 전, 현재 목표까지 즉시 갱신
            MoonStone = targetMoonStoneAmount;
            BaseUI.Inst.UpdateUIs();

            StopCoroutine(currentCoroutine);
        }

        // 새로운 목표 설정
        targetMoonStoneAmount = MoonStone - amount;

        // 코루틴 시작
        currentCoroutine = StartCoroutine(UseMoonStoneOverTimeCoroutine(duration, initialGold));
    }

    private IEnumerator UseMoonStoneOverTimeCoroutine(float duration, int initialGold)
    {
        BaseUI.Inst.TMP_Gold.color = Color.red;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            int currentGold = Mathf.RoundToInt(Mathf.Lerp(initialGold, targetMoonStoneAmount, t));
            int deltaGold = MoonStone - currentGold;

            MoonStone -= deltaGold;
            BaseUI.Inst.UpdateUIs();

            yield return null;
        }

        MoonStone = targetMoonStoneAmount;
        BaseUI.Inst.UpdateUIs();
        BaseUI.Inst.TMP_Gold.color = Color.white;

        currentCoroutine = null;
    }


    #endregion

    #region Deck
    public List<CardData> UserDeck = new List<CardData>();

    public void AddCard(CardData data)
    {
        UserDeck.Add(data);
        BaseUI.Inst.UpdateUIs();
    }

    public void RemoveCard(CardData data)
    {
        UserDeck.Remove(data);
        BaseUI.Inst.UpdateUIs();
    }

    public void AddCard(List<CardData> data)
    {
        UserDeck.AddRange(data);
        BaseUI.Inst.UpdateUIs();
    }

    #endregion
}
