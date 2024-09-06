using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 카드 사용시 어떤 식으로 효과가 전개될 것인지 담당하는 스크립트
/// NowCardData를 전달받고 UseCard를 실행
/// </summary>
public class CardEffectManager : MonoSingleton<CardEffectManager>
{
    public Action OnCardEffectUsed;

    public static CardData CurrentCardData;
    public static GameObject CurrentCardGO;

    private float resultDamage;

    

    //외부 호출용
    public void UseCard()
    {
        StartCoroutine(UseCardCoroutine());
    }

    /// <summary>
    /// 카드 사용을 처리하는 메인 코루틴입니다. 에너지 사용, 카드 효과 발동, 전투 로직 등을 처리합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UseCardCoroutine()
    {
        //카드에 필요한 코스트 사용
        GameManager.Battle.UseEnergy(CurrentCardData.CardCost);
        //카드 사용자가 앞으로 나섬
        GameManager.Battle.MovePlayerFront(CurrentCardData.CardOwner);

        //각 카드 효과 발동
        foreach (CardEffectData cardEffectData in CurrentCardData.CardEffectList)
        {
            //전투중이 아니라면 취소
            if (!BattleManager.Inst.IsOnBattle) yield break;

            //탈출용 트리거(30번 및 기타 효과와 연계 실패시 작동)
            bool shouldBreak = false;

            //대상 유닛들을 찾아서
            var targets = GameManager.Battle.GetProperUnits(CurrentCardData.CardOwner, cardEffectData.TargetType);

            //위 대상에 알맞은 효과를 작동
            switch (cardEffectData.CardEffectType)
            {
                //대기
                case E_EffectType.Interval:
                    yield return new WaitForSeconds(cardEffectData.Amount);
                    break;

                case E_EffectType.Damage:
                    foreach (UnitBase target in targets)
                    {
                        resultDamage = cardEffectData.Amount;
                        yield return StartCoroutine(CheckAdditionalDamageRelicCoroutine());
                        yield return StartCoroutine(CheckAdditionalDamageBuffCoroutine());

                        if(target.IsAlive())
                            yield return StartCoroutine(target.GetDamageCoroutine(resultDamage));
                    }
                    break;

                case E_EffectType.Shield:
                    foreach (UnitBase target in targets)
                    {
                        if (target.IsAlive())
                            yield return StartCoroutine(target.AddBarrierCoroutine(cardEffectData.Amount));
                    }
                    break;

                case E_EffectType.Heal:
                    foreach (UnitBase target in targets)
                    {
                        if (target.IsAlive())
                            yield return StartCoroutine(target.HealCoroutine(cardEffectData.Amount));
                    }
                    break;

            
                    //에너지 회복
                case E_EffectType.Energy:
                    BattleManager.Inst.AddEnergy(cardEffectData.Amount);
                    yield return null;
                    break;

                    //카드 드로우
                case E_EffectType.DrawCard:
                    yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine((int)cardEffectData.Amount));
                    break;

                    //랜덤 카드 손에서 버리기
                case E_EffectType.DiscardRandomCard:
                    HandManager.Inst.DiscardRandomCardFromHand();
                    yield return null;
                    break;

                    //특정 효과가 있다면 다음 로직 작동, 없다면 여기서 끝
                case E_EffectType.CheckStatusEffect:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.IsAlive() || !BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)((int)cardEffectData.Amount)))
                    {
                        shouldBreak = true;
                    }
                    break;

                //디버프가 있는지 체크하여 있다면 다음 로직 작동, 없다면 여기서 끝
                case E_EffectType.CheckHasDebuff:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.IsAlive() || !BattleManager.Inst.TargetMonster.HasDebuff())
                    {
                        shouldBreak = true;
                    }
                    break;

                    //HP가 가장 적은 아군 회복
                case E_EffectType.HealLowestHPAlly:
                    BattleManager.Inst.GetLowestHealthPlayer().HealCoroutine(cardEffectData.Amount);
                    yield return null;
                    break;

                case E_EffectType.SelfHarm:
                    yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).GetDamageCoroutine(cardEffectData.Amount));
                    yield return null;
                    break;

                    //Index번호 카드를 패에 추가함
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
                    yield return StartCoroutine(Seolha.Inst.ShootBulletToTargetCoroutine());
                    break;


                    //3번 카드
                case E_EffectType.DrainMagic:
                    {
                        if (BattleManager.Inst.TargetMonster.IsAlive() && BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bleeding);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).HealCoroutine(tempbuff.Duration));
                        }
                    }
                    break;

                    //17번 카드
                case E_EffectType.FourGodsJudgement:
                    {
                        int temp = 0;
                        for (int i = (int)E_EffectType.BlueDragon; i <= (int)E_EffectType.BlackTortoise; i++)
                        {
                            if (BattleManager.Inst.TargetMonster.IsAlive() && BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff))
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
                            if (BattleManager.Inst.TargetMonster.IsAlive() && BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff))
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
                            if(BattleManager.Inst.TargetMonster.IsAlive())
                                yield return StartCoroutine(Seolha.Inst.ShootBulletToTargetCoroutine());
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
                        foreach (BuffBase buff in BattleManager.Inst.TargetMonster.ActiveBuffList)
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
                        int temp = BattleManager.Inst.TargetMonster.ActiveBuffList.Count / 2;
                        yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).GetDamageCoroutine(temp));
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
                        foreach (BuffBase buff in BattleManager.Inst.TargetMonster.ActiveBuffList)
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
                        if (!BattleManager.Inst.TargetMonster.IsAlive())
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
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            yield return BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Bleeding, tempbuff.Duration);
                        }
                    }
                    break;

                    //85번 카드
                case E_EffectType.SilverDance:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bleeding);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).AddBarrierCoroutine(tempbuff.Duration));
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

                //각종 버프 디버프 적용
                default:
                    foreach (UnitBase target in targets)
                    {
                        if (target == null) continue;
                        #region 특수 처리
                        //배신자의 혀라면 1.5배로
                        if(TrialManager.Inst.HasRelic(E_RelicType.TraitorsTongue))
                        {
                            if (CurrentCardData.CardOwner == E_CharName.Yerin && (cardEffectData.CardEffectType == E_EffectType.Weakening || cardEffectData.CardEffectType == E_EffectType.Vulnerability)  )
                            {
                                yield return target.ApplyBuffCoroutine(cardEffectData.CardEffectType, cardEffectData.Amount*1.5f);
                                continue;
                            }
                        }

                        #endregion


                        yield return target.ApplyBuffCoroutine(cardEffectData.CardEffectType, cardEffectData.Amount);
                    }
                    break;
            }
            if (shouldBreak) break;
        }

        //공격 카드를 사용하였다면 추가로 처리할 로직
        if (CurrentCardData.CardType == E_CardType.Attack)
        {
            if (CurrentCardData.NeedTarget)
            {
                yield return StartCoroutine((BattleManager.Inst.TargetMonster as MonsterBase).AddInk(CurrentCardData.CardColor));
            }
            else
            {
                var enemies = BattleManager.Inst.GetProperUnits(CurrentCardData.CardOwner, E_TargetType.AllEnemies);
                if (enemies != null)
                {
                    foreach (UnitBase enemy in enemies)
                    {
                        yield return StartCoroutine((enemy as MonsterBase).AddInk(CurrentCardData.CardColor));
                    }
                }
            }
        }

        //카드와 연계된 유물 작동
        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnCardUse));
        OnCardEffectUsed?.Invoke();

        //카드 사용 이후 로직들

        //버리거나 소멸될 카드들 처리
        if (CurrentCardData.WillExpire)
        {
            HandManager.Inst.ExpireCardFromHand(CurrentCardGO);
        }
        else
        {
            HandManager.Inst.DiscardCardFromHand(CurrentCardGO);
        }

        //카드 사용 종료
        CardMouseDetection.IsUsing = false;
        //적이 죽었는지 확인
        BattleManager.Inst.ClearCheck();
    }

    //추가 데미지가 있는지 확인하고 추가 데미지를 계산
    private IEnumerator CheckAdditionalDamageRelicCoroutine()
    {
        var nowCardUser = GetPlayer(CurrentCardData.CardOwner);

        foreach (RelicBase relic in TrialManager.Inst.RelicList)
        {
            var relicIndex = TrialManager.Inst.RelicList.IndexOf(relic);

            switch (relic.RelicType)
            {

                case E_RelicType.BlessingofFourGods:
                    {
                        if (CurrentCardData.CardOwner == E_CharName.Minju && CurrentCardData.WeaponType == E_WeaponType.Talisman)
                        {
                            resultDamage *= 1.3f;
                            BaseUI.Inst.TwinkleRelicIcon(relicIndex);
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                    break;
            }
            
        }
    }

    //공격 시 추가 데미지를 제공하는 버프를 확인하고, 추가 데미지를 계산
    private IEnumerator CheckAdditionalDamageBuffCoroutine()
    {
        var nowCardUser = GetPlayer(CurrentCardData.CardOwner);

        foreach(BuffBase buff in nowCardUser.ActiveBuffList)
        {
            if(buff.isDealerBuff)
            {
                switch (buff.BuffType)
                {
                    //약화 디버프
                    case E_EffectType.Weakening:
                        resultDamage = resultDamage * 3 / 4;
                        nowCardUser.TwinkleBuffIcon(E_EffectType.Weakening);
                        yield return new WaitForSeconds(0.2f);
                        break;

                    //설하의 날붙이
                    case E_EffectType.Blade:
                        if (CurrentCardData.CardOwner == E_CharName.Seolha && CurrentCardData.CardCost == 0)
                        {
                            // Blade 효과를 찾고
                            var bladeEffect = nowCardUser.ActiveBuffList.FirstOrDefault(effect => effect.BuffType == E_EffectType.Blade);

                            resultDamage += bladeEffect.Stack;
                            nowCardUser.TwinkleBuffIcon(E_EffectType.Blade);
                            yield return new WaitForSeconds(0.2f);
                        }
                        break;
                   
      
                    case E_EffectType.SharpShooter:
                        if (CurrentCardData.CardOwner == E_CharName.Seolha && CurrentCardData.WeaponType == E_WeaponType.ShotGun)
                        {
                            resultDamage = resultDamage * 1.5f;
                            nowCardUser.TwinkleBuffIcon(E_EffectType.SharpShooter);
                            yield return new WaitForSeconds(0.2f);
                        }
                        break;
                  
                    case E_EffectType.DarkMagic:
                        if (CurrentCardData.CardColor == E_CardColor.Black)
                        {
                            var darkMagicStack = nowCardUser.ActiveBuffList.FirstOrDefault(effect => effect.BuffType == E_EffectType.DarkMagic);

                            resultDamage *= (1 +(darkMagicStack.Stack * 0.3f));
                            nowCardUser.TwinkleBuffIcon(E_EffectType.DarkMagic);
                            yield return new WaitForSeconds(0.2f);
                        }
                        break;
                    case E_EffectType.Strength:
                        resultDamage = resultDamage * 1.3f;
                        nowCardUser.TwinkleBuffIcon(E_EffectType.Strength);
                        yield return new WaitForSeconds(0.2f);
                        break;

                    default:
                        continue;
                }
            }
        }
    }

  
    private UnitBase GetPlayer(E_CardColor color)
    {
        return BattleManager.Inst.GetPlayer(color);
    }

    private UnitBase GetPlayer(E_CharName owner)
    {
        return BattleManager.Inst.GetPlayer(owner);
    }
}
