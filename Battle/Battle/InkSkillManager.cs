using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;

public class InkSkillManager : MonoBehaviour
{
    public static InkSkillManager Inst;
    private Action _magentaInkSkillAction;
    private Action _cyanInkSkillAction;
    private Action _yellowInkSkillAction;
    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        SetupInkSkills();
    }

    private void SetupInkSkills()
    {
        _magentaInkSkillAction = Encroachment_1_AllEnemies;
        _cyanInkSkillAction = MultiSlash;
        _yellowInkSkillAction = HealAll;
    }
    public void UseInkSkill(E_CardColor color)
    {
        switch (color)
        {
            case E_CardColor.Magenta:
                _magentaInkSkillAction?.Invoke();
                break;
            case E_CardColor.Cyan:
                _cyanInkSkillAction?.Invoke();
                break;
            case E_CardColor.Yellow:
                _yellowInkSkillAction?.Invoke();
                break;
        }
    }

    #region Magenta
    private void Encroachment_1_AllEnemies()
    {
        var enemies = BattleManager.Inst.MonsterUnits;
        foreach(MonsterBase enemy in enemies)
        {
            enemy.ApplyStatusEffect(E_EffectType.Encroachment, 1);
        }
    }
    #endregion

    #region Cyan
    private void MultiSlash()
    {
        var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

        //날붙이 스택이 없다면 탈출
        if (!seolhaUnit.HasEffect(E_EffectType.Blade, out EffectBase blade))
        {
            return;
        }

        var seq = DOTween.Sequence();
        //날붙이 스택만큼 3회 공격
        for (int i = 0; i < 3; i++)
        {
            var enemylist = BattleManager.Inst.GetProperUnits(E_CardOwner.Seolha, E_TargetType.AnyEnemy);
            if (enemylist.Count == 0) return;

            seq.AppendCallback(() =>
            {
                enemylist[0].GetDamage(blade.Stack);
            }).AppendInterval(0.3f);
        }
    }

    #endregion

    #region Yellow
    private void HealAll()
    {
        var Allies = BattleManager.Inst.PlayerUnits;
        foreach(UnitBase ally in Allies)
        {
            ally.Heal(10);
        }
    }
    #endregion
}
