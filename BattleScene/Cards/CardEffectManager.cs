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

        // DOTween 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        //전투가 끝났다면 종료
        if (!BattleManager.Inst.IsOnBattle) { sequence.Kill(); return; }

        // 카드 효과들을 차례대로 발동
        foreach (CardEffectData cardEffectData in NowCardData.CardEffectList)
        {
            //전투가 끝났다면 더이상 효과를 발동하지 않고 탈출, 이후 전투 종료 로직 실행
            if (!BattleManager.Inst.IsOnBattle) { sequence.Kill(); return; }

            //특정 조건 만족시 탈출용 플래그
            bool shouldBreak = false;

            // 대상 타겟들을 받아오기
            var targets = GameManager.Battle.GetProperUnits(NowCardData.CardOwner, cardEffectData.TargetType);
            
            // 각 효과 타입마다 알맞은 효과를 대상에게 적용
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
                        shouldBreak = true;  // 버프가 없을 경우 플래그 설정
                        break;
                    }
                    break;

                case E_EffectType.CheckHasDebuff:
                    if (!targets[0].HasDebuff())
                    {
                        shouldBreak = true;  // 디버프가 없을 경우 플래그 설정
                        break;
                    }
                    break;

                case E_EffectType.HealLowestHPAlly:
                    sequence.AppendCallback(() =>
                    {
                        BattleManager.Inst.GetLowestHealthPlayer().Heal(cardEffectData.Amount);
                    });
                    
                    break;

                    //TODO 자해 이펙트로 고정
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


                #region 각종 커스텀 효과들
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

                    //3번 카드
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


                    //17번 카드
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

                    //24번 카드
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

                //33번 카드 총공격
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


                //36번 카드
                case E_EffectType.Stuntman:
                    {
                        int nowCardCount = HandManager.Inst.CardsInMyHandList.Count;
                        if (nowCardCount >= 7) break;

                        //7장까지 드로우
                        for (int i = nowCardCount; i <= 7; i++)
                        {
                            sequence.AppendCallback(() =>
                            {
                                HandManager.Inst.DrawCards(1);
                            }).AppendInterval(0.3f);

                        }
                    }
                    break;

                    //38번 카드
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

                    //39번 카드
                case E_EffectType.LastShot:
                    {
                        //대상이 가진 디버프 스탯의 합만큼 피해
                        int temp = 0;
                        foreach(BuffBase buff in BattleManager.Inst.TargetMonster.BuffList)
                        {
                            if (buff.Duration == -1) temp += (int)buff.Stack;
                            else temp += (int)buff.Duration;
                        }
                        BattleManager.Inst.TargetMonster.GetDamageCoroutine(temp);
                    }
                    break;

                    //42번 카드
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

                //49번 카드 달콤해
                case E_EffectType.SoSweet:
                    sequence.AppendCallback(() => {
                        foreach (UnitBase player in BattleManager.Inst.PlayerUnits)
                        {
                            sequence.Append(player.ApplyBuff(E_EffectType.SugarRush, 1));
                        }
                    });
                    break;

                //53번 카드 이 정돈 껌이지~
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


                //57번 카드용
                case E_EffectType.AddRandomBlackCard:
                    sequence.AppendCallback(() => { HandManager.Inst.AddRandomBlackCard(); });
                    break;

                    //59번 카드 피의 제물
                case E_EffectType.SacrificeOfBlood:
                    sequence.AppendCallback(() => {
                        BattleManager.Inst.TargetMonster.GetDamageCoroutine(12);
                        if (!BattleManager.Inst.TargetMonster.isAlive())
                            BattleManager.Inst.GetPlayer(E_CharName.Minju).MaxHP += 3;

                    });
                    break;

                    //62번카드 생명의 정화
                case E_EffectType.Purify:
                    sequence.AppendCallback(() =>
                    {
                        if (HandManager.Inst.CanDiscardBlackCard())
                        {
                            HandManager.Inst.DiscardRandomBlackCard();
                        }
                    }).Append(BattleManager.Inst.GetPlayer(E_CharName.Minju).ApplyBuff(E_EffectType.DarkMagic, -1));
                    break;

                    //73번 카드 혈류검
                case E_EffectType.BloodySword:
                    {
                        BuffBase darkmagicCount;
                        BattleManager.Inst.GetPlayer(E_CharName.Minju).HasBuff(E_EffectType.DarkMagic, out darkmagicCount);

                        sequence.Append(Seolha.Inst.ApplyBuff(E_EffectType.Blade, darkmagicCount.Stack));
                    }
                    break;

                //82번 카드 상처찢기
                case E_EffectType.RipWound:
                    {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Bloodstain, tempbuff.Duration));
                        }
                    }
                    break;


                //85번 카드 은빛 회전
                case E_EffectType.SilverDance:
                    sequence.AppendCallback(() => {
                        if (BattleManager.Inst.TargetMonster.HasBuff(E_EffectType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_EffectType.Bloodstain);
                            BattleManager.Inst.GetPlayer(NowCardData.CardOwner).AddBarrier(tempbuff.Duration);
                        }
                    });
                    break;

                    //86번 카드 마무리
                case E_EffectType.Shimai:
                    sequence.AppendCallback(() => {
                        for (int i = 0; i < Seolha.Inst.NowTurnUsedAttackCardCount; i++)
                        {
                            BattleManager.Inst.TargetMonster.GetDamageCoroutine(3);
                        }
                    });
                    break;

                #endregion

                //각종 버프 디버프들
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

        // 색 묻히기
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

        //카드 사용 이후
        OnCardEffectUsed?.Invoke();

        // 카드 버리기 및 상태 설정
        sequence.AppendCallback(() =>
        {
            //소멸 카드일 경우
            if(NowCardData.WillExpire)
            {
                HandManager.Inst.ExpireCardFromHand(NowCardGO);
            }
            //소멸 카드가 아니면 묘지로 버려짐
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
