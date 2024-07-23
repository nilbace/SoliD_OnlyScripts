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

   

    public new void Start()
    {
        base.Start();
    }

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
        attackSequence.AppendCallback(() => Dana.sprite = Dana_Attack)
                        .AppendCallback(() => DanaBallon.SetActive(true))
                        .Append(Dana.transform.DOMoveX(Dana.transform.position.x - 1, times[0]))
                        .AppendCallback(() => Dana.sprite = Dana_Normal)
                        .AppendCallback(() => Attack(5))
                        .Append(Dana.transform.DOMoveX(Dana.transform.position.x, times[1]))
                        .AppendCallback(() => DanaBallon.SetActive(false))

                        .AppendCallback(() => NitmolBallon.SetActive(true))
                        .AppendCallback(() => Nitmol.sprite = Nitmol_Attack)
                        .Append(Nitmol.transform.DOMoveX(Nitmol.transform.position.x - 1, times[2]))
                        .AppendCallback(() => Nitmol.sprite = Nitmol_Normal)
                        .AppendCallback(() => Attack(5))
                        .Append(Nitmol.transform.DOMoveX(Nitmol.transform.position.x, times[3]))
                         .AppendCallback(() => NitmolBallon.SetActive(false));
        return attackSequence;
    }

    private Sequence GetShield()
    {
        Debug.Log("¹æ¾î");
        Sequence attackSequence = DOTween.Sequence();
        attackSequence.AppendCallback(() => AddBarrier(10));

        return attackSequence;
    }
}
