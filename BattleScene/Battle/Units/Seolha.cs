using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_BulletType { Basic, Frozen, Electric, Fire, Posion }

//���Ͽ� ��ź ��ũ��Ʈ
public class Seolha : PlayableUnit
{
    public static Seolha Inst;
    public GameObject BulletGO;
    public List<E_BulletType> LoadedBulletList;
    public Transform BulletParentTR;
    public int NowTurnUsedAttackCardCount;
    private void Awake()
    {
        Inst = this;
    }

    protected override void Start()
    {
        base.Start();
        BattleManager.Inst.OnBattleStart += Init;
        CardEffectManager.Inst.OnCardEffectUsed += AddStackWhenAttackCardUsed;
        BattleManager.Inst.OnPlayerTurnStart += ResetNowTurnUsedAttackCardCount;
    }

    void Init()
    {
        LoadedBulletList = new List<E_BulletType>();
    }

    public void AddBullet(E_BulletType bullet)
    {
        LoadedBulletList.Add(bullet);
        GameObject bulletGO = Instantiate(BulletGO, BulletParentTR);
        bulletGO.GetComponent<Seolhabullet>().BulletType = bullet;

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

        
    }

    [ContextMenu("źȯ�߰�")]
    public void AddRandomBullet()
    {
        int temp = Random.Range(1, 5);
        AddBullet((E_BulletType)temp);
    }

    public Sequence ShootBulletToTarget()
    {
        var sequence = DOTween.Sequence(); // ���ο� �������� �����մϴ�.

        if (LoadedBulletList.Count > 0)
        {
            E_BulletType bulletType = LoadedBulletList[0];

            // �� źȯ ������ ���� �ش� ������ �����ϴ� �������� �߰��մϴ�.
            switch (bulletType)
            {
                case E_BulletType.Frozen:
                    sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Frost, 2));
                    break;
                case E_BulletType.Electric:
                    sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Electrocution, 2));
                    break;
                case E_BulletType.Fire:
                    sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Burn, 2));
                    break;
                case E_BulletType.Posion:
                    sequence.Append(BattleManager.Inst.TargetMonster.ApplyBuff(E_EffectType.Posion, 2));
                    break;
            }

            // �������� ���� �� źȯ�� �����ϰ� ������Ʈ�� �ı��ϴ� �۾��� �߰��մϴ�.
            sequence.AppendCallback(() =>
            {
                LoadedBulletList.RemoveAt(0);
                Destroy(BulletParentTR.transform.GetChild(0).gameObject);
            });
        }

        return sequence; // �������� ��ȯ�մϴ�.
    }

    private void AddStackWhenAttackCardUsed()
    {
        if (CardEffectManager.NowCardData.CardType == E_CardType.Attack) NowTurnUsedAttackCardCount++;
    }

    private void ResetNowTurnUsedAttackCardCount()
    {
        NowTurnUsedAttackCardCount = 0;
    }
}
