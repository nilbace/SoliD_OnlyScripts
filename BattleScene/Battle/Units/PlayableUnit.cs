using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 유저가 플레이하는 민주, 설하, 예린의 뼈대
/// 베리어를 공유함
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
        // 결정화 효과가 있는 경우 방어막 증가량 증가
        var resultAmount = (int)(HasBuff(E_EffectType.Crystallization) ? barrier * 1.5f : barrier);

        // 방어막을 추가하고 시각 효과 생성
        s_sharedBarrier += resultAmount;
        VisualEffectManager.Inst.InstantiateEffect(E_EffectType.Shield, this);
        Debug.Log($"{gameObject.name}이 방어막 {resultAmount}만큼 획득");
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
