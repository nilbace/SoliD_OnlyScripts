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

    public override IEnumerator ApplyBuffCoroutine(E_EffectType type, float amount)
    {
        if(type==E_EffectType.Weakening && TrialManager.Inst.HasRelic(E_RelicType.SnailHouse))
        {
            BaseUI.Inst.TwinkleRelicIcon(E_RelicType.SnailHouse);
            yield break;
        }
        else if (type == E_EffectType.Vulnerability && TrialManager.Inst.HasRelic(E_RelicType.ScarfofLizard))
        {
            BaseUI.Inst.TwinkleRelicIcon(E_RelicType.ScarfofLizard);
            yield break;
        }
        yield return base.ApplyBuffCoroutine(type, amount);
    }
    public override int GetCurrentBarrier()
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
        // 결정화 효과가 있는 경우 방어막 증가량 증가
        var resultAmount = (int)(HasBuff(E_EffectType.Crystallization) ? barrier * 1.5f : barrier);

        yield return base.AddBarrierCoroutine(resultAmount);
    }

    protected override void Awake()
    {
        base.Awake();
        
    }
    protected virtual void Start()
    {
        MaxHP = 20; SetHP_To_Max_WithoutVFX();
        BattleManager.Inst.PlayerUnits.Add(this);
        if (BattleManager.Inst.PlayerUnits.Count == 3)
        {
            BattleManager.Inst.MovePlayersToBasePoz();
        }
    }

    public override IEnumerator DeathCoroutine()
    {
        yield return StartCoroutine(base.DeathCoroutine());
        yield return StartCoroutine(BattleManager.Inst.ReorganizePlayersWhenDeadCoroutine());
    }
}
