using DG.Tweening;
using UnityEngine;

/// <summary>
/// ������ �÷����ϴ� ����, ����, ������ ����
/// ����� ������
/// </summary>
public class PlayableUnit : UnitBase
{
    protected static float s_sharedBarrier = 0;

    public override float GetBarrier()
    {
        return s_sharedBarrier;
    }

    public override void UpdateBarrier(float amount)
    {
        s_sharedBarrier = amount;
    }

    public override void AddBarrier(float barrier)
    {
        // ����ȭ ȿ���� �ִ� ��� �� ������ ����
        float resultAmount = HasBuff(E_BuffType.Crystallization) ? barrier * 1.5f : barrier;

        // ���� �߰��ϰ� �ð� ȿ�� ����
        s_sharedBarrier += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}�� �� {resultAmount}��ŭ ȹ��");
    }
 
    
    protected virtual void Start()
    {
        MaxHP = 20;
        NowHp = 15;
        BattleManager.Inst.PlayerUnits.Add(this);
        if(BattleManager.Inst.PlayerUnits.Count==3)
        {
            var seq = DOTween.Sequence();
            seq.Append(BattleManager.Inst.MoveCharFront(E_CharName.Minju));
        }
    }
}
