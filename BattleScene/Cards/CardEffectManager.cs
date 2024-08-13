using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Inst;
    public Action OnCardEffectUsed;

    public static CardData NowCardData;
    public static GameObject NowCardGO;
    private void Awake()
    {
        Inst = this;
    }


    public void UseCard()
    {
        StartCoroutine(UseCardCoroutine());
    }

    private IEnumerator UseCardCoroutine()
    {
        GameManager.Battle.UseEnergy(NowCardData.CardCost);
        GameManager.Battle.MoveCharFront(NowCardData.CardOwner);

        // Exit if the battle is over
        if (!BattleManager.Inst.IsOnBattle) yield break;

        // Activate card effects sequentially
        foreach (CardEffectData cardEffectData in NowCardData.CardEffectList)
        {
            // Exit if the battle is over
            if (!BattleManager.Inst.IsOnBattle) yield break;

            // Specific condition flag to exit
            bool shouldBreak = false;

            // Get target units
            var targets = GameManager.Battle.GetProperUnits(NowCardData.CardOwner, cardEffectData.TargetType);

            // Apply the appropriate effect to targets
            switch (cardEffectData.CardEffectType)
            {
                case E_EffectType.Interval:
                    yield return new WaitForSeconds(cardEffectData.Amount);
                    break;

                case E_EffectType.Damage:
                    foreach (UnitBase target in targets)
                    {
                        var damageAmount = cardEffectData.Amount;
                        damageAmount += AdditionalAttack();
                        if(target.isAlive())
                            yield return StartCoroutine(target.GetDamageCoroutine(damageAmount));
                    }
                    break;

                case E_EffectType.Shield:
                    foreach (UnitBase target in targets)
                    {
                        if (target.isAlive())
                            yield return StartCoroutine(target.AddBarrierCoroutine(cardEffectData.Amount));
                    }
                    break;

                case E_EffectType.Heal:
                    foreach (UnitBase target in targets)
                    {
                        if (target.isAlive())
                            yield return StartCoroutine(target.HealCoroutine(cardEffectData.Amount));
                    }
                    break;

                case E_EffectType.Black:
                    foreach (UnitBase target in targets)
                    {
                        if (target.isAlive())
                        {
                            var mon = target as MonsterBase;
                            mon.AddInk(E_CardColor.Black);
                            yield return null;
                        }
                    }
                    break;

                case E_EffectType.Energy:
                    BattleManager.Inst.AddEnergy(cardEffectData.Amount);
                    yield return null;
                    break;

                case E_EffectType.DrawCard:
                    yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine((int)cardEffectData.Amount));
                    break;

                case E_EffectType.DiscardRandomCard:
                    HandManager.Inst.DiscardRandomCardFromHand();
                    yield return null;
                    break;

                case E_EffectType.CheckStatusEffect:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.isAlive() || !BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)((int)cardEffectData.Amount)))
                    {
                        shouldBreak = true;
                    }
                    break;

                case E_EffectType.CheckHasDebuff:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.isAlive() || !BattleManager.Inst.TargetMonster.HasDebuff())
                    {
                        shouldBreak = true;
                    }
                    break;

                case E_EffectType.HealLowestHPAlly:
                    BattleManager.Inst.GetLowestHealthPlayer().HealCoroutine(cardEffectData.Amount);
                    yield return null;
                    break;

                case E_EffectType.SelfHarm:
                    yield return StartCoroutine(BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamageCoroutine(cardEffectData.Amount));
                    yield return null;
                    break;

                case E_EffectType.MakeCardToHand:
                    {
                        CardData tempcard = GameManager.Card_RelicContainer.GetCardDataByIndex((int)cardEffectData.Amount);
                        yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(tempcard));
                    }
                    break;

                #region 각종 커스텀 효과들
                case E_EffectType.AddRandomFourGods:
                    {
                        CardData tempcard = GameManager.Card_RelicContainer.GetRandomFourGodCardData();
                        yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(tempcard));
                    }
                    break;

                case E_EffectType.AddRandomBullet:
                    for (int i = 0; i < (int)cardEffectData.Amount; i++)
                    {
                        yield return StartCoroutine(Seolha.Inst.AddRandomBulletCoroutine());
                    }
                    break;

                case E_EffectType.ShootBullet:
                    yield return StartCoroutine(Seolha.Inst.ShootBulletToTarget());
                    break;


                    //3번 카드
                case E_EffectType.DrainMagic:
                    {
                        if (BattleManager.Inst.TargetMonster.isAlive() && BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bloodstain);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(NowCardData.CardOwner).HealCoroutine(tempbuff.Duration));
                        }
                    }
                    break;

                    //17번 카드
                case E_EffectType.FourGodsJudgement:
                    {
                        int temp = 0;
                        for (int i = (int)E_EffectType.BlueDragon; i <= (int)E_EffectType.BlackTortoise; i++)
                        {
                            if (BattleManager.Inst.TargetMonster.isAlive() && BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff))
                                temp += i;
                        }
                        BattleManager.Inst.AddEnergy(temp);
                        yield return new WaitForSeconds(0.3f);
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp * 6));
                        yield return null;
                    }
                    break;

                    //24번 카드
                case E_EffectType.CatchingBreath:
                    {
                        int temp = 0;
                        for (int i = (int)E_EffectType.BlueDragon; i <= (int)E_EffectType.BlackTortoise; i++)
                        {
                            if (BattleManager.Inst.TargetMonster.isAlive() && BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff))
                                temp += i;
                        }
                        BattleManager.Inst.AddEnergy(temp);
                        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(temp));
                    }
                    break;

                    //33번 카드
                case E_EffectType.AllOutAttack:
                    {
                        int bulletCount = Seolha.Inst.LoadedBulletList.Count;
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(bulletCount * 8));
                        for (int i = 0; i < bulletCount; i++)
                        {
                            if(BattleManager.Inst.TargetMonster.isAlive())
                                yield return StartCoroutine(Seolha.Inst.ShootBulletToTarget());
                        }
                    }
                    break;

                    //36번 카드
                case E_EffectType.Stuntman:
                    {
                        int nowCardCount = HandManager.Inst.CardsInMyHandList.Count;
                        if (nowCardCount >= 7) break;
                        else
                        {
                            int numberOfCardsToDraw = 7 - nowCardCount;
                            yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(numberOfCardsToDraw));
                        }
                    }
                    break;

                    //38번 카드
                case E_EffectType.Overcome:
                    {
                        if (Seolha.Inst.LoadedBulletList.Count == 0)
                        {
                            for (int i = 0; i <= 7; i++)
                            {
                                StartCoroutine(Seolha.Inst.AddRandomBulletCoroutine());
                            }
                        }
                    }
                    break;

                    //39번 카드
                case E_EffectType.LastShot:
                    {
                        int temp = 0;
                        foreach (BuffBase buff in BattleManager.Inst.TargetMonster.BuffList)
                        {
                            if (buff.Duration == -1)
                                temp += (int)buff.Stack;
                            else
                                temp += (int)buff.Duration;
                        }
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp));
                        yield return null;
                    }
                    break;

                    //42번 카드
                case E_EffectType.UnfairTrade:
                    {
                        int temp = BattleManager.Inst.TargetMonster.BuffList.Count / 2;
                        yield return StartCoroutine(BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamageCoroutine(temp));
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp));
                        yield return new WaitForSeconds(0.3f);
                        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(temp + 1));
                    }
                    break;

                    //43번 카드
                case E_EffectType.LastMercy:
                    {
                        bool allMonstersWithoutReaperMark = true;

                        foreach (UnitBase mon in BattleManager.Inst.MonsterUnits)
                        {
                            if (mon.HasBuff(E_EffectType.ReaperMark))
                            {
                                allMonstersWithoutReaperMark = false;
                                break;
                            }
                        }

                        if (allMonstersWithoutReaperMark) break;

                        if (allMonstersWithoutReaperMark)
                        {
                            foreach (UnitBase mon in BattleManager.Inst.MonsterUnits)
                            {
                                mon.RemoveBuff(E_EffectType.ReaperMark);
                            }
                        }

                        yield return new WaitForSeconds(0.3f);
                        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
                        {
                            player.HealCoroutine(10);
                            yield return null;
                        }
                    }
                    break;

                    //53번 카드
                    case E_EffectType.EZ:
                    {
                        int temp = 0;
                        foreach (BuffBase buff in BattleManager.Inst.TargetMonster.BuffList)
                        {
                            if (buff.IsDebuff) temp++;
                        }
                        yield return StartCoroutine(Seolha.Inst.AddBarrierCoroutine(temp * 3));
                    }
                    break;

                    //57번 카드
                case E_EffectType.AddRandomBlackCard:
                    yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetRandomBlackCardData()));
                    break;

                    //59번 카드
                case E_EffectType.SacrificeOfBlood:
                    {
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(12));
                        yield return null;
                        if (!BattleManager.Inst.TargetMonster.isAlive())
                            BattleManager.Inst.GetPlayer(E_CharName.Minju).MaxHP += 3;
                        yield return null;
                    }
                    break;

                    //62번 카드
                case E_EffectType.Purify:
                    {
                        if (HandManager.Inst.CanDiscardBlackCard())
                        {
                            HandManager.Inst.DiscardRandomBlackCard();
                        }
                        yield return BattleManager.Inst.GetPlayer(E_CharName.Minju).ApplyBuffCoroutine(E_EffectType.DarkMagic, -1);
                    }
                    break;

                    //72번 카드
                case E_EffectType.BloodySword:
                    {
                        BuffBase darkmagicCount;
                        BattleManager.Inst.GetPlayer(E_CharName.Minju).HasBuff(E_EffectType.DarkMagic, out darkmagicCount);
                        yield return Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, darkmagicCount.Stack);
                    }
                    break;

                    //82번 카드
                case E_EffectType.RipWound:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            yield return BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Bloodstain, tempbuff.Duration);
                        }
                    }
                    break;

                    //85번 카드
                case E_EffectType.SilverDance:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bloodstain);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(NowCardData.CardOwner).AddBarrierCoroutine(tempbuff.Duration));
                        }
                    }
                    break;

                    //86번 카드
                case E_EffectType.Shimai:
                    {
                        for (int i = 0; i < Seolha.Inst.NowTurnUsedAttackCardCount; i++)
                        {
                            yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(3));
                            yield return null;
                        }
                    }
                    break;
                #endregion

                // Buff and debuff effects
                default:
                    foreach (UnitBase target in targets)
                    {
                        if (target == null) continue;
                        yield return target.ApplyBuffCoroutine(cardEffectData.CardEffectType, cardEffectData.Amount);
                    }
                    break;
            }
            if (shouldBreak) break;
        }

        // Apply ink if the card is of attack type
        if (NowCardData.CardType == E_CardType.Attack)
        {
            if (NowCardData.NeedTarget)
            {
                (BattleManager.Inst.TargetMonster as MonsterBase).AddInk(NowCardData.CardColor);
            }
            else
            {
                var enemies = BattleManager.Inst.GetProperUnits(NowCardData.CardOwner, E_TargetType.AllEnemies);
                foreach (UnitBase enemy in enemies)
                {
                    (enemy as MonsterBase).AddInk(NowCardData.CardColor);
                    yield return null;
                }
            }
        }

        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnCardUse));
        // Invoke after card use
        OnCardEffectUsed?.Invoke();

        // Discard the card and update state
        if (NowCardData.WillExpire)
        {
            HandManager.Inst.ExpireCardFromHand(NowCardGO);
        }
        else
        {
            HandManager.Inst.DiscardCardFromHand(NowCardGO);
        }

        CardMouseDetection.IsUsing = false;
        BattleManager.Inst.ClearCheck();
    }
    private float AdditionalAttack()
    {
        float damage = 0;
        if (NowCardData.CardOwner == E_CharName.Seolha && NowCardData.CardCost == 0)
        {
            var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

            // Blade 효과를 찾고 널 체크
            var bladeEffect = seolhaUnit.BuffList.FirstOrDefault(effect => effect.BuffType == E_EffectType.Blade);

            // Blade 효과가 존재하는 경우에만 스택 값을 가져옴
            if (bladeEffect != null)
            {
                damage = bladeEffect.Stack;
            }
        }


        return damage;
    }
}
