using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class YareYare : MonsterBase
{
    public Sprite Dana_Attack;
    public Sprite Dana_Normal;
    public Sprite Nitmol_Attack;
    public Sprite Nitmol_Normal;
    public SpriteRenderer Dana;
    public SpriteRenderer Nitmol;
    public GameObject DanaBallon;
    public GameObject NitmolBallon;
    public float[] times;

   

    public override void SetIntent()
    {
        NowIntentNumber = Random.Range(0, 2);
        if(NowIntentNumber == 0)
        {
            SR_Intent.sprite = BattleIconManager.Inst.GetIntentIcon(E_IntentIconType.Attack);
        }
        else
        {
            SR_Intent.sprite = BattleIconManager.Inst.GetIntentIcon(E_IntentIconType.Shield);
        }
    }

    public override Sequence GetSequenceByIntent()
    {
        if (NowIntentNumber == 0) return AttackSequence();

        return GetShield();
    }

    private Sequence AttackSequence()
    {
        Sequence attackSequence = DOTween.Sequence();

        attackSequence.Append(CreateAttackSequence(Dana, DanaBallon, Dana_Attack, Dana_Normal, times[0], times[1]))
                      .Append(CreateAttackSequence(Nitmol, NitmolBallon, Nitmol_Attack, Nitmol_Normal, times[2], times[3]));

        return attackSequence;
    }

    private Sequence CreateAttackSequence(SpriteRenderer character, GameObject balloon, Sprite attackSprite, Sprite normalSprite, float moveTimeOut, float moveTimeIn)
    {
        Sequence characterSequence = DOTween.Sequence();

        characterSequence.AppendCallback(() => character.sprite = attackSprite)
                         .AppendCallback(() => balloon.SetActive(true))
                         .Append(character.transform.DOMoveX(character.transform.position.x - 1, moveTimeOut))
                         .AppendCallback(() => character.sprite = normalSprite)
                         .AppendCallback(() => Attack(5))
                         .Append(character.transform.DOMoveX(character.transform.position.x, moveTimeIn))
                         .AppendCallback(() => balloon.SetActive(false));

        return characterSequence;
    }


    private Sequence GetShield()
    {
        Debug.Log("¹æ¾î");
        Sequence attackSequence = DOTween.Sequence();
        attackSequence.AppendCallback(() => AddBarrier(10));

        return attackSequence;
    }
}
