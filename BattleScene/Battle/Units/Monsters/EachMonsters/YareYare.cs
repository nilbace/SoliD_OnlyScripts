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
        base.SetIntent();
        NowIntentNumber = 0;
        if(NowIntentNumber == 0)
        {
            SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.attack);
        }
        else
        {
            SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.shield);
        }
    }

    public override IEnumerator StartNowPattern()
    {
        if (NowIntentNumber == 0)
        {
            return AttackPatternCoroutine();
        }
        else
        {
            return GetShieldCoroutine();
        }
    }

    private IEnumerator AttackPatternCoroutine()
    {
        yield return StartCoroutine(CreateAttackCoroutine(Dana, DanaBallon, Dana_Attack, Dana_Normal, times[0], times[1]));
        yield return StartCoroutine(CreateAttackCoroutine(Nitmol, NitmolBallon, Nitmol_Attack, Nitmol_Normal, times[2], times[3]));
    }

    private IEnumerator CreateAttackCoroutine(SpriteRenderer character, GameObject balloon, Sprite attackSprite, Sprite normalSprite, float moveTimeOut, float moveTimeIn)
    {
        character.sprite = attackSprite;
        balloon.SetActive(true);

        Vector3 startPos = character.transform.position;
        Vector3 targetPos = new Vector3(startPos.x - 1, startPos.y, startPos.z);

        yield return MoveOverTime(character.transform, targetPos, moveTimeOut);

        character.sprite = normalSprite;
        StartCoroutine(MoveOverTime(character.transform, startPos, moveTimeIn));
        yield return StartCoroutine(Attack(15));
        yield return new WaitForSeconds(moveTimeIn);
        balloon.SetActive(false);
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


    private IEnumerator GetShieldCoroutine()
    {
        yield return AddBarrierCoroutine(10);
    }
}
