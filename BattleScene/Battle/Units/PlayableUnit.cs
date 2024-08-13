using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// ������ �÷����ϴ� ����, ����, ������ ����
/// ����� ������
/// </summary>
public class PlayableUnit : UnitBase
{
    protected static int s_sharedBarrier = 0;

    public override int GetBarrier()
    {
        return s_sharedBarrier;
    }

    public override void UpdateBarrier(float amount)
    {
        var intAmount = (int)amount;
        s_sharedBarrier = intAmount;
    }

    public override IEnumerator AddBarrierCoroutine(float barrier)
    {
        // ����ȭ ȿ���� �ִ� ��� �� ������ ����
        var resultAmount = (int)(HasBuff(E_EffectType.Crystallization) ? barrier * 1.5f : barrier);

        yield return base.AddBarrierCoroutine(resultAmount);
    }
 
    
    protected virtual void Start()
    {
        MaxHP = 20; SetUpHP();
        BattleManager.Inst.PlayerUnits.Add(this);
        if(BattleManager.Inst.PlayerUnits.Count==3)
        {
            var seq = DOTween.Sequence();
            seq.Append(BattleManager.Inst.MoveCharFront(E_CharName.Minju));
        }
    }

    public override IEnumerator DeadCoroutine()
    {
        yield return StartCoroutine(base.DeadCoroutine());
        yield return StartCoroutine(BattleManager.Inst.ReorganizeCharactersWhenDeadCoroutine());
    }
}
