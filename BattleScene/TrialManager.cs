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
    private List<RelicBase> _relicList;

    private void Awake()
    {
        Inst = this;
        _relicList = new List<RelicBase>();
    }
    public void AddRelics(List<RelicBase> relics)
    {
        foreach (var relic in relics)
        {
            // Add the relic to the list
            _relicList.Add(relic);
            BaseUI.Inst.AddRelicIcon(relic);
        }
    }


    public void AddStack(E_RelicType type)
    {
        var relic = _relicList.FirstOrDefault(relic => relic.RelicType == type);
        relic.Stack++;
        BaseUI.Inst.UpdateRelicStacks();
    }

    public int GetStack(E_RelicType type)
    {
        var relic = _relicList.FirstOrDefault(relic => relic.RelicType == type);
        return relic.Stack;
    }

    public void ResetStack(E_RelicType type)
    {
        var relic = _relicList.FirstOrDefault(relic => relic.RelicType == type);
        relic.Stack = 0;
        BaseUI.Inst.UpdateRelicStacks();
    }

    public bool HasRelic(E_RelicType relicType)
    {
        foreach (var relic in _relicList)
        {
            if (relic.RelicType == relicType)
            {
                return true; // Relic found
            }
        }
        return false; // Relic not found
    }

    public IEnumerator ActiveRelic(E_RelicEffectTriggerType trigger)
    {
        for (int i = 0; i < _relicList.Count; i++)
        {
            RelicBase relic = _relicList[i];
            if (relic.TriggerType == trigger)
            {
                yield return StartCoroutine(RelicEffectCoroutine(relic.RelicType, i));
            }
        }
    }

    private IEnumerator RelicEffectCoroutine(E_RelicType relicType, int relicIndex)
    {
        var battleManager = BattleManager.Inst;
        switch (relicType)
        {
            case E_RelicType.RubyPendant:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                yield return StartCoroutine(battleManager.GetPlayer(E_CharName.Minju).HealCoroutine(2));
                break;

            case E_RelicType.BlackRose:
                {
                    var minju = battleManager.GetPlayer(E_CharName.Minju);
                    if (minju.GetHP() <= minju.MaxHP * 0.5f)  // 민주의 체력이 50% 이하인지 확인
                    {
                        BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                        yield return StartCoroutine(minju.ApplyBuffCoroutine(E_EffectType.DarkMagic, 3));  // 흑마력 3 적용
                    }
                }
                break;

            case E_RelicType.DarkHeartbeat:
                {
                    var nowcardColor = CardEffectManager.NowCardData.CardColor;
                    if (nowcardColor == E_CardColor.Black)
                    {
                        BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                        yield return StartCoroutine(battleManager.GetPlayer(E_CharName.Minju).HealCoroutine(2));
                    }
                }
                break;

            case E_RelicType.FourGodsBox:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetRandomFourGodCardData()));
                break;

            case E_RelicType.InfiniteRevolver:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                yield return StartCoroutine(Seolha.Inst.AddRandomBulletCoroutine());
                break;

            case E_RelicType.WrathElixir:
                {
                    if(CardEffectManager.NowCardData.CardType==E_CardType.Attack)
                        AddStack(E_RelicType.WrathElixir);

                    if (GetStack(E_RelicType.WrathElixir) == 3)
                    {
                        BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                        ResetStack(E_RelicType.WrathElixir);
                        yield return StartCoroutine(Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, 1));
                    }
                }
                break;

            case E_RelicType.BladeofExecutor:
                {
                    if (CardEffectManager.NowCardData.CardType == E_CardType.Attack)
                        AddStack(E_RelicType.BladeofExecutor);

                    if (GetStack(E_RelicType.BladeofExecutor) == 10)
                    {
                        BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                        ResetStack(E_RelicType.BladeofExecutor);
                        yield return StartCoroutine(Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, 1));
                    }
                }
                break;

            case E_RelicType.ScytheofGod:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex); foreach (var enemy in battleManager.MonsterUnits)
                {
                    yield return StartCoroutine(enemy.ApplyBuffCoroutine(E_EffectType.ReaperMark, -1)); // Assuming -1 indicates infinite duration
                }
                break;

            case E_RelicType.ViperVenom:
                {
                    if (battleManager.EnemyDifficultyType == EnemyDifficultyType.Elite)
                    {
                        BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                        foreach (var mon in battleManager.MonsterUnits)
                        {
                            yield return StartCoroutine(mon.GetDamageCoroutine(mon.MaxHP / 4));
                        }
                    }
                }
                break;

            case E_RelicType.Printer:
                // On acquisition, copy a card from deck
                // Implement card copy logic on relic acquisition
                break;

            case E_RelicType.NicePerfume:
                // First turn: Gain 1 energy
                if (battleManager.TurnCount == 1)
                {
                    BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                    battleManager.AddEnergy(1);
                }
                break;

            case E_RelicType.FountainofLife:
                // Start of boss fights: Heal all allies for 10 HP
                if (battleManager.EnemyDifficultyType == EnemyDifficultyType.Boss)
                {
                    BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                    foreach (var player in battleManager.PlayerUnits)
                    {
                        yield return StartCoroutine(player.HealCoroutine(player.MaxHP / 10));
                    }
                }
                break;

            case E_RelicType.ChocoProtein:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                foreach (var player in battleManager.PlayerUnits)
                {
                    player.MaxHP++;
                }
                break;

            case E_RelicType.TurtleHouse:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                foreach (var player in battleManager.PlayerUnits)
                {
                    yield return StartCoroutine(player.AddBarrierCoroutine(10));
                }
                break;

            case E_RelicType.MiniMirror:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                foreach (var player in battleManager.PlayerUnits)
                {
                    yield return StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Blessing, 5));
                }
                break;

            case E_RelicType.MiniComb:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                foreach (var player in battleManager.PlayerUnits)
                {
                    yield return StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Crystallization, 3));
                }
                break;

            case E_RelicType.FrozenCat:
                BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                foreach (var mon in battleManager.MonsterUnits)
                {
                    yield return StartCoroutine(mon.ApplyBuffCoroutine(E_EffectType.Frost, 10));
                }
                break;
        }
    }

    public void OnGetRelic()
    {

    }




    #endregion

    #region Reward
    public int MoonStone;

    #endregion
    #region Deck
    public List<CardData> UserDeck = new List<CardData>();

    #endregion
}
