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
    private void Awake()
    {
        Inst = this;
    }

    protected override void Start()
    {
        base.Start();
        BattleManager.Inst.OnBattleStart += Init;
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

    [ContextMenu("ÅºÈ¯Ãß°¡")]
    public void AddRandomBullet()
    {
        int temp = Random.Range(1, 5);
        AddBullet((E_BulletType)temp);
    }

    public void ShootBulletToTarget()
    {
        if(LoadedBulletList.Count>0)
        {
            switch (LoadedBulletList[0])
            {
                case E_BulletType.Frozen:
                    BattleManager.Inst.TargetMonster.ApplyBuff(E_BuffType.Burn, 2);
                    break;
                case E_BulletType.Electric:
                    BattleManager.Inst.TargetMonster.ApplyBuff(E_BuffType.Burn, 2);
                    break;
                case E_BulletType.Fire:
                    BattleManager.Inst.TargetMonster.ApplyBuff(E_BuffType.Burn, 2);
                    break;
                case E_BulletType.Posion:
                    BattleManager.Inst.TargetMonster.ApplyBuff(E_BuffType.Burn, 2);
                    break;
            }
            LoadedBulletList.RemoveAt(0);
            Destroy(BulletParentTR.transform.GetChild(0).gameObject);
        }
        
    }
}
