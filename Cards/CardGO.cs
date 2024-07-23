using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardGO : MonoBehaviour
{
    public CardData thisCardData;
    public Sprite[] Sprites_Cost;
    public Sprite[] Sprites_Line;
    [Tooltip("모든 카드 일러스트 담아두는 곳")]
    public Sprite[] Sprites_illust;
    public SpriteRenderer SR_Cost;
    public SpriteRenderer SR_Line;
    public SpriteRenderer SR_illust;
    public TMPro.TMP_Text TMP_Name;
    public TMPro.TMP_Text TMP_Info;
    public TMPro.TMP_Text TMP_Cost;


    [ContextMenu("초기 설정")]
    public void SetCardSprite()
    {
        int index = (int)thisCardData.CardColor;
        SR_Cost.sprite = Sprites_Cost[index];
        SR_Line.sprite = Sprites_Line[index];
        Set_illust();
        TMP_Name.text = thisCardData.CardName;
        TMP_Info.text = thisCardData.CardInfoText;
        TMP_Cost.text = thisCardData.CardCost.ToString();
    }

    void Set_illust()
    {
        if (thisCardData.CardSpriteNameString == "")
        {
             return;
        }
        SR_illust.sprite = Sprites_illust.FirstOrDefault(sprite => sprite.name == thisCardData.CardSpriteNameString);
    }

    public void UseCard()
    {
        StartCoroutine(UseCardCor());
    }

    IEnumerator UseCardCor()
    {
        GameManager.Battle.UseEnergy(thisCardData.CardCost);
        GameManager.Battle.MoveCharFront(thisCardData.CardOwner);

        

        //카드 효과들을 차례대로 발동함
        foreach (CardEffectData cardEffectData in thisCardData.CardEffectList)
        {
            //Interval효과라면 그 시간만큼 대기
            if(cardEffectData.TargetType == E_TargetType.None && cardEffectData.CardEffectType == E_EffectType.Interval)
            {
                yield return new WaitForSeconds(cardEffectData.Amount);
                continue;
            }

            //대상 타겟들을 받아 와서
            var targets = GameManager.Battle.GetProperUnits(thisCardData.CardOwner, cardEffectData.TargetType);

            //카드 효과 발동 도중에 적이 죽어서 없어진 경우(임시)
            if (targets.Count == 0)
            {
                Debug.Log("탈출");
                yield break;
            }

            //각 효과 타입마다 알맞은 효과를 대상에게 적용
            switch (cardEffectData.CardEffectType)
            {
                case E_EffectType.Damage:
                    foreach (UnitBase target in targets)
                    {
                        var damageAmount = cardEffectData.Amount;
                        damageAmount += AdditionalAttack();
                        target.GetDamage(damageAmount);
                    }
                    break;
                case E_EffectType.Shield:
                    foreach(UnitBase target in targets)
                    {
                        target.AddBarrier(cardEffectData.Amount);
                    }
                    break;
               
                case E_EffectType.Heal:
                    foreach (UnitBase target in targets)
                    {
                        target.Heal(cardEffectData.Amount);
                    }
                    break;

                case E_EffectType.Black:
                    foreach (UnitBase target in targets)
                    {
                        var mon = target as MonsterBase;
                        mon.AddInk(E_CardColor.Black);
                    }
                    break;

                default:
                    foreach(UnitBase target in targets)
                    {
                        if (target == null) continue;
                        target.ApplyStatusEffect(cardEffectData.CardEffectType, cardEffectData.Amount);
                    }
                    break;
            }
        }

        //색 묻히기
        if(thisCardData.CardType==E_CardType.Attack)
        {
            if(thisCardData.NeedTarget)
            {
                (BattleManager.Inst.TargetMonster as MonsterBase).AddInk(thisCardData.CardColor);
            }
            else
            {
                var enemies = BattleManager.Inst.GetProperUnits(thisCardData.CardOwner, E_TargetType.AllEnemies);
                foreach(UnitBase enemy in enemies)
                {
                    (enemy as MonsterBase).AddInk(thisCardData.CardColor);
                }
             
            }
        }
        HandManager.Inst.DiscardCardFromHand(gameObject);
        CardMouseDetection.IsUsing = false;
    }

    private float AdditionalAttack()
    {
        float damage = 0;
        if (thisCardData.CardOwner == E_CardOwner.Seolha && thisCardData.CardCost == 0)
        {
            var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

            // Blade 효과를 찾고 널 체크
            var bladeEffect = seolhaUnit.ActiveEffectList.FirstOrDefault(effect => effect.ThisEffectType == E_EffectType.Blade);

            // Blade 효과가 존재하는 경우에만 스택 값을 가져옴
            if (bladeEffect != null)
            {
                damage = bladeEffect.Stack;
            }
        }

        return damage;
    }
}
