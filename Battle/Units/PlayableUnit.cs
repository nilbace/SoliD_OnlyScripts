using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayableUnit : UnitBase
{
    protected static float sharedBarrier = 0;

    public override float GetBarrier()
    {
        return sharedBarrier;
    }

    public override void AddBarrier(float barrier)
    {
        sharedBarrier += barrier;
    }
    public override void GetDamage(float amount)
    {
        if (sharedBarrier > 0)
        {
            sharedBarrier -= amount;
            if (sharedBarrier < 0)
            {
                NowHp += sharedBarrier; // ���� �������� HP�� ����
                sharedBarrier = 0;
            }
        }
        else
        {
            NowHp -= amount;
        }
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Damage, this);
        Debug.Log($"{gameObject.name} took damage. Barrier: {sharedBarrier}, HP: {NowHp}");
    }

    public override void Dead()
    {
        Debug.Log($"{gameObject.name} ���");
    }

    [ContextMenu("�� ǥ��")]
    public void ShowBarrier()
    {
        Debug.Log(sharedBarrier);
    }
    private new void Start()
    {
        base.Start();
        BattleManager.Inst.PlayerUnits.Add(this);
        if(BattleManager.Inst.PlayerUnits.Count==3)
        {
            var seq = DOTween.Sequence();
            seq.Append(BattleManager.Inst.MoveCharFront(E_CardOwner.Minju));
        }
    }
}
