using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_BulletType { Basic, Frozen, Electric, Fire, Posion }

//¼³ÇÏ¿Í ÃÑÅº ½ºÅ©¸³Æ®
public class Seolha : PlayableUnit
{
    public static Seolha Inst;
    public GameObject BulletGO;
    public List<E_BulletType> LoadedBulletList;
    public Transform BulletParentTR;
    public int NowTurnUsedAttackCardCount;
    protected override void Awake()
    {
        base.Awake();
        Inst = this;
    }

    protected override void Start()
    {
        base.Start();
        BattleManager.Inst.OnBattleStart += Init;
        BattleManager.Inst.OnBattleStart += GetBlade;
        BattleManager.Inst.OnBattleEnd += RemoveAllBullets;
        CardEffectManager.Inst.OnCardEffectUsed += AddStackWhenAttackCardUsed;
        BattleManager.Inst.OnPlayerTurnStart += ResetNowTurnUsedAttackCardCount;
    }

    void GetBlade()
    {
        StartCoroutine(ApplyBuffCoroutine(E_EffectType.Weakening, 13));
        StartCoroutine(ApplyBuffCoroutine(E_EffectType.SharpShooter, 3));
        StartCoroutine(ApplyBuffCoroutine(E_EffectType.Strength, 7));
        StartCoroutine(ApplyBuffCoroutine(E_EffectType.Blade, 2));

    }

    void Init()
    {
        LoadedBulletList = new List<E_BulletType>();
    }

    private void RemoveAllBullets()
    {
        LoadedBulletList.Clear();
        foreach (Transform child in BulletParentTR)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator AddBulletCoroutine(E_BulletType bullet)
    {
        LoadedBulletList.Add(bullet);

        // Instantiate the bullet GameObject and set its parent
        GameObject bulletGO = Instantiate(BulletGO, BulletParentTR);
        bulletGO.GetComponent<Seolhabullet>().BulletType = bullet;

        // Set the color of the bullet based on its type
        switch (bullet)
        {
            case E_BulletType.Basic:
                break;
            case E_BulletType.Frozen:
                bulletGO.GetComponent<UnityEngine.UI.Image>().color = Color.blue;
                break;
            case E_BulletType.Electric:
                bulletGO.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
                break;
            case E_BulletType.Fire:
                bulletGO.GetComponent<UnityEngine.UI.Image>().color = Color.red;
                break;
            case E_BulletType.Posion:
                bulletGO.GetComponent<UnityEngine.UI.Image>().color = Color.magenta;
                break;
        }

        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator AddRandomBulletCoroutine()
    {
        int temp = Random.Range(1, 5);
        yield return StartCoroutine(AddBulletCoroutine((E_BulletType)temp));
    }


    public IEnumerator ShootBulletToTargetCoroutine()
    {
        if (LoadedBulletList.Count > 0)
        {
            E_BulletType bulletType = LoadedBulletList[0];

            int debuffAmount = TrialManager.Inst.HasRelic(E_RelicType.ReinforcedBullet) ? 4 : 2;
            // Apply the appropriate buff based on bullet type
            switch (bulletType)
            {
                case E_BulletType.Frozen:
                    yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Freeze, debuffAmount));
                    break;
                case E_BulletType.Electric:
                    yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Lightning, debuffAmount));
                    break;
                case E_BulletType.Fire:
                    yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Burn, debuffAmount));
                    break;
                case E_BulletType.Posion:
                    yield return StartCoroutine(BattleManager.Inst.TargetMonster.ApplyBuffCoroutine(E_EffectType.Posion, debuffAmount));
                    break;
            }

            if (TrialManager.Inst.HasRelic(E_RelicType.ReinforcedBullet)) BaseUI.Inst.TwinkleRelicIcon(E_RelicType.ReinforcedBullet);
            LoadedBulletList.RemoveAt(0);

            if (BulletParentTR.childCount > 0)
            {
                Destroy(BulletParentTR.GetChild(0).gameObject);
            }
        }
    }

    private void AddStackWhenAttackCardUsed()
    {
        if (CardEffectManager.CurrentCardData.CardType == E_CardType.Attack) NowTurnUsedAttackCardCount++;
    }

    private void ResetNowTurnUsedAttackCardCount()
    {
        NowTurnUsedAttackCardCount = 0;
    }
}
