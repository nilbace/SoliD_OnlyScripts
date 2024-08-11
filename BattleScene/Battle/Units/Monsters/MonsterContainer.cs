using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_MinorEnemyType { Yare, SeekeroftheRainbow, MaxCount}
public class MonsterContainer : MonoBehaviour
{
    public GameObject[] Minor_Monsters;

    public static MonsterContainer Inst;
    private void Awake()
    {

        Inst = this;
    }

    public GameObject GetMonsterByType(E_MinorEnemyType minorEnemyType)
    {
        return Minor_Monsters[(int)minorEnemyType];
    }

    public GameObject GetMonsterByType(int index)
    {
        return Minor_Monsters[index];
    }

    public GameObject GetAnyMinorMonster()
    {
        int rand = Random.Range(0, (int)E_MinorEnemyType.MaxCount);
        return GetMonsterByType(rand);
    }
}
