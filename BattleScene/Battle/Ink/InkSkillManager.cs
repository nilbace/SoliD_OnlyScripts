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
    public int Index_NowMinjuInkSkill;
    public string Info_NowMinjuInkSkill;
    public int Index_NowSeolhaInkSkill;
    public string Info_NowSeolhaInkSkill;
    public int Index_nowYerinInkSkill;
    public string Info_NowYerinInkSkill;
    public string[] InkSkillInfoStrings = new string[]
{
    "흑마력을 1 획득합니다.",
    "흑마력을 1 제거합니다.",
    "HP를 3 회복합니다.",
    "최대 체력이 1 증가합니다.",
    "출혈을 1 부여합니다.",
    "패에 있는 검정 카드의 수만큼 피해를 줍니다.",
    "패에 있는 부적의 수만큼 피해를 줍니다.",
    "신수의 각인을 보유하고 있는 모든 적에게 3의 피해를 줍니다.",
    "무작위 신수의 각인을 부여합니다.",
    "날붙이를 1 획득합니다.",
    "사용한 공격카드의 수만큼 피해를 줍니다.",
    "무작위 탄환을 1발 장전합니다.",
    "헤드샷을 2 부여합니다.",
    "단검을 패에 추가합니다.",
    "탄창의 탄환을 1발 사격합니다.",
    "4의 피해를 줍니다.",
    "탄창에 보유 중인 탄환 수만큼 피해를 줍니다.",
    "출혈을 보유하고 있는 모든 적에게 3의 피해를 줍니다.",
    "체력이 가장 낮은 아군의 HP를 5 회복합니다.",
    "방어도를 2 획득합니다.",
    "축복을 1 획득합니다.",
    "에너지를 1 회복합니다.",
    "카드 1장을 드로우합니다.",
    "사신의 표식을 부여합니다.",
    "취약을 2 부여합니다.",
    "결정화를 2 획득합니다.",
    "모든 아군의 HP를 3 회복합니다."
};


    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        ChangeInkSkills();
    }

    public void ChangeInkSkills()
    {
        SetRandomMinjuInkSkill();
        SetRandomSeolhaInkSkill();
        SetRandomYerinInkSkill();
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

    public void SetRandomMinjuInkSkill()
    {
        Index_NowMinjuInkSkill = UnityEngine.Random.Range(0, 9); // 0~8 사이의 랜덤 값 생성
        Info_NowMinjuInkSkill = InkSkillInfoStrings[Index_NowMinjuInkSkill]; // 해당하는 설명 문자열 가져오기

        switch (Index_NowMinjuInkSkill)
        {
            case 0:
                _magentaInkSkillAction = GainDarkMagic;
                break;
            case 1:
                _magentaInkSkillAction = RemoveDarkMagic;
                break;
            case 2:
                _magentaInkSkillAction = Heal3;
                break;
            case 3:
                _magentaInkSkillAction = MaxHP1;
                break;
            case 4:
                _magentaInkSkillAction = Bleed1;
                break;
            case 5:
                _magentaInkSkillAction = AttackByBlackCard;
                break;
            case 6:
                _magentaInkSkillAction = AttackByTalisman;
                break;
            case 7:
                _magentaInkSkillAction = AttackAllEnemiesWithMark;
                break;
            case 8:
                _magentaInkSkillAction = GiveRandMark;
                break;
        }

    }

    private IEnumerator GainDarkMagic()
    {
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

    private IEnumerator AttackAllEnemiesWithMark()
    {
        var enemies = BattleManager.Inst.MonsterUnits;
        
        foreach(UnitBase monster in enemies)
        {
            if(monster.HasBuff(E_EffectType.BlueDragon) ||
                monster.HasBuff(E_EffectType.RedBird) ||
                monster.HasBuff(E_EffectType.WhiteTiger) ||
                monster.HasBuff(E_EffectType.BlackTortoise) )
            {
                yield return StartCoroutine(monster.GetDamageCoroutine(3));
            }
        }
    }

    private IEnumerator GiveRandMark()
    {
        int rand = UnityEngine.Random.Range(6, 10);
        E_EffectType effect = (E_EffectType)rand;
        yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(effect, 1));
    }


    #endregion

    #region Cyan

    public void SetRandomSeolhaInkSkill()
    {
        Index_NowSeolhaInkSkill = UnityEngine.Random.Range(9, 18); 
        Info_NowSeolhaInkSkill = InkSkillInfoStrings[Index_NowSeolhaInkSkill]; // 해당하는 설명 문자열 가져오기

        switch (Index_NowSeolhaInkSkill)
        {
            case 0:
                _cyanInkSkillAction = CyanSkill1;
                break;
            case 1:
                _cyanInkSkillAction = CyanSkill2;
                break;
            case 2:
                _cyanInkSkillAction = CyanSkill3;
                break;
            case 3:
                _cyanInkSkillAction = CyanSkill4;
                break;
            case 4:
                _cyanInkSkillAction = CyanSkill5;
                break;
            case 5:
                _cyanInkSkillAction = CyanSkill6;
                break;
            case 6:
                _cyanInkSkillAction = CyanSkill7;
                break;
            case 7:
                _cyanInkSkillAction = CyanSkill8;
                break;
            case 8:
                _cyanInkSkillAction = CyanSkill9;
                break;
        }

    }

    private IEnumerator CyanSkill1()
    {
        var seolha = BattleManager.Inst.GetPlayer(E_CharName.Seolha);
        yield return StartCoroutine(seolha.ApplyBuffCoroutine(E_EffectType.Blade, 1));
    }

    private IEnumerator CyanSkill2()
    {
        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(Seolha.Inst.NowTurnUsedAttackCardCount));
    }

    private IEnumerator CyanSkill3()
    {
        yield return StartCoroutine(Seolha.Inst.AddRandomBulletCoroutine());
    }

    private IEnumerator CyanSkill4()
    {
        yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.HeadShot,2));
    }

    private IEnumerator CyanSkill5()
    {
        yield return StartCoroutine(HandManager.Inst.AddCardToHandCoroutine(GameManager.Card_RelicContainer.GetCardDataByIndex(87)));
    }

    private IEnumerator CyanSkill6()
    {
        yield return StartCoroutine(Seolha.Inst.ShootBulletToTargetCoroutine());
    }

    private IEnumerator CyanSkill7()
    {
        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(4));
    }

    private IEnumerator CyanSkill8()
    {
        yield return StartCoroutine(BattleManager.Inst.TargetMonster.GetDamageCoroutine(Seolha.Inst.LoadedBulletList.Count));
    }

    private IEnumerator CyanSkill9()
    {
        var enemies = BattleManager.Inst.MonsterUnits;

        foreach (UnitBase monster in enemies)
        {
            if (monster.HasBuff(E_EffectType.Bleeding))
            {
                yield return StartCoroutine(monster.GetDamageCoroutine(3));
            }
        }
    }

    #endregion

    #region Yellow

    public void SetRandomYerinInkSkill()
    {
        Index_nowYerinInkSkill = UnityEngine.Random.Range(18, 27);
        Info_NowYerinInkSkill = InkSkillInfoStrings[Index_nowYerinInkSkill]; // 해당하는 설명 문자열 가져오기

        switch (Index_nowYerinInkSkill)
        {
            case 0:
                _yellowInkSkillAction = YellowSkill1;
                break;
            case 1:
                _yellowInkSkillAction = YellowSkill2;
                break;
            case 2:
                _yellowInkSkillAction = YellowSkill3;
                break;
            case 3:
                _yellowInkSkillAction = YellowSkill4;
                break;
            case 4:
                _yellowInkSkillAction = YellowSkill5;
                break;
            case 5:
                _yellowInkSkillAction = YellowSkill6;
                break;
            case 6:
                _yellowInkSkillAction = YellowSkill7;
                break;
            case 7:
                _yellowInkSkillAction = YellowSkill8;
                break;
            case 8:
                _yellowInkSkillAction = YellowSkill9;
                break;
        }
    }

    private IEnumerator YellowSkill1()
    {
        // 스킬 1: 체력이 가장 낮은 아군의 HP를 5 회복합니다.
        var ally = BattleManager.Inst.GetLowestHealthPlayer();
        if (ally != null)
        {
            yield return StartCoroutine(ally.HealCoroutine(5));
        }
    }

    private IEnumerator YellowSkill2()
    {
        // 스킬 2: 방어도를 2 획득합니다.
        var player = BattleManager.Inst.GetPlayer(E_CharName.Yerin);
        yield return StartCoroutine(player.AddBarrierCoroutine(2));


        // 스킬 2: 모든 아군의 HP를 3 회복합니다.
        var allies = BattleManager.Inst.PlayerUnits;
        foreach (var ally in allies)
        {
            yield return StartCoroutine(ally.HealCoroutine(3));
        }
    }

    private IEnumerator YellowSkill3()
    {
        // 스킬 3: 축복을 1 획득합니다.
        var player = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        yield return StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Blessing, 1));
    }

    private IEnumerator YellowSkill4()
    {
        BattleManager.Inst.AddEnergy(1);
        yield return null;
    }

    private IEnumerator YellowSkill5()
    {
        yield return StartCoroutine(HandManager.Inst.DrawCardsCoroutine(1));
    }

    private IEnumerator YellowSkill6()
    {
        var enemy = BattleManager.Inst.TargetMonster;
        yield return StartCoroutine(enemy.ApplyBuffCoroutine(E_EffectType.ReaperMark, 1));

    }

    private IEnumerator YellowSkill7()
    {
        var enemy = BattleManager.Inst.TargetMonster;
        yield return StartCoroutine(enemy.ApplyBuffCoroutine(E_EffectType.Vulnerability, 2));

    }

    private IEnumerator YellowSkill8()
    {
        var player = BattleManager.Inst.GetPlayer(E_CharName.Minju);
        yield return StartCoroutine(player.ApplyBuffCoroutine(E_EffectType.Crystallization, 2));

    }

    private IEnumerator YellowSkill9()
    {
        foreach (UnitBase unit in BattleManager.Inst.PlayerUnits)
        {
            if (unit.isAlive())
            {
                StartCoroutine(unit.HealCoroutine(3));
            }
        }
        yield return null;
    }

    #endregion
}
