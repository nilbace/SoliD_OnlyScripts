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

    public override void AddBarrier(float barrier)
    {
        // ����ȭ ȿ���� �ִ� ��� �� ������ ����
        var resultAmount = (int)(HasBuff(E_EffectType.Crystallization) ? barrier * 1.5f : barrier);

        // ���� �߰��ϰ� �ð� ȿ�� ����
        s_sharedBarrier += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}�� �� {resultAmount}��ŭ ȹ��");
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
