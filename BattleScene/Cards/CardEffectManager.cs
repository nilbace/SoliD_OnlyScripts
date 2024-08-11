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
        GameManager.Battle.UseEnergy(NowCardData.CardCost);
        GameManager.Battle.MoveCharFront(NowCardData.CardOwner);

        // DOTween ������ ����
        Sequence sequence = DOTween.Sequence();

        //������ �����ٸ� ����
        if (!BattleManager.Inst.IsOnBattle) { sequence.Kill(); return; }

        // ī�� ȿ������ ���ʴ�� �ߵ�
        foreach (CardEffectData cardEffectData in NowCardData.CardEffectList)
        {
            //������ �����ٸ� ���̻� ȿ���� �ߵ����� �ʰ� Ż��, ���� ���� ���� ���� ����
            if (!BattleManager.Inst.IsOnBattle) { sequence.Kill(); return; }

            //Ư�� ���� ������ Ż��� �÷���
            bool shouldBreak = false;

            // ��� Ÿ�ٵ��� �޾ƿ���
            var targets = GameManager.Battle.GetProperUnits(NowCardData.CardOwner, cardEffectData.TargetType);
            
            // �� ȿ�� Ÿ�Ը��� �˸��� ȿ���� ��󿡰� ����
            switch (cardEffectData.CardEffectType)
            {
                case E_EffectType.Interval:
                    sequence.AppendInterval(cardEffectData.Amount);
                    break;

                case E_EffectType.Damage:
                    foreach (UnitBase target in targets)
                    {
                        var damageAmount = cardEffectData.Amount;
                        damageAmount += AdditionalAttack();
                        sequence.AppendCallback(() => target.GetDamageCoroutine(damageAmount));
                    }
                    break;

                case E_EffectType.Shield:
                    foreach (UnitBase target in targets)
                    {
                        sequence.AppendCallback(() => target.AddBarrier(cardEffectData.Amount));
                    }
                    break;

                case E_EffectType.Heal:
                    foreach (UnitBase target in targets)
                    {
                        sequence.AppendCallback(() => target.Heal(cardEffectData.Amount));
                    }
                    break;

                case E_EffectType.Black:
                    foreach (UnitBase target in targets)
                    {
                        var mon = target as MonsterBase;
                        sequence.AppendCallback(() => mon.AddInk(E_CardColor.Black));
                    }
                    break;

                case E_EffectType.Energy:
                    sequence.AppendCallback(() => BattleManager.Inst.AddEnergy(cardEffectData.Amount));
                    break;

                case E_EffectType.DrawCard:
                    sequence.AppendCallback(() => HandManager.Inst.DrawCards((int)cardEffectData.Amount));
                    break;

                case E_EffectType.DiscardRandomCard:
                    sequence.AppendCallback(() => HandManager.Inst.DiscardRandomCardFromHand());
                    break;

                case E_EffectType.CheckStatusEffect:
                    if (!BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)((int)cardEffectData.Amount)))
                    {
                        shouldBreak = true;  // ������ ���� ��� �÷��� ����
                        break;
                    }
                    break;

                case E_EffectType.CheckHasDebuff:
                    if (!targets[0].HasDebuff())
                    {
                        shouldBreak = true;  // ������� ���� ��� �÷��� ����
                        break;
                    }
                    break;

                case E_EffectType.HealLowestHPAlly:
                    sequence.AppendCallback(() =>
                    {
                        BattleManager.Inst.GetLowestHealthPlayer().Heal(cardEffectData.Amount);
                    });
                    
                    break;

                    //TODO ���� ����Ʈ�� ����
                case E_EffectType.SelfHarm:
                    sequence.AppendCallback(() =>
                    {
                        BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamageCoroutine(cardEffectData.Amount);
                    });
                    break;

                case E_EffectType.MakeCardToHand:
                    sequence.AppendCallback(() =>
                    {
                        CardData tempcard = GameManager.CardData.AllCardsList.FirstOrDefault(e => e.CardIndex == (int)cardEffectData.Amount);
                        HandManager.Inst.AddCardToHand(tempcard);
                    });
                    break;


                #region ���� Ŀ���� ȿ����
                case E_EffectType.AddRandomFourGods:
                    sequence.AppendCallback(() =>
                    {
                        int cardindex = UnityEngine.Random.Range(18, 22);
                        CardData tempcard = GameManager.CardData.AllCardsList.FirstOrDefault(e => e.CardIndex == cardindex);
                        HandManager.Inst.AddCardToHand(tempcard);
                    });

                    break;

                case E_EffectType.AddRandomBullet:
                    sequence.AppendCallback(() =>
                    {
                        for (int i = 0; i < (int)cardEffectData.Amount; i++)    
                        {
                            Seolha.Inst.AddRandomBullet();
                        }
                    });
                    break;

                case E_EffectType.ShootBullet:
                    sequence.Append(Seolha.Inst.ShootBulletToTarget());
                    break;

                    //3�� ī��
                case E_EffectType.DrainMagic:
                    sequence.AppendCallback(() =>
                    {
                        if(BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bloodstain);
                            BattleManager.Inst.GetPlayer(NowCardData.CardOwner).Heal(tempbuff.Duration);
                        }
                        
                    });
                    break;


                    //17�� ī��
                case E_EffectType.FourGodsJudgement:
                    {
                        int temp = 0;
                        sequence.AppendCallback(() =>
                        {

                            for (int i = (int)E_EffectType.BlueDragon; i <= (int)E_EffectType.BlackTortoise; i++)
                            {
                                if (BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff)) temp += i;
                            }
                            BattleManager.Inst.AddEnergy(temp);
                        });
                        sequence.AppendInterval(0.3f);
                        sequence.AppendCallback(() => { BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp * 6); });
                    }
                    break;

                    //24�� ī��
                case E_EffectType.CatchingBreath:
                    {
                        int temp = 0;
                        sequence.AppendCallback(() =>
                        {

                            for (int i = (int)E_EffectType.BlueDragon; i <= (int)E_EffectType.BlackTortoise; i++)
                            {
                                if (BattleManager.Inst.TargetMonster.HasBuff((E_EffectType)i, out BuffBase tempbuff)) temp += i;
                            }
                            BattleManager.Inst.AddEnergy(temp);
                        });
                        sequence.AppendInterval(0.3f);
                        sequence.AppendCallback(() => { HandManager.Inst.DrawCards(temp); });
                    }
                    break;

                //33�� ī�� �Ѱ���
                case E_EffectType.AllOutAttack:
                    sequence.AppendCallback(() => {
                        int bulletCount = Seolha.Inst.LoadedBulletList.Count;
                        BattleManager.Inst.TargetMonster.GetDamageCoroutine(bulletCount * 8);
                        for (int i = 0; i < bulletCount; i++)
                        {
                            Seolha.Inst.ShootBulletToTarget();
                        }
                    });
                    break;


                //36�� ī��
                case E_EffectType.Stuntman:
                    {
                        int nowCardCount = HandManager.Inst.CardsInMyHandList.Count;
                        if (nowCardCount >= 7) break;

                        //7����� ��ο�
                        for (int i = nowCardCount; i <= 7; i++)
                        {
                            sequence.AppendCallback(() =>
                            {
                                HandManager.Inst.DrawCards(1);
                            }).AppendInterval(0.3f);

                        }
                    }
                    break;

                    //38�� ī��
                case E_EffectType.Overcome:
                    {
                        if(Seolha.Inst.LoadedBulletList.Count==0)
                        {
                            for (int i = 0; i <= 7; i++)
                            {
                                sequence.AppendCallback(() =>
                                {
                                    Seolha.Inst.AddRandomBullet();
                                }).AppendInterval(0.3f);

                            }
                        }
                    }
                    break;

                    //39�� ī��
                case E_EffectType.LastShot:
                    {
                        //����� ���� ����� ������ �ո�ŭ ����
                        int temp = 0;
                        foreach(BuffBase buff in BattleManager.Inst.TargetMonster.BuffList)
                        {
                            if (buff.Duration == -1) temp += (int)buff.Stack;
                            else temp += (int)buff.Duration;
                        }
                        BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp);
                    }
                    break;

                    //42�� ī��
                case E_EffectType.UnfairTrade:
                    {
                        int temp = BattleManager.Inst.TargetMonster.BuffList.Count / 2;
                        sequence.
                            AppendCallback(() =>
                        {
                            BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamageCoroutine(temp);
                            BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp);
                        })
                            .AppendInterval(0.3f)
                            .AppendCallback(() =>
                            {
                                HandManager.Inst.DrawCards(temp + 1);
                            });
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

                        sequence.AppendCallback(() =>
                        {
                            // Remove ReaperMark from all monsters
                            if (allMonstersWithoutReaperMark)
                            {
                                foreach (UnitBase mon in BattleManager.Inst.MonsterUnits)
                                {
                                    mon.RemoveBuff(E_EffectType.ReaperMark);
                                }
                            }

                        }).AppendInterval(0.3f)
                          .AppendCallback(() =>
                          {
                              foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
                              {
                                  player.Heal(10);
                              }
                          });
                    }
                    break;

                //49�� ī�� ������
                case E_EffectType.SoSweet:
                    sequence.AppendCallback(() => {
                        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
                        {
                            sequence.Append(player.ApplyBuff(E_EffectType.SugarRush, 1));
                        }
                    });
                    break;

                //53�� ī�� �� ���� ������~
                case E_EffectType.EZ:
                    {
                        int temp = 0;
                        foreach (BuffBase buff in BattleManager.Inst.TargetMonster.BuffList)
                        {
                            if (buff.IsDebuff) temp++;
                        }
                        Seolha.Inst.AddBarrier(temp * 3);
                    }
                    break;


                //57�� ī���
                case E_EffectType.AddRandomBlackCard:
                    sequence.AppendCallback(() => { HandManager.Inst.AddRandomBlackCard(); });
                    break;

                    //59�� ī�� ���� ����
                case E_EffectType.SacrificeOfBlood:
                    sequence.AppendCallback(() => {
                        BattleManager.Inst.TargetMonster.GetDamageCoroutine(12);
                        if (!BattleManager.Inst.TargetMonster.isAlive())
                            BattleManager.Inst.GetPlayer(E_CharName.Minju).MaxHP += 3;

                    });
                    break;

                    //62��ī�� ������ ��ȭ
                case E_EffectType.Purify:
                    sequence.AppendCallback(() =>
                    {
                        if (HandManager.Inst.CanDiscardBlackCard())
                        {
                            HandManager.Inst.DiscardRandomBlackCard();
                        }
                    }).Append(BattleManager.Inst.GetPlayer(E_CharName.Minju).ApplyBuff(E_EffectType.DarkMagic, -1));
                    break;

                    //73�� ī�� ������
                case E_EffectType.BloodySword:
                    {
                        BuffBase darkmagicCount;
                        BattleManager.Inst.GetPlayer(E_CharName.Minju).HasBuff(E_EffectType.DarkMagic, out darkmagicCount);

                        sequence.Append(Seolha.Inst.ApplyBuff(E_EffectType.Blade, darkmagicCount.Stack));
                    }
                    break;

                //82�� ī�� ��ó����
                case E_EffectType.RipWound:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Bloodstain, tempbuff.Duration));
                        }
                    }
                    break;


                //85�� ī�� ���� ȸ��
                case E_EffectType.SilverDance:
                    sequence.AppendCallback(() => {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bloodstain);
                            BattleManager.Inst.GetPlayer(NowCardData.CardOwner).AddBarrier(tempbuff.Duration);
                        }
                    });
                    break;

                    //86�� ī�� ������
                case E_EffectType.Shimai:
                    sequence.AppendCallback(() => {
                        for (int i = 0; i < Seolha.Inst.NowTurnUsedAttackCardCount; i++)
                        {
                            BattleManager.Inst.TargetMonster.GetDamageCoroutine(3);
                        }
                    });
                    break;

                #endregion

                //���� ���� �������
                default:
                    foreach (UnitBase target in targets)
                    {
                        if (target == null) continue;
                        sequence.Append(target.ApplyBuff(cardEffectData.CardEffectType, cardEffectData.Amount));
                    }
                    break;
            }
            if (shouldBreak) {  break; }
        }

        // �� ������
        if (NowCardData.CardType == E_CardType.Attack)
        {
            if (NowCardData.NeedTarget)
            {
                sequence.AppendCallback(() => (BattleManager.Inst.TargetMonster as MonsterBase).AddInk(NowCardData.CardColor));
            }
            else
            {
                var enemies = BattleManager.Inst.GetProperUnits(NowCardData.CardOwner, E_TargetType.AllEnemies);
                foreach (UnitBase enemy in enemies)
                {
                    sequence.AppendCallback(() => (enemy as MonsterBase).AddInk(NowCardData.CardColor));
                }
            }
        }

        //ī�� ��� ����
        OnCardEffectUsed?.Invoke();

        // ī�� ������ �� ���� ����
        sequence.AppendCallback(() =>
        {
            //�Ҹ� ī���� ���
            if(NowCardData.WillExpire)
            {
                HandManager.Inst.ExpireCardFromHand(NowCardGO);
            }
            //�Ҹ� ī�尡 �ƴϸ� ������ ������
            else
            {
                HandManager.Inst.DiscardCardFromHand(NowCardGO);
            }

            CardMouseDetection.IsUsing = false;
            BattleManager.Inst.ClearCheck();
        });
    }

    private float AdditionalAttack()
    {
        float damage = 0;
        if (NowCardData.CardOwner == E_CharName.Seolha && NowCardData.CardCost == 0)
        {
            var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

            // Blade ȿ���� ã�� �� üũ
            var bladeEffect = seolhaUnit.BuffList.FirstOrDefault(effect => effect.BuffType == E_EffectType.Blade);

            // Blade ȿ���� �����ϴ� ��쿡�� ���� ���� ������
            if (bladeEffect != null)
            {
                damage = bladeEffect.Stack;
            }
        }


        return damage;
    }
}
