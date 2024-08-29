using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime_F : MonsterBase
{
    public float moveTimeOut;
    public float moveTimeIn;
    public Sprite StandSprite;
    public Sprite AttackSprite;
    public SpriteRenderer SR_Monster;
 
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
