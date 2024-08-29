using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunEnergy : MonsterBase
{
    public Sprite Sprite1;
    public Sprite Sprite2;
    public SpriteRenderer SR_Monster;

    public override void Start()
    {
        base.Start();
        StartCoroutine(SwitchSprites());
    }

    private IEnumerator SwitchSprites()
    {
        while (true)
        {
            // Sprite1�� SR_Monster�� ����
            SR_Monster.sprite = Sprite1;
            yield return new WaitForSeconds(0.5f);

            // Sprite2�� SR_Monster�� ����
            SR_Monster.sprite = Sprite2;
            yield return new WaitForSeconds(0.5f);
        }
    }
    public override void SetIntent()
    {
        base.SetIntent();
        NowIntentNumber++;
        Debug.Log(NowIntentNumber);
        if (NowIntentNumber % 4 == 0)
        {
            NowIntentNumber = 0;
            SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.bless_attack);
        }
        else
        {
            
            SR_Intent.sprite = IconContainer.Inst.GetIntentIcon(E_IntentIconType.curse);
        }
    }

    public override IEnumerator StartNowPattern()
    {
        if (NowIntentNumber % 4 == 0)
        {
            return Fire();
            
        }
        else
        {
            return Charging();
        }
    }

    private IEnumerator Charging()
    {
        yield return StartCoroutine(Speech(2, "������..."));
    }

    private IEnumerator Fire()
    {
        yield return StartCoroutine(Speech(1.5f, "�¾翡������ ��������!!!!!!!"));
        yield return StartCoroutine(AttackCoroutine(50));
    }

 
}
