using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Inst;

    public static CardData NowCardData;
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

        // 카드 효과들을 차례대로 발동
        foreach (CardEffectData cardEffectData in NowCardData.CardEffectList)
        {
            //탈출용 플래그
            bool shouldBreak = false;

            // Interval 효과일 경우 대기
            if (cardEffectData.TargetType == E_TargetType.None && cardEffectData.CardEffectType == E_EffectType.Interval)
            {
                sequence.AppendInterval(cardEffectData.Amount);
                continue;
            }

            // 대상 타겟들을 받아오기
            var targets = GameManager.Battle.GetProperUnits(NowCardData.CardOwner, cardEffectData.TargetType);

            // 카드 효과 발동 도중에 적이 죽어서 없어진 경우
            if (targets.Count == 0)
            {
                Debug.Log("탈출");
                return; // 시퀀스를 중단
            }

            // 각 효과 타입마다 알맞은 효과를 대상에게 적용
            switch (cardEffectData.CardEffectType)
            {
                case E_EffectType.Damage:
                    foreach (UnitBase target in targets)
                    {
                        var damageAmount = cardEffectData.Amount;
                        damageAmount += AdditionalAttack();
                        sequence.AppendCallback(() => target.GetDamage(damageAmount));
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
                    if (!targets[0].HasBuff((E_BuffType)((int)cardEffectData.Amount)))
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
                        BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamage(cardEffectData.Amount);
                    });
                    break;

                #region 각종 커스텀 효과들
                case E_EffectType.AddRandomFourGods:
                    sequence.AppendCallback(() =>
                    {
                        int cardindex = Random.Range(18, 22);
                        CardData tempcard = GameManager.UserData.AllCardsList.FirstOrDefault(e => e.CardIndex == cardindex);
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
                    sequence.AppendCallback(() =>
                    {
                        Seolha.Inst.ShootBulletToTarget();
                    });
                    break;

                    //3번 카드
                case E_EffectType.DrainMagic:
                    sequence.AppendCallback(() =>
                    {
                        if(BattleManager.Inst.TargetMonster.HasBuff(E_BuffType.Bloodstain, out BuffBase tempbuff))
                        {
                            BattleManager.Inst.TargetMonster.RemoveBuff(E_BuffType.Bloodstain);
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

                            for (int i = (int)E_BuffType.BlueDragon; i <= (int)E_BuffType.BlackTortoise; i++)
                            {
                                if (BattleManager.Inst.TargetMonster.HasBuff((E_BuffType)i, out BuffBase tempbuff)) temp += i;
                            }
                            BattleManager.Inst.AddEnergy(temp);
                        });
                        sequence.AppendInterval(0.3f);
                        sequence.AppendCallback(() => { BattleManager.Inst.TargetMonster.GetDamage(temp * 6); });
                    }
                    break;

                    //24번 카드
                case E_EffectType.CatchingBreath:
                    {
                        int temp = 0;
                        sequence.AppendCallback(() =>
                        {

                            for (int i = (int)E_BuffType.BlueDragon; i <= (int)E_BuffType.BlackTortoise; i++)
                            {
                                if (BattleManager.Inst.TargetMonster.HasBuff((E_BuffType)i, out BuffBase tempbuff)) temp += i;
                            }
                            BattleManager.Inst.AddEnergy(temp);
                        });
                        sequence.AppendInterval(0.3f);
                        sequence.AppendCallback(() => { HandManager.Inst.DrawCards(temp); });
                    }
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
                        BattleManager.Inst.TargetMonster.GetDamage(temp);
                    }
                    break;

                    //42번 카드
                case E_EffectType.UnfairTrade:
                    {
                        //대상이 가진 디버프 스탯의 합만큼 피해
                        int temp = BattleManager.Inst.TargetMonster.BuffList.Count / 2;
                        sequence.
                            AppendCallback(() =>
                        {
                            BattleManager.Inst.GetPlayer(NowCardData.CardOwner).GetDamage(temp);
                            BattleManager.Inst.TargetMonster.GetDamage(temp);
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
                            if (mon.HasBuff(E_BuffType.ReaperMark))
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
                                    mon.RemoveBuff(E_BuffType.ReaperMark);
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


                    //57번 카드용
                case E_EffectType.AddRandomBlackCard:
                    sequence.AppendCallback(() => { HandManager.Inst.AddRandomBlackCard(); });
                    break;


                #endregion

                //각종 버프 디버프들
                default:
                    foreach (UnitBase target in targets)
                    {
                        if (target == null) continue;
                        sequence.AppendCallback(() => target.ApplyBuff(cardEffectData.CardEffectType, cardEffectData.Amount));
                    }
                    break;
            }

            if (shouldBreak) break;
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

        // 카드 버리기 및 상태 설정
        sequence.AppendCallback(() =>
        {
            HandManager.Inst.DiscardCardFromHand(gameObject);
            CardMouseDetection.IsUsing = false;
        });

        // 시퀀스 실행
        sequence.Play();
    }

    private float AdditionalAttack()
    {
        float damage = 0;
        if (NowCardData.CardOwner == E_CharName.Seolha && NowCardData.CardCost == 0)
        {
            var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

            // Blade 효과를 찾고 널 체크
            var bladeEffect = seolhaUnit.BuffList.FirstOrDefault(effect => effect.BuffType == E_BuffType.Blade);

            // Blade 효과가 존재하는 경우에만 스택 값을 가져옴
            if (bladeEffect != null)
            {
                damage = bladeEffect.Stack;
            }
        }


        return damage;
    }
}
