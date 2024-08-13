using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker_of_the_Rainbow : MonsterBase
{
    public float moveTimeOut;
    public float moveTimeIn;
    public override void Start()
    {
        base.Start();
        ApplyBuffCoroutine(E_EffectType.LightThirst, 1);
    }

    public override IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        // Group similar effect types and handle them accordingly
        if (type == E_EffectType.Frost ||
            type == E_EffectType.Electrocution ||
            type == E_EffectType.Burn ||
            type == E_EffectType.Posion)
        {
            yield return StartCoroutine(LightEat()); // Start the LightEat coroutine
        }
        else
        {
            yield return base.ApplyBuffCoroutine(type, amount); // Default behavior
        }
    }

    private IEnumerator LightEat()
    {
        // Heal over time or instant healing
        yield return StartCoroutine(HealCoroutine(5));

        // After healing, apply another buff
        yield return StartCoroutine(ApplyBuffCoroutine(E_EffectType.Strength, 5));
    }

    public override void SetIntent()
    {
        base.SetIntent();
        SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.attack);
    }

    //public override IEnumerator StartNowPattern()
    //{
    //    return AttackSequence();
    //}

    //private Sequence AttackSequence()
    //{
    //    Sequence attackSequence = DOTween.Sequence();

    //    attackSequence.Append(transform.DOMoveX(transform.position.x - 1, moveTimeOut))
    //                     .Append(Attack(5))
    //                     .Append(transform.DOMoveX(transform.position.x, moveTimeIn));

    //    return attackSequence;
    //}
}
