using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// ī�� ���� � ������ ȿ���� ������ ������ ����ϴ� ��ũ��Ʈ
/// NowCardData�� ���޹ް� UseCard�� ����
/// </summary>
public class CardEffectManager : MonoSingleton<CardEffectManager>
{
    public Action OnCardEffectUsed;

    public static CardData CurrentCardData;
    public static GameObject CurrentCardGO;

    private float resultDamage;

    

    //�ܺ� ȣ���
    public void UseCard()
    {
        StartCoroutine(UseCardCoroutine());
    }

    /// <summary>
    /// ī�� ����� ó���ϴ� ���� �ڷ�ƾ�Դϴ�. ������ ���, ī�� ȿ�� �ߵ�, ���� ���� ���� ó���մϴ�.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UseCardCoroutine()
    {
        //ī�忡 �ʿ��� �ڽ�Ʈ ���
        GameManager.Battle.UseEnergy(CurrentCardData.CardCost);
        //ī�� ����ڰ� ������ ����
        GameManager.Battle.MovePlayerFront(CurrentCardData.CardOwner);

        //�� ī�� ȿ�� �ߵ�
        foreach (CardEffectData cardEffectData in CurrentCardData.CardEffectList)
        {
            //�������� �ƴ϶�� ���
            if (!BattleManager.Inst.IsOnBattle) yield break;

            //Ż��� Ʈ����(30�� �� ��Ÿ ȿ���� ���� ���н� �۵�)
            bool shouldBreak = false;

            //��� ���ֵ��� ã�Ƽ�
            var targets = GameManager.Battle.GetProperUnits(CurrentCardData.CardOwner, cardEffectData.TargetType);

            //�� ��� �˸��� ȿ���� �۵�
            switch (cardEffectData.CardEffectType)
            {
                //���
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

            
                    //������ ȸ��
                case E_EffectType.Energy:
                    BattleManager.Inst.AddEnergy(cardEffectData.Amount);
                    yield return null;
                    break;

                    //ī�� ��ο�
                case E_EffectType.DrawCard:
                    yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine((int)cardEffectData.Amount));
                    break;

                    //���� ī�� �տ��� ������
                case E_EffectType.DiscardRandomCard:
                    HandManager.Inst.DiscardRandomCardFromHand();
                    yield return null;
                    break;

                    //Ư�� ȿ���� �ִٸ� ���� ���� �۵�, ���ٸ� ���⼭ ��
                case E_EffectType.CheckStatusEffect:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.IsAlive() || !BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)((int)cardEffectData.Amount)))
                    {
                        shouldBreak = true;
                    }
                    break;

                //������� �ִ��� üũ�Ͽ� �ִٸ� ���� ���� �۵�, ���ٸ� ���⼭ ��
                case E_EffectType.CheckHasDebuff:
                    if (!BattleManager.Inst.IsOnBattle || !BattleManager.Inst.TargetMonster.IsAlive() || !BattleManager.Inst.TargetMonster.HasDebuff())
                    {
                        shouldBreak = true;
                    }
                    break;

                    //HP�� ���� ���� �Ʊ� ȸ��
                case E_EffectType.HealLowestHPAlly:
                    BattleManager.Inst.GetLowestHealthPlayer().HealCoroutine(cardEffectData.Amount);
                    yield return null;
                    break;

                case E_EffectType.SelfHarm:
                    yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).GetDamageCoroutine(cardEffectData.Amount));
                    yield return null;
                    break;

                    //Index��ȣ ī�带 �п� �߰���
                case E_EffectType.MakeCardToHand:
                    {
                        CardData tempcard = GameManager.Card_RelicContainer.GetCardDataByIndex((int)cardEffectData.Amount);
                        yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(tempcard));
                    }
                    break;

                #region ���� Ŀ���� ȿ����
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


                    //3�� ī��
                case E_EffectType.DrainMagic:
                    {
                        if (BattleManager.Inst.TargetMonster.IsAlive() && BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bleeding);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).HealCoroutine(tempbuff.Duration));
                        }
                    }
                    break;

                    //17�� ī��
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

                    //24�� ī��
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

                    //33�� ī��
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

                    //36�� ī��
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

                    //38�� ī��
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

                    //39�� ī��
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

                    //42�� ī��
                case E_EffectType.UnfairTrade:
                    {
                        int temp = BattleManager.Inst.TargetMonster.ActiveBuffList.Count / 2;
                        yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).GetDamageCoroutine(temp));
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp));
                        yield return new WaitForSeconds(0.3f);
                        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(temp + 1));
                    }
                    break;

                    //43�� ī��
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

                    //53�� ī��
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

                    //57�� ī��
                case E_EffectType.AddRandomBlackCard:
                    yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetRandomBlackCardData()));
                    break;

                    //59�� ī��
                case E_EffectType.SacrificeOfBlood:
                    {
                        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(12));
                        yield return null;
                        if (!BattleManager.Inst.TargetMonster.IsAlive())
                            BattleManager.Inst.GetPlayer(E_CharName.Minju).MaxHP += 3;
                        yield return null;
                    }
                    break;

                    //62�� ī��
                case E_EffectType.Purify:
                    {
                        if (HandManager.Inst.CanDiscardBlackCard())
                        {
                            HandManager.Inst.DiscardRandomBlackCard();
                        }
                        yield return BattleManager.Inst.GetPlayer(E_CharName.Minju).ApplyBuffCoroutine(E_EffectType.DarkMagic, -1);
                    }
                    break;

                    //72�� ī��
                case E_EffectType.BloodySword:
                    {
                        BuffBase darkmagicCount;
                        BattleManager.Inst.GetPlayer(E_CharName.Minju).HasBuff(E_EffectType.DarkMagic, out darkmagicCount);
                        yield return Seolha.Inst.ApplyBuffCoroutine(E_EffectType.Blade, darkmagicCount.Stack);
                    }
                    break;

                    //82�� ī��
                case E_EffectType.RipWound:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            yield return BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Bleeding, tempbuff.Duration);
                        }
                    }
                    break;

                    //85�� ī��
                case E_EffectType.SilverDance:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bleeding, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bleeding);
                            yield return StartCoroutine(BattleManager.Inst.GetPlayer(CurrentCardData.CardOwner).AddBarrierCoroutine(tempbuff.Duration));
                        }
                    }
                    break;

                    //86�� ī��
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

                //���� ���� ����� ����
                default:
                    foreach (UnitBase target in targets)
                    {
                        if (target == null) continue;
                        #region Ư�� ó��
                        //������� ����� 1.5���
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

        //���� ī�带 ����Ͽ��ٸ� �߰��� ó���� ����
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

        //ī��� ����� ���� �۵�
        yield return StartCoroutine(TrialManager.Inst.ActiveRelic(E_RelicEffectTriggerType.OnCardUse));
        OnCardEffectUsed?.Invoke();

        //ī�� ��� ���� ������

        //�����ų� �Ҹ�� ī��� ó��
        if (CurrentCardData.WillExpire)
        {
            HandManager.Inst.ExpireCardFromHand(CurrentCardGO);
        }
        else
        {
            HandManager.Inst.DiscardCardFromHand(CurrentCardGO);
        }

        //ī�� ��� ����
        CardMouseDetection.IsUsing = false;
        //���� �׾����� Ȯ��
        BattleManager.Inst.ClearCheck();
    }

    //�߰� �������� �ִ��� Ȯ���ϰ� �߰� �������� ���
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

    //���� �� �߰� �������� �����ϴ� ������ Ȯ���ϰ�, �߰� �������� ���
    private IEnumerator CheckAdditionalDamageBuffCoroutine()
    {
        var nowCardUser = GetPlayer(CurrentCardData.CardOwner);

        foreach(BuffBase buff in nowCardUser.ActiveBuffList)
        {
            if(buff.isDealerBuff)
            {
                switch (buff.BuffType)
                {
                    //��ȭ �����
                    case E_EffectType.Weakening:
                        resultDamage = resultDamage * 3 / 4;
                        nowCardUser.TwinkleBuffIcon(E_EffectType.Weakening);
                        yield return new WaitForSeconds(0.2f);
                        break;

                    //������ ������
                    case E_EffectType.Blade:
                        if (CurrentCardData.CardOwner == E_CharName.Seolha && CurrentCardData.CardCost == 0)
                        {
                            // Blade ȿ���� ã��
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
