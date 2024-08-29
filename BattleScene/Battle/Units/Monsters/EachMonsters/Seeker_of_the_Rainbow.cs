using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker_of_the_Rainbow : MonsterBase
{
    public float moveTimeOut;
    public float moveTimeIn;
    public Sprite StandSprite;
    public Sprite AttackSprite;
    public SpriteRenderer SR_Monster;
    public override void Start()
    {
        base.Start();
        ApplyBuffCoroutine(E_EffectType.LightThirst, 1);
    }

    public override IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        // Group similar effect types and handle them accordingly
        if (type == E_EffectType.Freeze ||
            type == E_EffectType.Lightning ||
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

    public override IEnumerator StartNowPattern()
    {
        yield return StartCoroutine(AttackCoroutine(moveTimeOut, moveTimeIn));
    }

    private IEnumerator AttackCoroutine(float moveTimeOut, float moveTimeIn)
    {
        SR_Monster.sprite = AttackSprite;

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x - 2, startPos.y, startPos.z);

        yield return MoveOverTime(transform, targetPos, moveTimeOut);
        yield return new WaitForSeconds(0.2f);
        SR_Monster.sprite = StandSprite;
        StartCoroutine(MoveOverTime(transform, startPos, moveTimeIn));
        yield return StartCoroutine(AttackCoroutine(15));
        yield return new WaitForSeconds(moveTimeIn);
    }

    private IEnumerator MoveOverTime(Transform transform, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
