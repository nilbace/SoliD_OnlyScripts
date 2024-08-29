using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;

public class InkSkillManager : MonoBehaviour
{
    public static InkSkillManager Inst;

    private Func<IEnumerator> _magentaInkSkillAction;
    private Func<IEnumerator> _cyanInkSkillAction;
    private Func<IEnumerator> _yellowInkSkillAction;

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
        _magentaInkSkillAction = GainDarkMagic;
        _cyanInkSkillAction = MultiSlashCoroutine;
        _yellowInkSkillAction = HealAllCoroutine;
    }

    public IEnumerator UseInkSkill(E_CardColor color)
    {
        switch (color)
        {
            case E_CardColor.Magenta:
                if (_magentaInkSkillAction != null)
                    yield return StartCoroutine(_magentaInkSkillAction());
                break;
            case E_CardColor.Cyan:
                if (_cyanInkSkillAction != null)
                    yield return StartCoroutine(_cyanInkSkillAction());
                break;
            case E_CardColor.Yellow:
                if (_yellowInkSkillAction != null)
                    yield return StartCoroutine(_yellowInkSkillAction());
                break;
        }
    }

    #region Minju
    private IEnumerator GainDarkMagic()
    {
        Debug.Log("�渶��1");
        var minju = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        yield return StartCoroutine(minju.ApplyBuffCoroutine(E_EffectType.DarkMagic, 1));
    }

    private IEnumerator RemoveDarkMagic()
    {
        var minju = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        yield return StartCoroutine(minju.ApplyBuffCoroutine(E_EffectType.DarkMagic, -1));
    }

    private IEnumerator Heal3()
    {
        var minju = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        yield return StartCoroutine(minju.HealCoroutine(3));
    }

    private IEnumerator MaxHP1()
    {
        var minju = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        minju.MaxHP += 1;
        yield return null;
    }

    private IEnumerator Bleed1()
    {
        var enemy = BattleManager.Inst.TargetMonster;
        yield return enemy.ApplyBuffCoroutine(E_EffectType.Bleeding, 1);
    }

    private IEnumerator AttackByBlackCard()
    {
        var enemy = BattleManager.Inst.TargetMonster;
        int amount = 0;

        foreach(var card in HandManager.Inst.CardsInMyHandList)
        {
            if (card.GetComponent<CardGO>().thisCardData.CardColor == E_CardColor.Black) amount++;
        }
        if (amount == 0) yield break;
        yield return BattleManager.Inst.TargetMonster.GetDamageCoroutine(amount);
    }


    private IEnumerator AttackByTalisman()
    {
        var enemy = BattleManager.Inst.TargetMonster;
        int amount = 0;

        foreach (var card in HandManager.Inst.CardsInMyHandList)
        {
            if (card.GetComponent<CardGO>().thisCardData.WeaponType == E_WeaponType.Talisman) amount++;
        }
        if (amount == 0) yield break;
        yield return BattleManager.Inst.TargetMonster.GetDamageCoroutine(amount);
    }


    #endregion

    #region Cyan
    private IEnumerator MultiSlashCoroutine()
    {
        var seolhaUnit = BattleManager.Inst.GetPlayer(E_CharName.Seolha);

        // ������ ������ ���ٸ� Ż��
        if (!seolhaUnit.HasBuff(E_EffectType.Blade, out BuffBase blade))
        {
            yield break;
        }

        var seq = DOTween.Sequence();
        // ������ ���ø�ŭ 3ȸ ����
        for (int i = 0; i < 3; i++)
        {
            var enemylist = BattleManager.Inst.GetProperUnits(E_CharName.Seolha, E_TargetType.AnyEnemy);
            if (enemylist.Count == 0) yield break;

            seq.AppendCallback(() =>
            {
                enemylist[0].GetDamageCoroutine(blade.Stack);
            }).AppendInterval(0.3f);

            // �� ���� ���̿� �ڷ�ƾ ��� �ð� �߰�
            yield return new WaitForSeconds(0.3f);
        }

        // DOTween �������� �Ϸ�� ������ ��ٸ��ϴ�.
        yield return seq.WaitForCompletion();
    }
    #endregion

    #region Yellow
    private IEnumerator HealAllCoroutine()
    {
        var Allies = BattleManager.Inst.PlayerUnits;
        foreach (UnitBase ally in Allies)
        {
            ally.HealCoroutine(10);
            yield return null; // �� �Ʊ��� ġ���� �� ���� �����ӱ��� ���
        }
    }
    #endregion
}
